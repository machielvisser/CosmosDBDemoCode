# CosmosDBDemoCode

## Multi-Master setup ChangeFeed behaviour
This sample shows a multi-master setup. 
On both write regions a ChangeFeedProcessor is started.
A conflicting write will be done which will be resolved with the server side conflict resolution policy.
The received changes from the change feed are logged.

```
Connecting to https://mvcosmosdemo.documents.azure.com/ (North Europe)
Connecting to https://mvcosmosdemo.documents.azure.com/ (Australia Southeast)
Initialization finished at 12:38:13.773917
Adding item 1963543886 at 12:38:13.777174 in region North Europe
Adding item 1963543886 at 12:38:13.805347 in region Australia Southeast
Added item 1963543886 at 12:38:13.881753 in region North Europe
Added item 1963543886 at 12:38:14.098053 in region Australia Southeast
Region Australia Southeast won within 461.3721ms at 12:38:14.461377
Change of 1963543886, created at 12:38:14.000000, in region Australia Southeast, reported by region North Europe at 12:38:18.880889
Change of 1963543886, created at 12:38:14.000000, in region Australia Southeast, reported by region Australia Southeast at 12:38:19.330700
```
