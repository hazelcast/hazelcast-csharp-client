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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects;

/// <summary>
/// Specifies a reliable topic terminated event handler.
/// </summary>
/// <typeparam name="T">The reliable topic objects type.</typeparam>
public interface IReliableTopicTerminatedEventHandler<T> : IReliableTopicEventHandlerBase
{
    /// <summary>
    /// Handles an event.
    /// </summary>
    /// <param name="sender">The <see cref="IHReliableTopic{T}"/> that triggered the event.</param>
    /// <param name="sequence">The sequence of the message in the ring buffer.</param>
    /// <param name="state">A state object.</param>
    /// <exception name="e">Exception occured while processing the event.</exception>
    ValueTask HandleAsync(IHReliableTopic<T> sender, long sequence, object state);
}
