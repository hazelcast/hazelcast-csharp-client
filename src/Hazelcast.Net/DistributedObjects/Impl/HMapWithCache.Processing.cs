﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMapWithCache<TKey, TValue> // Processing
    {
        protected override async Task<TResult> ExecuteAsync<TResult>(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.ExecuteAsync<TResult>(processorData, keyData, cancellationToken).CfAwait();
            }
            finally
            {
                _cache.Remove(keyData);
            }
        }
    }
}
