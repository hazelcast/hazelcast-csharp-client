// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Data;
using Hazelcast.Data.Topic;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for the <see cref="TopicEventType.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
    public sealed class TopicMessageEventArgs<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicMessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The object.</param>
        public TopicMessageEventArgs(MemberInfo member, long publishTime, T payload)
        {
            Member = member;
            PublishTime = publishTime;
            Payload = payload;
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the message publish time.
        /// </summary>
        // TODO: consider UTC DateTime
        public long PublishTime { get; }

        /// <summary>
        /// Gets the topic object carried by the message.
        /// </summary>
        public T Payload { get; }
    }
}