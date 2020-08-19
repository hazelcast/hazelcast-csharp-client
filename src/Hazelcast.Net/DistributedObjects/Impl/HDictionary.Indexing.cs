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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HDictionary<TKey, TValue> // Indexing
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public Task AddIndexAsync(IndexConfig indexConfig)
            => AddIndexAsync(indexConfig, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task AddIndexAsync(IndexConfig indexConfig, CancellationToken cancellationToken)
        {
            if (indexConfig == null) throw new ArgumentNullException(nameof(indexConfig));

            var requestMessage = MapAddIndexCodec.EncodeRequest(Name, indexConfig.ValidateAndNormalize(Name));
            var task = Cluster.Messaging.SendAsync(requestMessage, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}
