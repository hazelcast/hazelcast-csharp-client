// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
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

            var logger = options.LoggerFactory.Service.CreateLogger<SqlBasicQueryExample>();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // FIXME? [Oleksii] clarify if below is correct and OK
            // We will use Jet to emulate long-running query.
            // To run this example you will need to enable Jet in the cluster configuration via '<jet enabled="true"></jet>'.
            // Below is a simple example of such setup:
            // "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            // <hazelcast xmlns="http://www.hazelcast.com/schema/config
            //   xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance
            //   xsi:schemaLocation="http://www.hazelcast.com/schema/config http://www.hazelcast.com/schema/config/hazelcast-config-5.0.xsd">
            //   <jet enabled="true"></jet>" +
            // </hazelcast>;

            // cancel after timeout
            {
                var timeout = TimeSpan.FromSeconds(10);
                logger.LogInformation("Generating values for {Timeout}", timeout);

                // infinite stream that will generate 1 sequential long value about every second
                await using var result = client.Sql.ExecuteQuery("SELECT * from TABLE(generate_stream(1))");

                var cancellationTask = Task.Delay(timeout);

                while (!cancellationTask.IsCompleted)
                {
                    var moveNext = result.MoveNextAsync().AsTask();

                    if (await Task.WhenAny(moveNext, cancellationTask) == cancellationTask)
                        break;

                    if (await moveNext)
                        logger.LogInformation("{Value}", result.Current.GetColumn<long>(0));
                    else
                        break;
                }
            } // result will dispose here, leading to executing query being cancelled

            // cancel after user action
            {
                var timeout = TimeSpan.FromSeconds(10);
                logger.LogInformation("Generating values until user action", timeout);

                // infinite stream that will generate 1 sequential long value about every second
                await using var result = client.Sql.ExecuteQuery("SELECT * from TABLE(generate_stream(1))");

                var cancellationSource = new TaskCompletionSource<object>();
                var cancellationTask = cancellationSource.Task;

                // listening for console input to emulate action, but any other event can be used
                _ = Task.Run(() =>
                {
                    logger.LogInformation("Press Enter to stop");
                    Console.ReadLine();
                    cancellationSource.SetResult(null);
                });

                while (!cancellationTask.IsCompleted)
                {
                    var moveNext = result.MoveNextAsync().AsTask();

                    if (await Task.WhenAny(moveNext, cancellationTask) == cancellationTask)
                        break;

                    if (await moveNext)
                        logger.LogInformation("{Value}", result.Current.GetColumn<long>(0));
                    else
                        break;
                }
            } // result will dispose here, leading to executing query being cancelled

            logger.LogInformation("Finished");
        }
    }
}
