const JsonRpcProtocol = require('@modules/json-rpc-protocol');

main()
  .then(() => {
    process.exit();
  })
  .catch(err => {
    console.error('There was an unhandled exception', err);
    process.exit();
  });


async function main() {
  let target = { notify: async details => {
    console.log('notify:', details);
  }};
  let rpc = await JsonRpcProtocol.connect('localhost', 8337, target);
  let graphId = await rpc.createGraph();
  let constFilter = await rpc.createFilter({ graphId, type: 'const', params: 42 });
  let resultFilter = await rpc.createFilter({ graphId, type: 'result' });
  await rpc.connect({
    graphId,
    upstream: { filterId: constFilter, pinName: 'value' },
    downstream: { filterId: resultFilter, pinName: 'value' }
  });
  await rpc.buildGraph({ graphId });
  await rpc.go({ graphId });
  console.log('finished');
}