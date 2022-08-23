// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Sql
{
    // ReSharper disable once UnusedMember.Global
    public class SqlCancellationExample
    {
        public static async Task Main(params string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast.Examples", "Information")
                .Build();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            var logger = client.Options.LoggerFactory.CreateLogger<SqlCancellationExample>();

            // We will use Jet to emulate long-running query.
            // To run this example you will need to enable Jet in the cluster configuration via '<jet enabled="true"></jet>'.
            // Below is a simple example of such config:
            // "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            // <hazelcast xmlns="http://www.hazelcast.com/schema/config
            //   xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance
            //   xsi:schemaLocation="http://www.hazelcast.com/schema/config http://www.hazelcast.com/schema/config/hazelcast-config-5.0.xsd">
            //   <jet enabled="true"></jet>" +
            // </hazelcast>;

            await CancelAfterTimeout(client, logger);
            await CancelAfterUserAction(client, logger);

            logger.LogInformation("Finished");
        }

        private static async Task CancelAfterTimeout(IHazelcastClient client, ILogger logger)
        {
            var timeout = TimeSpan.FromSeconds(5);
            logger.LogInformation("Generating values for {Timeout}", timeout);

            // infinite stream that will generate 1 sequential long value about every second
            await using var result = await client.Sql.ExecuteQueryAsync("SELECT * from TABLE(generate_stream(1))");

            using var cancellationSource = new CancellationTokenSource(timeout);

            try
            {
                await foreach (var row in result.WithCancellation(cancellationSource.Token))
                    logger.LogInformation("{Value}", row.GetColumn<long>(0));
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Enumeration was cancelled after {Timeout}", timeout);
            }
        } // result will dispose here, freeing resources used by the query

        private static async Task CancelAfterUserAction(IHazelcastClient client, ILogger logger)
        {
            var timeout = TimeSpan.FromSeconds(10);
            logger.LogInformation("Generating values until user action", timeout);

            // infinite stream that will generate 1 sequential long value about every second
            await using var result = await client.Sql.ExecuteQueryAsync("SELECT * from TABLE(generate_stream(1))");

            using var cancellationSource = new CancellationTokenSource();

            Console.WriteLine(@"Press Ctrl/Command+C to stop");
            Console.CancelKeyPress += (sender, args) =>
            {
                cancellationSource.Cancel();
                args.Cancel = true; // wait for iteration to end
            };

            try
            {
                await foreach (var row in result.WithCancellation(cancellationSource.Token))
                    logger.LogInformation("{Value}", row.GetColumn<long>(0));
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Enumeration was cancelled by the user");
            }
        } // result will dispose here, freeing resources used by the query
    }
}
