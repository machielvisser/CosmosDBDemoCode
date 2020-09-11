# CosmosDBDemoCode

## Multi-Master setup ChangeFeed behaviour
This sample shows a multi-master setup. 
On both write regions a ChaneFeedProcessor is started.
A conflicting write will be done which will be resolved with the server side conflict resolution policy.
The received changes from the change feed are logged.