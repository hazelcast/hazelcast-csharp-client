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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects;

/// <summary>
/// Represents the event handler for reliable topic.
/// </summary>
/// <typeparam name="T">Type of topic message.</typeparam>
public sealed class ReliableTopicEventHandler<T> : EventHandlersBase<IReliableTopicEventHandler<T>>
{
    /// <summary>
    /// Adds the handler which runs when a message is received.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>The handlers.</returns>
    public ReliableTopicEventHandler<T> Message(Action<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicMessageEventHandler<T>(handler));
        return this;
    }

    /// <summary>
    /// Adds the handler which runs when a message is received.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>The handlers.</returns>
    public ReliableTopicEventHandler<T> Message(Func<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>, ValueTask> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicMessageEventHandler<T>(handler));
        return this;
    }
    
    /// <summary>
    /// Sets the handler which runs after the subscription disposed or canceled.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public ReliableTopicEventHandler<T> Terminated(Action<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicTerminatedEventHandler<T>(handler));
        return this;
    }

    /// <summary>
    /// Sets the handler which runs after the subscription disposed or canceled.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public ReliableTopicEventHandler<T> Terminated(Func<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>, ValueTask> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicTerminatedEventHandler<T>(handler));
        return this;
    }
    
    // Exception event is single, no chaining.
    
    /// <summary>
    /// Sets the handler when runs on exception.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public void Exception(Action<IHReliableTopic<T>, ReliableTopicExceptionEventArgs> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicExceptionEventHandler<T>(handler));
    }

    /// <summary>
    /// Sets the handler when runs on exception.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public void Exception(Func<IHReliableTopic<T>, ReliableTopicExceptionEventArgs, ValueTask> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Add(new ReliableTopicExceptionEventHandler<T>(handler));
    }
}
