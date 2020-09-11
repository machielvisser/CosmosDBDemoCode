using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiMasterChangeFeed
{
    public class Region
    {
        private readonly string _region;
        private readonly Container _container;
        private readonly ChangeFeedProcessor _changeFeedProcessor;

        public Region(string region, CosmosClientBuilder cosmosClientBuilder, string database, string container, string leasesContainer)
        {
            _region = region;

            var client = cosmosClientBuilder
                .WithConnectionModeDirect()
                .WithApplicationRegion(region)
                .WithConsistencyLevel(ConsistencyLevel.Eventual)
                .Build();

            NonBlockingConsole.WriteLine($"Connecting to {client.Endpoint.AbsoluteUri} ({region})");

            _container = client.GetDatabase(database).GetContainer(container);

            var leases = client.GetContainer(database, leasesContainer);
            _changeFeedProcessor = client
                .GetContainer(database, container)
                .GetChangeFeedProcessorBuilder<Item>(_region, HandleChangesAsync)
                .WithInstanceName(_region)
                .WithLeaseContainer(leases)
                .Build();
        }

        public async Task Start()
        {
            await _changeFeedProcessor.StartAsync();
        }

        public async Task Add(string id)
        {
            NonBlockingConsole.WriteLine($"Adding item {id} at {DateTime.UtcNow:hh:mm:ss.ffffff} in region {_region}");
            try
            {
                await _container.CreateItemAsync(
                    new Item
                    {
                        Id = id,
                        Partition = id,
                        Region = _region
                    }, 
                    requestOptions: new ItemRequestOptions
                    {
                        ConsistencyLevel = ConsistencyLevel.Eventual,
                    });
            }
            catch (CosmosException e)
            {
                NonBlockingConsole.WriteLine($"Exception in {_region}: {e.Message}");
            }
            NonBlockingConsole.WriteLine($"Added item {id} at {DateTime.UtcNow:hh:mm:ss.ffffff} in region {_region}");
        }

        public async Task<Item> Get(string id)
        {
            return await _container.ReadItemAsync<Item>(id, new PartitionKey(id));
        }

        async Task HandleChangesAsync(IReadOnlyCollection<Item> changes, CancellationToken cancellationToken)
        {
            foreach (Item item in changes)
                NonBlockingConsole.WriteLine($"Change of {item.Id}, created at {DateTime.Parse(item.InsertionTimestamp):hh:mm:ss.fff}, in region {_region}");

            await Task.CompletedTask;
        }
    }
}
