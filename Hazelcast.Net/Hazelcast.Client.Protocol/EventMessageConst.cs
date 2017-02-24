// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client.Protocol
{
    /// <summary>Message type ids of event responses in client protocol.</summary>
    /// <remarks>
    ///     Message type ids of event responses in client protocol. They also used to bind a request to event inside Request
    ///     annotation.
    /// </remarks>
    public sealed class EventMessageConst
    {
        public const int EventMember = 200;
        public const int EventMemberSet = 201;
        public const int EventMemberAttributeChange = 202;
        public const int EventEntry = 203;
        public const int EventItem = 204;
        public const int EventTopic = 205;
        public const int EventPartitionLost = 206;
        public const int EventDistributedObject = 207;
        public const int EventCacheInvalidation = 208;
        public const int EventMapPartitionLost = 209;
        public const int EventCache = 210;
        public const int EventCacheBatchInvalidation = 211;
        public const int EventQueryCacheSingle = 212;
        public const int EventQueryCacheBatch = 213;
        public const int EventCachePartitionLost = 214;
        public const int EventIMapInvalidation = 215;
        public const int EventIMapBatchInvalidation = 216;
    }
}