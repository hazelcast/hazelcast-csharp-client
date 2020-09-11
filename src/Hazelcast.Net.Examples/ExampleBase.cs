// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples
{
    public abstract class ExampleBase
    {
        public static HazelcastOptions BuildExampleOptions(string[] args, Dictionary<string, string> keyValues = null, string optionsFilePath = null, string optionsFileName = null, string environmentName = null, Action<IConfiguration, HazelcastOptions> configureOptions = null)
        {
            keyValues ??= new Dictionary<string, string>();

            static void AddIfMissing(IDictionary<string, string> d, string k, string v)
            {
                if (!d.ContainsKey(k)) d.Add(k, v);
            }

            // add Microsoft logging configuration
            AddIfMissing(keyValues, "Logging:LogLevel:Default", "Debug");
            AddIfMissing(keyValues, "Logging:LogLevel:System", "Information");
            AddIfMissing(keyValues, "Logging:LogLevel:Microsoft", "Information");

            return HazelcastOptions.Build(args, keyValues, optionsFilePath, optionsFileName, environmentName,(configuration, options) =>
            {
                // configure logging factory and add the console provider
                options.Logging.LoggerFactory.Creator = () =>
                    LoggerFactory.Create(builder =>
                        builder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddConsole());

                configureOptions?.Invoke(configuration, options);
            });
        }
    }
}
