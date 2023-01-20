// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;
using Microsoft.Extensions.Options;

namespace Hazelcast.DependencyInjection;

// an options factory for Hazelcast failover options, that wires the IServiceProvider into the options
internal class HazelcastFailoverOptionsFactory : IOptionsFactory<HazelcastFailoverOptions>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Action<HazelcastFailoverOptionsBuilder> _configureBuilder;

    public HazelcastFailoverOptionsFactory(IServiceProvider serviceProvider, Action<HazelcastFailoverOptionsBuilder> configureBuilder)
    {
        _serviceProvider = serviceProvider;
        _configureBuilder = configureBuilder;
    }

    public HazelcastFailoverOptions Create(string name)
    {
        var builder = new HazelcastFailoverOptionsBuilder().With(_serviceProvider);
        _configureBuilder(builder);
        return builder.Build();
    }
}
