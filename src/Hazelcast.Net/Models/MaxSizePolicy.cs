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

namespace Hazelcast.Models;

/// <summary>
/// Represents the maximum size policy.
/// </summary>
public enum MaxSizePolicy
{
    /// <summary>
    /// Policy based on maximum number of entries stored per data
    /// structure (map, cache etc) on each Hazelcast instance
    /// </summary>
    [Enums.JavaName("PER_NODE")] PerNode = 0,

    /// <summary>
    /// Policy based on maximum number of entries stored per
    /// data structure (map, cache etc) on each partition
    /// </summary>
    [Enums.JavaName("PER_PARTITION")] PerPartition = 1,

    /// <summary>
    /// Policy based on maximum used JVM heap memory percentage per
    /// data structure (map, cache etc) on each Hazelcast instance
    /// </summary>
    [Enums.JavaName("USED_HEAP_PERCENTAGE")] UsedHeapPercentage = 2,

    /// <summary>
    /// Policy based on maximum used JVM heap memory in megabytes per
    /// data structure (map, cache etc) on each Hazelcast instance
    /// </summary>
    [Enums.JavaName("USED_HEAP_SIZE")] UsedHeapSize = 3,

    /// <summary>
    /// Policy based on minimum free JVM
    /// heap memory percentage per JVM
    /// </summary>
    [Enums.JavaName("FREE_HEAP_PERCENTAGE")] FreeHeapPercentage = 4,

    /// <summary>
    /// Policy based on minimum free JVM
    /// heap memory in megabytes per JVM
    /// </summary>
    [Enums.JavaName("FREE_HEAP_SIZE")] FreeHeapSize = 5,

    /// <summary>
    /// Policy based on maximum number of entries
    /// stored per data structure (map, cache etc)
    /// </summary>
    [Enums.JavaName("ENTRY_COUNT")] EntryCount = 6,

    /// <summary>
    /// Policy based on maximum used native memory in megabytes per
    /// data structure (map, cache etc) on each Hazelcast instance
    /// </summary>
    [Enums.JavaName("USED_NATIVE_MEMORY_SIZE")] UsedNativeMemorySize = 7,

    /// <summary>
    /// Policy based on maximum used native memory percentage per
    /// data structure (map, cache etc) on each Hazelcast instance
    /// </summary>
    [Enums.JavaName("USED_NATIVE_MEMORY_PERCENTAGE")] UsedNativeMemoryPercentage = 8,

    /// <summary>
    /// Policy based on minimum free native
    /// memory in megabytes per Hazelcast instance
    /// </summary>
    [Enums.JavaName("FREE_NATIVE_MEMORY_SIZE")] FreeNativeMemorySize = 9,

    /// <summary>
    /// Policy based on minimum free native
    /// memory percentage per Hazelcast instance
    /// </summary>
    [Enums.JavaName("FREE_NATIVE_MEMORY_PERCENTAGE")] FreeNativeMemoryPercentage = 10
}
