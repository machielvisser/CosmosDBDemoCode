# CosmosDBDemoCode

## Multi-Master setup ChangeFeed behaviour
This sample shows a multi-master setup. 
On both write regions a ChangeFeedProcessor is started.
A conflicting write will be done which will be resolved with the server side conflict resolution policy.
The received changes from the change feed are logged.

```
Connecting to https://mvcosmosdemo.documents.azure.com/ (North Europe)
Connecting to https://mvcosmosdemo.documents.azure.com/ (Australia Southeast)
Initialization finished at 12:27:51.187410
Adding item 2092177417 at 12:27:51.190810 in region North Europe
Adding item 2092177417 at 12:27:51.220290 in region Australia Southeast
Added item 2092177417 at 12:27:51.299823 in region North Europe
Added item 2092177417 at 12:27:51.505110 in region Australia Southeast
Region Australia Southeast won within 1689.2438ms
Change of 2092177417, created at 12:27:51.000000, in region Australia Southeast, reported by region North Europe
Change of 2092177417, created at 12:27:51.000000, in region Australia Southeast, reported by region Australia Southeast
```
