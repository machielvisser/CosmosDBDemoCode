# CosmosDBDemoCode

## Multi-Master setup ChangeFeed behaviour
This sample shows a multi-master setup. 
On both write regions a ChangeFeedProcessor is started.
A conflicting write will be done which will be resolved with the server side conflict resolution policy.
The received changes from the change feed are logged.

```
Connecting to https://mvcosmosdemo.documents.azure.com/ (North Europe)
Connecting to https://mvcosmosdemo.documents.azure.com/ (Australia Southeast)
Initialization finished at 12:12:28.656510
Adding item 105287713 at 12:12:28.660405 in region North Europe
Adding item 105287713 at 12:12:28.689252 in region Australia Southeast
Added item 105287713 at 12:12:28.768806 in region North Europe
Added item 105287713 at 12:12:28.981506 in region Australia Southeast
Region North Europe won
Change of 105287713, created at 12:12:28.000, in region Australia Southeast, reported by region North Europe
Change of 105287713, created at 12:12:28.000, in region Australia Southeast, reported by region Australia Southeast
```
