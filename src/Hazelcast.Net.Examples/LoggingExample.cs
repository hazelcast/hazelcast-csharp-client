// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples
{
    // ReSharper disable once UnusedMember.Global
    public class LoggingExample
    {
        // run this example with
        // ./hz.ps1 run-example Logging
        // ./hz.ps1 run-example Logging --- --Logging:LogLevel:Hazelcast.Examples.LoggingExample.A=Debug
        //
        // note that options set with the With(...) method of the HazelcastOptionsBuilder take precedence
        // over everything, including command-line and environment variable options.

        public static void Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast.Examples.LoggingExample.B", "Warning")
                .Build();

            var loggerFactory = options.LoggerFactory.Service;

            var loggerA = loggerFactory.CreateLogger<A>();

            loggerA.LogDebug("This is a DEBUG message from Hazelcast.Examples.LoggingExamples.A");
            loggerA.LogInformation("This is an INFO message from Hazelcast.Examples.LoggingExamples.A");
            loggerA.LogWarning("This is a WARNING message from Hazelcast.Examples.LoggingExamples.A");

            var loggerB = loggerFactory.CreateLogger<B>();

            loggerB.LogDebug("This is a DEBUG message from Hazelcast.Examples.LoggingExamples.B");
            loggerB.LogInformation("This is an INFO message from Hazelcast.Examples.LoggingExamples.B");
            loggerB.LogWarning("This is a WARNING message from Hazelcast.Examples.LoggingExamples.B");

            // flush logs!
            loggerFactory.Dispose();
        }

        public class A
        { }

        public class B
        { }
    }
}
