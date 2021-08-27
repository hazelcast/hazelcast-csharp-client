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

using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.FlakeId
{
    /// <inheritdoc cref="IFlakeIdGenerator"/>
    internal sealed class FlakeIdGenerator: DistributedObjectBase, IFlakeIdGenerator
    {
        private readonly FlakeIdGeneratorOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlakeIdGenerator"/> class.
        /// </summary>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="factory">The factory that owns this object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="options">A client options.</param>
        public FlakeIdGenerator(string name, DistributedObjectFactory factory, Cluster cluster,
            SerializationService serializationService, ILoggerFactory loggerFactory,
            HazelcastOptions options
        )
            : base(ServiceNames.FlakeIdGenerator, name, factory, cluster, serializationService, loggerFactory)
        {
            _options = options.GetFlakeIdGeneratorOptions(name) ?? FlakeIdGeneratorOptions.Default;
        }

        /// <inheritdoc />
        public async Task<long> GetNewIdAsync()
        {
            var requestMessage = FlakeIdGeneratorNewIdBatchCodec.EncodeRequest(Name, _options.PrefetchCount);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage);
            var response = FlakeIdGeneratorNewIdBatchCodec.DecodeResponse(responseMessage);
            return response.Base;
        }
    }
}
