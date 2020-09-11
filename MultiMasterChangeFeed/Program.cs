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
        private static string RandomId => new Random().Next().ToString();

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

            var clientBuilder = new CosmosClientBuilder(endpoint, authKey);

            var region1 = new Region(Regions.WestUS, clientBuilder, database, container, leasesContainer);
            var region2 = new Region(Regions.AustraliaSoutheast, clientBuilder, database, container, leasesContainer);

            await region1.Start();
            await region2.Start();

            NonBlockingConsole.WriteLine($"Initialization finished at {DateTime.UtcNow:hh:mm:ss.ffffff}");

            var id = RandomId;

            await Task.WhenAll(
                Task.Run(async () => await region1.Add(id)),
                Task.Run(async () => await region2.Add(id))
                );

            var written = await region1.Get(id);
            NonBlockingConsole.WriteLine($"Region {written.Region} won");

            Console.ReadKey();
        }
    }
}
