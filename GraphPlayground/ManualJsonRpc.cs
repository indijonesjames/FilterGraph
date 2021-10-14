using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GraphPlayground
{
    public class ManualJsonRpc
    {
        TcpClient client;
        int nextSequence = 1;
        Dictionary<int, TaskCompletionSource<JToken>> pending = new Dictionary<int, TaskCompletionSource<JToken>>();
        List<byte> accumulator = new List<byte>();

        // Ugh.  Let's hard code this for now.
        public event Action<string, string> Notify;

        public async Task<bool> Connect(string host, int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            doRead();
            return true;
        }

        public async Task<T> Call<T>(string method, JObject @params = null) {
            JToken result = await Call(method, @params);
            return result.ToObject<T>();
        }

        public Task<JToken> Call(string method, JObject @params = null)
        {
            // put together the JsonRpc call object;
            JObject call = new JObject();
            call["method"] = method;
            int id = nextSequence++;
            call["id"] = id;
            if (@params != null) {
                call["params"] = @params;
            }
            string json = call.ToString();
            send(json);
            TaskCompletionSource<JToken> source = new TaskCompletionSource<JToken>();
            pending.Add(id, source);
            return source.Task;
        }

        void send(string message)
        {
            client.Client.Send(new List<ArraySegment<byte>>() {
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                    new ArraySegment<byte>(new byte[] { 0 })
                   });
        }

        private void doRead()
        {
            byte[] buffer = new byte[4096];
            try {
                client.Client.BeginReceive(buffer, 0, 1024, SocketFlags.None, new AsyncCallback(ar => {
                    try {
                        if (client.Client == null) return;
                        int cbRead = client.Client.EndReceive(ar);
                        if (cbRead == 0) return; // Disconnected
                        processBytes(buffer, cbRead);
                        doRead();
                    } catch (Exception) {
                    }
                }), null);
            } catch (Exception) {
                // We've disconnected.
            }
        }


        private void processMessage(JObject msg)
        {
            // does this complete something?
            if (msg.ContainsKey("error")) {
                // it's a failure
                JToken result = (JToken)msg["result"];
                if (msg.ContainsKey("id")) {
                    int id = (int)msg["id"];

                    if (pending.TryGetValue(id, out TaskCompletionSource<JToken> source)) {
                        source.SetException(new Exception((string)msg["error"]["message"]));
                        pending.Remove(id);
                    }
                }
            } else if (msg.ContainsKey("result")) {
                // it's a success
                JToken result = (JToken)msg["result"];
                if (msg.ContainsKey("id")) {
                    int id = (int)msg["id"];

                    if (pending.TryGetValue(id, out TaskCompletionSource<JToken> source)) {
                        source.SetResult(result);
                        pending.Remove(id);
                    }
                }
            } else if (msg.ContainsKey("method")) {
                // it's a call
                // TODO: factor this out of this file
                // by adding an RPC target.
                string method = (string)msg["method"];
                if (method == "notify") {
                    JToken jId = msg["params"]["filterId"];
                    JToken jValue = msg["params"]["value"];
                    string id = jId.ToObject<string>();
                    string str = null;
                    if (jValue != null) {
                        str = jValue.ToString();
                    }
                    if (this.Notify != null) {
                        this.Notify(id, str);
                    }
                }
            }
        }

        private void processMessage(byte[] buf)
        {
            string json = Encoding.UTF8.GetString(buf);
            JObject msg = JObject.Parse(json);
            processMessage(msg);
        }
        private void processByte(byte b)
        {
            if (b == 0) {
                processMessage(accumulator.ToArray());
                accumulator.Clear();
            } else {
                accumulator.Add(b);
            }
        }

        private void processBytes(byte[] buf, int len)
        {
            for (int i = 0; i < len; ++i) {
                processByte(buf[i]);
            }
        }

        internal void Close()
        {
            client.Close();
        }
    }
}
