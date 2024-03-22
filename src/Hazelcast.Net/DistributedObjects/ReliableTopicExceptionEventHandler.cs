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

using System;
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a handler for the <see cref="TopicEventTypes.Exception"/> event.
    /// </summary>
    /// <typeparam name="T">The reliable topic object type.</typeparam>
    internal class ReliableTopicExceptionEventHandler<T> : IReliableTopicExceptionEventHandler<T>
    {
        private readonly Func<IHReliableTopic<T>, ReliableTopicExceptionEventArgs, ValueTask> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicExceptionEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicExceptionEventHandler(Action<IHReliableTopic<T>, ReliableTopicExceptionEventArgs> handler)
        {
            _handler = (sender, args) =>
            {
                handler(sender, args);
                return default;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicExceptionEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicExceptionEventHandler(Func<IHReliableTopic<T>, ReliableTopicExceptionEventArgs, ValueTask> handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public ValueTask HandleAsync(IHReliableTopic<T> sender, ReliableTopicExceptionEventArgs args)
        {
            return _handler(sender, args);
        }
    }
}
