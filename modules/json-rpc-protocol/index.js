const JsonRpcProtocol = require('./json-rpc-protocol');
const JsonRpcChannel = require('./json-rpc-channel');

JsonRpcProtocol.connect = JsonRpcChannel.connect;

module.exports = JsonRpcProtocol;