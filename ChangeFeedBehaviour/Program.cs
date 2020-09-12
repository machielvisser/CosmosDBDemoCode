using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var regions = new[] { Regions.NorthEurope, Regions.AustraliaSoutheast }
                .Select(id => new Region(id, clientBuilder, database, container, leasesContainer))
                .ToList();

            await Task.WhenAll(regions.Select(region => region.Start()));

            NonBlockingConsole.WriteLine($"Initialization finished at {DateTime.UtcNow:hh:mm:ss.ffffff}");

            var methods = new[] { nameof(MultiRegionChangeFeedTrigger), nameof(ServerSideConflictResolution) };

            do
            {
                Console.WriteLine($"Choose a Method: {string.Join(", ", methods.Select((method, index) => $"{index}: {method}"))} (Default: 0)");
                var input = Console.ReadKey();
                try
                {
                    if (int.TryParse(input.KeyChar.ToString(), out int choice))
                    {
                        Console.WriteLine($"Starting: {methods[choice]}");

                        var task = choice switch
                        {
                            1 => MultiRegionChangeFeedTrigger(regions),
                            _ => ServerSideConflictResolution(regions)
                        };
                        await task;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            while (true);
        }

        private static async Task MultiRegionChangeFeedTrigger(IEnumerable<Region> regions)
        {
            var id = RandomId;
            var item = await regions.First().Add(id);
            var writeTimestamp = DateTime.UtcNow;

            // Monitor the regions till the change is consist for all read regions
            var consistent = false;
            do
            {
                var result = await Task.WhenAll(regions.Select(region => region.Get(id)));

                consistent = result.All(result => result is object);

                if (consistent)
                {
                    var timeDiff = (DateTime.UtcNow - writeTimestamp).TotalMilliseconds;
                    NonBlockingConsole.WriteLine($"Item became consistent after {timeDiff}ms at {DateTime.UtcNow:hh:mm:ss.ffffff}");
                }
            }
            while (!consistent);
        }


        private static async Task ServerSideConflictResolution(IEnumerable<Region> regions)
        {
            if (!await regions.First().IsMultiMaster())
                throw new NotSupportedException("Account is not configured as Multi-Master");

            var id = RandomId;
            var success = false;
            DateTime writeTimestamp;

            // Sometimes one write is finished before the other reaches the database, 
            // resulting in a client conflict instead of a server conflict
            // Repeat till the two writes have resulted in server side conflict resolution
            do
            {
                id = RandomId;

                var result = await Task.WhenAll(regions.Select(region => region.Add(id)));

                writeTimestamp = DateTime.UtcNow;

                success = result.All(x => x is object);
            }
            while (!success);

            // Monitor the regions till the conflict is resolved
            var conflictResolved = false;
            do
            {
                var result = await Task.WhenAll(regions.Select(region => region.Get(id)));

                conflictResolved = result.Select(item => item.Region).Distinct().Count().Equals(1);
                
                if (conflictResolved)
                {
                    var timeDiff = (DateTime.UtcNow - writeTimestamp).TotalMilliseconds;
                    NonBlockingConsole.WriteLine($"Region {result.First().Region} won within {timeDiff}ms at {DateTime.UtcNow:hh:mm:ss.ffffff}");
                }
            }
            while (!conflictResolved);
        }
    }
}
