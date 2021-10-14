const net = require('net');
const JsonRpcProtocol = require('./json-rpc-protocol');

const filterTypes = [
  { type: 'result', fn(io) { this.graph.notify(this, io.value) } },
  { type: 'const', fn(io) { io.value = this.params } },
  { type: 'get', fn(io) { io.output = this.variables[this.params] } },
  { type: 'set', fn(io) { this.variables[this.params] = io.input } },
  { type: 'add', fn(io) { io.output = io.A + io.B } },
  { type: 'subtract', fn(io) { io.output = io.pos - io.neg } },
  { type: 'multiply', fn(io) { io.output = io.A * io.B } },
  { type: 'divide', fn(io) { io.output = io.num / io.den } },
  { type: 'negate', fn(io) { io.output = -io.input } }
]

function locateFilterType(type) {
  return filterTypes.find(t => t.type == type);
}

class Service {
  constructor() {
    this.nextUnusedId = 1n;
    this.graphs = [];
  }

  async test() {
    return 'Hello, world';
  }
  async add({ a, b }) {
    return a + b;
  }

  async createGraph() {
    console.log('createGraph called');
    let id = this.nextUnusedId++;
    let graph = { id, filters: [], transfers: [], rpc: this.rpc };
    graph.notify = function (filter, value) {
      this.rpc.notify({ filterId: filter.id.toString(), value });
    };
    this.graphs.push(graph);

    return id.toString();
  }

  async requireFilter({ type }) {
    console.log('requireFilter called');
    return { info: `metadata for ${type}` };
  }

  async createFilter({ graphId, type, params }) {
    console.log('createFilter called');
    let id = this.nextUnusedId++;
    // find the graph
    let graph = this.graphs.find(g => g.id == graphId);
    if (!graph) throw new Error("graph not found");
    let filter = { id, type, params, graph };
    graph.filters.push(filter);

    return id.toString();
  }

  async connect({ graphId, upstream, downstream }) {
    console.log('connect called');

    let graph = this.graphs.find(g => g.id == graphId);
    if (!graph) throw new Error("graph not found");

    // find the filters
    let upstreamFilter = graph.filters.find(f => f.id == upstream.filterId);
    if (!upstreamFilter) throw new Error("upstream filter not found");
    let downstreamFilter = graph.filters.find(f => f.id == downstream.filterId);
    if (!upstreamFilter) throw new Error("downstream filter not found");

    let transfer = {
      upstream: { filter: upstreamFilter, pinName: upstream.pinName },
      downstream: { filter: downstreamFilter, pinName: downstream.pinName }
    };
    graph.transfers.push(transfer);
  }

  async buildGraph({ graphId, ...args }) {
    console.log('buildGraph called');

    let graph = this.graphs.find(g => g.id == graphId);
    if (!graph) throw new Exception('graph not found');

    // Here's where we create all the objects.

    // 1) Determine a render order for the filters and connections.  This is "Topological sorting"
    // 1) Determine a render function for each filter
    // 2) Assign io storage locations for each filter

    // Upstream objects must render before downstream objects

    // Topological sort (See https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search)
    let order = [];
    const visit = n => {
      if (n.perm) return;
      if (n.temp) throw new Exception('graph has cycles');
      n.temp = true;
      for (let t of graph.transfers) if (t.upstream.filter == n) visit(t.downstream.filter);
      delete n.temp;
      n.perm = true;
      order.unshift(n);
    };
    var n;
    while (graph.filters.find(f => !f.perm)) {
      visit(graph.filters.find(f => !f.perm && !f.temp));
    }
    graph.filters = order;

    // Show order of filters
    //console.log(order.map(n => n.type).join(' > '));

    for (let filter of graph.filters) {
      // the filter function can be determined by the type of filter
      let filterType = locateFilterType(filter.type);
      if (!filterType) throw new Exception('unsupported filter type');
      filter.render = filterType.fn;
    }

    for (let transfer of graph.transfers) {
      transfer.render = function (input, output) {
        output[transfer.downstream.pinName] = input[transfer.upstream.pinName];
      };
    }

  }

  async go({ graphId }) {
    console.log('go called');
    let graph = this.graphs.find(g => g.id == graphId);
    if (!graph) throw new Exception('graph not found');

    for (let filter of graph.filters) {
      filter.io = {};
    }

    for (let filter of graph.filters) {
      filter.render(filter.io);
      for (let transfer of graph.transfers) if (transfer.upstream.filter == filter) {
        transfer.render(filter.io, transfer.downstream.filter.io);
      }
    }
  }

  async stop({ graphId }) {
    console.log('stop called');
  }

  async destroy({ graphId }) {
    console.log('destroy called');
  }

  async set({ name, value }) {
    console.log('set called');
  }

  async get({ name }) {
    console.log('get called');
  }

  async watch({ graphId, name }) {
    console.log('watch called');
  }

}

const service = new Service();

var server = net.createServer(socket => {
  socket.on('error', err => {
  });
  const rpc = new JsonRpcProtocol(socket, service);
  service.rpc = rpc;
});

server.listen(8337, '127.0.0.1');