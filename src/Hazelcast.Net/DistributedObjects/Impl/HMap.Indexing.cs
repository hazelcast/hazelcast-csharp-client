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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Indexing
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public Task AddIndexAsync(IndexOptions indexOptions)
            => AddIndexAsync(indexOptions, CancellationToken.None);

        public Task AddIndexAsync(IndexType indexType, params string[] attributes)
        {
            var indexConfig = new IndexOptions {Type = indexType};
            indexConfig.AddAttributes(attributes);
            return AddIndexAsync(indexConfig, CancellationToken.None);
        }

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task AddIndexAsync(IndexOptions indexOptions, CancellationToken cancellationToken)
        {
            if (indexOptions == null) throw new ArgumentNullException(nameof(indexOptions));

            var requestMessage = MapAddIndexCodec.EncodeRequest(Name, indexOptions.ValidateAndNormalize(Name));
            var task = Cluster.Messaging.SendAsync(requestMessage, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }
    }
}
