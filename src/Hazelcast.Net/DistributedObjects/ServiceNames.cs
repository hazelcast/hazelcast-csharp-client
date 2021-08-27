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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Define the service names.
    /// </summary>
    internal static class ServiceNames
    {
        /// <summary>
        /// The name of the map service.
        /// </summary>
        public const string Map = "hz:impl:mapService";

        /// <summary>
        /// The name of the topic service.
        /// </summary>
        public const string Topic = "hz:impl:topicService";

        /// <summary>
        /// The name of the list service.
        /// </summary>
        public const string List = "hz:impl:listService";

        /// <summary>
        /// The name of the multi map service.
        /// </summary>
        public const string MultiMap = "hz:impl:multiMapService";

        /// <summary>
        /// The name of the queue service.
        /// </summary>
        public const string Queue = "hz:impl:queueService";

        /// <summary>
        /// The name of the replicated map service.
        /// </summary>
        public const string ReplicatedMap = "hz:impl:replicatedMapService";

        /// <summary>
        /// The name off the ring buffer service.
        /// </summary>
        public const string RingBuffer = "hz:impl:ringbufferService";

        /// <summary>
        /// The name of the set service.
        /// </summary>
        public const string Set = "hz:impl:setService";

        /// <summary>
        /// The name of the raft atomic long service.
        /// </summary>
        public const string AtomicLong = "hz:raft:atomicLongService";

        /// <summary>
        /// The name of the raft atomic ref service.
        /// </summary>
        public const string AtomicRef = "hz:raft:atomicRefService";

        /// <summary>
        /// The name of the Flake ID Generator service.
        /// </summary>
        public const string FlakeIdGenerator = "hz:impl:flakeIdGeneratorService";
    }
}
