using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiMasterChangeFeed
{
    public class Region
    {
        private readonly string _region;
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly ChangeFeedProcessor _changeFeedProcessor;

        public Region(string region, CosmosClientBuilder cosmosClientBuilder, string database, string container, string leasesContainer)
        {
            _region = region;

            _client = cosmosClientBuilder
                .WithConnectionModeDirect()
                .WithApplicationRegion(region)
                .WithConsistencyLevel(ConsistencyLevel.Eventual)
                .Build();

            NonBlockingConsole.WriteLine($"Connecting to {_client.Endpoint.AbsoluteUri} ({region})");

            _container = _client.GetDatabase(database).GetContainer(container);

            var leases = _client.GetContainer(database, leasesContainer);
            _changeFeedProcessor = _client
                .GetContainer(database, container)
                .GetChangeFeedProcessorBuilder<Item>(_region, HandleChangesAsync)
                .WithInstanceName(_region)
                .WithLeaseContainer(leases)
                .Build();
        }

        public async Task Start()
        {
            // Warmup of the connection
            await _container.GetItemLinqQueryable<Item>().ToFeedIterator().ReadNextAsync();

            await _changeFeedProcessor.StartAsync();
        }

        public async Task<bool> Add(string id)
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
                return false;
            }
            NonBlockingConsole.WriteLine($"Added item {id} at {DateTime.UtcNow:hh:mm:ss.ffffff} in region {_region}");
            return true;
        }

        public async Task<Item> Get(string id)
        {
            return await _container.ReadItemAsync<Item>(id, new PartitionKey(id));
        }

        async Task HandleChangesAsync(IReadOnlyCollection<Item> changes, CancellationToken cancellationToken)
        {
            foreach (Item item in changes)
                NonBlockingConsole.WriteLine($"Change of {item.Id}, created at {item.InsertionTimestamp:hh:mm:ss.ffffff}, in region {item.Region}, reported by region {_region}");

            await Task.CompletedTask;
        }
    }
}
