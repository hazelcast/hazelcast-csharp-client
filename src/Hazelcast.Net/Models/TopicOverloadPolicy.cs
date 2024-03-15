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

namespace Hazelcast.Models;

/// <summary>
/// Defines the overload policy for a topic when there is no place to store a message.
/// </summary>
public enum TopicOverloadPolicy
{
    /// <summary>
    /// A message that has not expired can be overwritten.
    /// No matter the retention period set, the overwrite will just overwrite the item.
    /// <remarks>
    /// The policy can cause problem for slow consumers
    /// since they were promised a certain time window to process the messages.
    /// But it will benefit producers and fast consumers since they are able to continue.
    /// This policy sacrifices the slow producer in favor of fast producers/consumers.
    /// </remarks>
    /// </summary>
    DiscardOldest = 0,

    /// <summary>
    /// The message that was to be published is discarded.
    /// </summary>
    DiscardNewest = 1,

    /// <summary>
    /// The caller will wait till there space in the ring buffer.
    /// </summary>
    Block = 2,

    /// <summary>
    /// The publish call immediately fails.
    /// </summary>
    Error = 3
}
