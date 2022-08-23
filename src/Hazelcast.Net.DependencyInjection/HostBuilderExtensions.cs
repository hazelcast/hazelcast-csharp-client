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

using Hazelcast.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hazelcast.DependencyInjection
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureHazelcast(this IHostBuilder builder, string[] args)
        {
            return builder.ConfigureAppConfiguration((hostingContext, hostBuilder) =>
            {
                // inserts Hazelcast-specific configuration
                // (default configuration has been added by the host)
                hostBuilder.AddHazelcast(args);

                // for more control, users would need to implement their own version of this metho
                // example: change the hazelcast options file name
                //builder.AddHazelcast(args, optionsFileName: "special.json");
            });
        }
    }
}
