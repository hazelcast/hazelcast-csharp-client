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
using System;
using Hazelcast.Serialization;
using Microsoft.Extensions.ObjectPool;
namespace Hazelcast.Core
{
    internal class ObjectDataOutputPooledObjectPolicy : PooledObjectPolicy<ObjectDataOutput>
    {
        private Func<ObjectDataOutput> _create;

        public ObjectDataOutputPooledObjectPolicy(Func<ObjectDataOutput> create)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
        }

        public override ObjectDataOutput Create()
            => _create();
        public override bool Return(ObjectDataOutput obj)
            => obj.TryReset();
    }
}
