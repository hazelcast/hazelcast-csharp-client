// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal sealed class AutoBatcher : AutoBatcherBase
    {
        private readonly string _name;
        private readonly FlakeIdGeneratorOptions _options;
        private readonly ClusterMessaging _messaging;

        public AutoBatcher(string name, FlakeIdGeneratorOptions options, ClusterMessaging messaging)
        {
            _name = name;
            _options = options;
            _messaging = messaging;
        }

        protected override async Task<Batch> FetchBatch()
        {
            var requestMessage = FlakeIdGeneratorNewIdBatchCodec.EncodeRequest(_name, _options.PrefetchCount);
            var responseMessage = await _messaging.SendAsync(requestMessage).CfAwait();
            var response = FlakeIdGeneratorNewIdBatchCodec.DecodeResponse(responseMessage);

            return new Batch(response.Base, response.Increment, response.BatchSize, _options.PrefetchValidityPeriod);
        }
    }
}
