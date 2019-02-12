// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    internal class ServiceNames
    {
        public const string Map = "hz:impl:mapService";

        public const string Topic = "hz:impl:topicService";

        public const string Set = "hz:impl:setService";

        public const string List = "hz:impl:listService";

        public const string MultiMap = "hz:impl:multiMapService";

        public const string Lock = "hz:impl:lockService";

        public const string IdGenerator = "hz:impl:idGeneratorService";

        public const string AtomicLong = "hz:impl:atomicLongService";

        public const string CountDownLatch = "hz:impl:countDownLatchService";

        public const string PNCounter = "hz:impl:PNCounterService";

        public const string Semaphore = "hz:impl:semaphoreService";

        public const string Cluster = "hz:impl:clusterService";

        public const string Queue = "hz:impl:queueService";

        public const string Partition = "hz:impl:partitionService";

        public const string ClientEngine = "hz:impl:clientEngineService";

        public const string DistributedExecutor = "hz:impl:distributedExecutorService";

        public const string Ringbuffer = "hz:impl:ringbufferService";

        public const string ReplicatedMap = "hz:impl:replicatedMapService";
    }
}