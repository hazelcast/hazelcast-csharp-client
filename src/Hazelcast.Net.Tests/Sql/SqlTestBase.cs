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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Models;
using Hazelcast.Sql;
using Hazelcast.Testing;

namespace Hazelcast.Tests.Sql
{
    public abstract class SqlTestBase : SingleMemberClientRemoteTestBase
    {
        protected virtual bool EnableJet => false;

        protected override string RcClusterConfiguration => EnableJet
            ? Resources.jet_enabled
            : base.RcClusterConfiguration;

        protected async Task<IHMap<int, string>> CreateIntMapAsync(IDictionary<int, string> entries)
        {
            var map = await Client.GetMapAsync<int, string>(GenerateMapName());

            await map.AddIndexAsync(IndexType.Sorted, "__key");
            await map.AddIndexAsync(IndexType.Sorted, "this");

            await map.SetAllAsync(entries);

            await Client.Sql.CreateMapping(map);

            return map;
        }

        protected Task<IHMap<int, string>> CreateIntMapAsync(int size)
        {
            var entries = Enumerable.Range(1, size).ToDictionary(i => i, i => i.ToString());
            return CreateIntMapAsync(entries);
        }

        // TODO: what are the rules for acceptable SQL map names?
        protected string GenerateMapName()
            => new string($"{Guid.NewGuid():N}".Select(c => char.IsDigit(c) ? (char)(c + 'g' - '1') : c).ToArray());
    }
}