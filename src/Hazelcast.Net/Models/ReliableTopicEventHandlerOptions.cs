// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.DistributedObjects;

namespace Hazelcast.Models;

public class ReliableTopicEventHandlerOptions
{
    /// <summary>
    /// Initial sequence to start processing to messages.
    /// <remarks>-1 means start from next published message.</remarks>
    /// <remarks>For a durable subscription, you continue where you left by using the
    /// <see cref="ReliableTopicMessageEventArgs{T}.Sequence"/>.
    /// If provide <see cref="ReliableTopicMessageEventArgs{T}.Sequence"/> without incrementing,
    /// the same message will be received twice.</remarks>
    /// </summary>
    public long InitialSequence { get; set; } = -1;

    /// <summary>
    /// Whether this <see cref="ReliableTopicMessageEventHandler{T}"/> is able to deal with message loss.
    /// Even though the reliable topic promises to be reliable, it can be that a
    /// <see cref="ReliableTopicMessageEventHandler{T}"/> is too slow. Eventually the message won't be available anymore.
    /// <remarks>
    /// <see cref="StoreSequence"/> should be set <code>true</code> to dispose the subscriber in a data lost situation. 
    /// </remarks>
    /// </summary>
    public bool IsLossTolerant { get; set; }

    /// <summary>
    /// Stores the sequence of last received message. It provides durability to subscriber.
    /// </summary>
    public bool StoreSequence { get; set; }
}
