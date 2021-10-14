const net = require('net');
const JsonRpcProtocol = require('./json-rpc-protocol');

class JsonRpcChannel {
  connect(host, port, target) {
    return new Promise((resolve, reject) => {
      let socket = new net.Socket();
      socket.connect({
        host, port
      });
      socket.on('connect', async () => {
        console.log('connected');

        let rpc = new JsonRpcProtocol(socket, target);
        resolve(rpc);
      });
      socket.on('error', err => {
        console.error('socket error:', err);
        reject(err);
      });
    });
  }
}

JsonRpcChannel.connect = async function (...args) {
  let channel = new JsonRpcChannel();
  return await channel.connect(...args);
}

module.exports = JsonRpcChannel;
