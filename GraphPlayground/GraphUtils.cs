using GraphLayoutLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GraphPlayground
{
    internal class GraphUtils
    {
        internal async static Task<string> Test(ManualJsonRpc rpc, GraphDesign graphDesign)
        {
            string graphId = await rpc.Call<string>("createGraph");

            //JObject p = new JObject();
            //p["a"] = 3;
            //p["b"] = 4;
            //var result = await rpc.Call<int>("add", p);

            StringBuilder metas = new StringBuilder();
            Dictionary<object, string> ids = new Dictionary<object, string>();
            JObject args = null;

            foreach (var node in graphDesign.nodeDesigns) {
                args = new JObject();
                args["type"] = node.name;
                JToken meta = await rpc.Call("requireFilter", args);
                metas.Append(meta.ToString() + "; ");

                args = new JObject();
                args["graphId"] = graphId;
                args["type"] = node.name;
                if (node.@params != null) args["params"] = JToken.Parse(node.@params);
                string filterId = await rpc.Call<string>("createFilter", args);
                ids.Add(node, filterId);
            }

            foreach (var connection in graphDesign.pinConnectionDesigns) {
                args = new JObject();
                JObject upstream = new JObject();
                JObject downstream = new JObject();
                
                upstream["filterId"] = ids[FindNodeForPin(graphDesign, connection.outputPinDesign)];
                upstream["pinName"] = connection.outputPinDesign.name;
                downstream["filterId"] = ids[FindNodeForPin(graphDesign, connection.inputPinDesign)];
                downstream["pinName"] = connection.inputPinDesign.name;
                args["graphId"] = graphId;
                args["upstream"] = upstream;
                args["downstream"] = downstream;
                await rpc.Call("connect", args);
            }

            args = new JObject();
            args["graphId"] = graphId;
            await rpc.Call("buildGraph", args);

            args = new JObject();
            args["graphId"] = graphId;
            await rpc.Call("go", args);

            return "Done!";
        }

        private static NodeDesign FindNodeForPin(GraphDesign graphDesign, PinDesign pinDesign)
        {
            foreach (var node in graphDesign.nodeDesigns) {
                foreach (var input in node.inputPinDesigns) {
                    if (object.ReferenceEquals(input, pinDesign)) return node;
                }
                foreach (var output in node.outputPinDesigns) {
                    if (object.ReferenceEquals(output, pinDesign)) return node;
                }
            }
            return null;
        }
    }
}