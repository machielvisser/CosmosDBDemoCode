using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiMasterChangeFeed
{
    class Program
    {
        static async Task Main(string[] _)
        {
            var cosmosConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(typeof(Program).Assembly)
                .Build()
                .GetSection("Cosmos");

            var endpoint = cosmosConfiguration["Endpoint"];
            var authKey = cosmosConfiguration["AuthorizationKey"];
            var database = cosmosConfiguration["Database"];
            var container = cosmosConfiguration["Container"];
            var leasesContainer = cosmosConfiguration["LeasesContainer"];

            var europeClient = new CosmosClientBuilder(endpoint, authKey).WithApplicationRegion(Regions.NorthEurope).Build();
            var australiaClient = new CosmosClientBuilder(endpoint, authKey).WithApplicationRegion(Regions.AustraliaSoutheast).Build();

            var europeContainer = europeClient.GetDatabase(database).GetContainer(container);
            var australiaContainer = australiaClient.GetDatabase(database).GetContainer(container);


            // ChangeFeed Processor
            var europeLeaseContainer = europeClient.GetContainer(database, leasesContainer);
            await europeClient.GetContainer(database, container)
                .GetChangeFeedProcessorBuilder<Item>(nameof(HandleChangesAsync), HandleChangesAsync)
                    .WithInstanceName(nameof(Program))
                    .WithLeaseContainer(europeLeaseContainer)
                    .Build()
                    .StartAsync();

            var australiaLeaseContainer = europeClient.GetContainer(database, leasesContainer);
            await europeClient.GetContainer(database, container)
                .GetChangeFeedProcessorBuilder<Item>(nameof(HandleChangesAsync), HandleChangesAsync)
                    .WithInstanceName(nameof(Program))
                    .WithLeaseContainer(australiaLeaseContainer)
                    .Build()
                    .StartAsync();
        }

        static async Task HandleChangesAsync(IReadOnlyCollection<Item> changes, CancellationToken cancellationToken)
        {
            foreach (Item item in changes)
            {
                Console.WriteLine($"Detected operation for item with id {item.Id}, created at {item._ts}.");
            }

            await Task.CompletedTask;
        }
    }
}
