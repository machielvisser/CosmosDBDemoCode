using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var accountRegion = new Region(Regions.NorthEurope, clientBuilder, database, container, leasesContainer);
            var secondRegion = new Region(Regions.AustraliaSoutheast, clientBuilder, database, container, leasesContainer);

            await Task.WhenAll(
                    accountRegion.Start(),
                    secondRegion.Start()
                    );

            NonBlockingConsole.WriteLine($"Initialization finished at {DateTime.UtcNow:hh:mm:ss.ffffff}");

            await SimulateConflictResolution(accountRegion, secondRegion);

            Console.ReadKey();
        }

        private static async Task SimulateConflictResolution(Region region1, Region region2)
        {
            var id = RandomId;
            var success = false;

            // Sometimes one write is finished before the other reaches the database, 
            // resulting in a client conflict instead of a server conflict
            // Repeat till the two writes have resulted in server side conflict resolution
            do
            {
                id = RandomId;

                var result = await Task.WhenAll(
                    region1.Add(id),
                    region2.Add(id)
                    );

                success = result.All(x => x);
            }
            while (!success);

            // Monitor the regions till the conflict is resolved
            var conflictResolved = false;
            do
            {
                var region1Result = await region1.Get(id);
                var region2Result = await region2.Get(id);

                conflictResolved = region1Result.Region.Equals(region2Result.Region);

                if (conflictResolved)
                {
                    var timeDiff = (DateTime.UtcNow - new DateTime(Math.Min(region1Result.InsertionTimestamp.Ticks, region2Result.InsertionTimestamp.Ticks))).TotalMilliseconds;
                    NonBlockingConsole.WriteLine($"Region {region1Result.Region} won within {timeDiff}ms at {DateTime.UtcNow:hh:mm:ss.ffffff}");
                }
            }
            while (!conflictResolved);
        }
    }
}
