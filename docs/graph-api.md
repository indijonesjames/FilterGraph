# Graph API (Client → Server)

These are messages sent from the Client to the Server.

## `createGraph()`

Creates a new graph.

* Returns: _\<string>_ id of the graph that was created.

## `requireFilter(type)`

Returns the metadata for a registered filter type.

* `type` _\<string>_ The registered type name of the filter.
* Returns: _\<object>_ the filter-specific metadata of the type.

## `createFilter(graphId, type, params)`

Creates a filter that can be added to a graph.

* `graphId` _\<string>_  The id of the graph.
* `params` _\<object>_  Filter-specific parameters.
* Returns: _\<string>_ id of the filter that was created.

## `connect(graphId, upstream: { filterId[, pinName] }, downstream: { filterId[, pinName] })`

Connects two filters in a filter graph.

* `graphId` _\<string>_  The id of the graph.
* `upstream.filterId` _\<string>_ The id of the upstream filter to be connected.
* `upstream.pinName` _\<string>_ The name of the upstream filter's output pin to be connected.
* `downstream.filterId` _\<string>_ The id of the downstream filter to be connected.
* `downstream.pinName` _\<string>_ The name of the downstream filter's input pin to be connected.

## `buildGraph(graphId[, options])`

Builds the graph.  The graph must be built before it can be run.

* `options` _\<object>_
  * `stream` _\<boolean>_ If `true`, graph will run in streaming mode (until stopped).  Otherwise, the graph will run once.
* Returns: `true`

## `go(graphId)`

Starts a graph running.

* `graphId` _\<string>_  The id of the graph.
* Returns: `true`

## `stop(graphId)`

Stops a running graph.

* `graphId` _\<string>_  The id of the graph.
* Returns: `true`

## `destroy(graphId)`

Destroys a graph and all its filters.

* `graphId` _\<string>_  The id of the graph.
* Returns: `true`

## `set(name, value)`

Sets a global variable's value.

* `name` _\<string>_  The name of the global variable.
* `value` _<any>_  The value to store in the global variable.

## `get(name)`

Gets a global variable's value.

* `name` _\<string>_  The name of the global variable.
* Returns: _\<any>_  The value of the global variable.

## `watch(graphId, name)`

Notifies the caller every time the specified graph assigns a new value to the specified variable and provides a copy of the variable's value.

* `name` _\<string>_ The name of the variable to watch.
* Returns: Nothing

# Callbacks (Server → Client)

Callbacks are initiated by the server with the following API.

## `changed(name, value)`

A global variable was modified by a running graph.

* `name` _\<string>_ The name of the variable.
* `value` _\<any>_ Filter-specific details of the callback.
* Returns: Nothing

## `notify(filterId, value)`

A global variable was modified by a running graph.

* `filterId` _\<string>_ The id of the filter generating the notification.
* `value` _\<any>_ Filter-specific details the notification.
* Returns: Nothing

# Filters Needed

## `subtract()`

Subtracts two values.  Computes `result` = `positive` - `negative`.

* Inputs:
  * `positive` _\<value>_ The positive value.
  * `negative` _\<value>_ The value to be subtracted.
* Outputs:
  * `result` _\<value>_ The difference result = `positive` - `negative`.

## `extern`

Provides an external value.

* Outputs:
  * `output` _\<value>_ The static data.

## `set(name)`

Stores a value in a global variable so that it can be used later in another graph.

* `name` _\<string>_ The name of the variable to retrieve.
* Inputs:
  * `input` _\<value>_  The value to store in the variable.

## `get(name)`

Retrieves a value from a global variable that was stored previously in another graph.

* `name` _\<string>_ The name of the variable to retrieve.
* Outputs:
  * `output` _\<value>_ The value that was stored in the variable.

Sends a copy of the data as
## `camera()`

* Outputs:
  * `output` _\<image>_ The captured image

