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

using Hazelcast.Core;

namespace Hazelcast.Protocol.Models;

internal enum ListenerConfigType
{
    /// <summary>
    /// Not specific to any data structure or service.
    /// </summary>
    [Enums.JavaName("GENERIC")] Generic = 0,

    /// <summary>
    /// For ItemListenerConfig.
    /// </summary>
    [Enums.JavaName("ITEM")] Item = 1,

    /// <summary>
    /// For EntryListenerConfig.
    /// </summary>
    [Enums.JavaName("ENTRY")] Entry = 2,

    /// <summary>
    /// For SplitBrainProtectionListenerConfig.
    /// </summary>
    [Enums.JavaName("SPLIT_BRAIN_PROTECTION")] SplitBrainProtection = 3,

    /// <summary>
    /// For CachePartitionLostListenerConfig.
    /// </summary>
    [Enums.JavaName("CACHE_PARTITION_LOST")] CachePartitionLost = 4,

    /// <summary>
    /// For MapPartitionLostListenerConfig.
    /// </summary>
    [Enums.JavaName("MAP_PARTITION_LOST")] MapPartitionLost = 5
}
