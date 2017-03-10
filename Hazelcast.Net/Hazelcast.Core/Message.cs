// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    ///     Message for <see cref="ITopic{E}">ITopic&lt;E&gt;</see>.
    /// </summary>
    [Serializable]
    public class Message<T> : EventObject
    {
        private readonly T _messageObject;

        private readonly IMember _publishingMember;

        private readonly long _publishTime;

        public Message(string topicName, T messageObject, long publishTime, IMember publishingMember) : base(topicName)
        {
            _messageObject = messageObject;
            _publishTime = publishTime;
            _publishingMember = publishingMember;
        }

        /// <summary>Returns published message</summary>
        /// <returns>message object</returns>
        public virtual T GetMessageObject()
        {
            return _messageObject;
        }

        /// <summary>Returns the member that published the message</summary>
        /// <returns>publishing member</returns>
        public virtual IMember GetPublishingMember()
        {
            return _publishingMember;
        }

        /// <summary>Return the time when the message is published</summary>
        /// <returns>publish time</returns>
        public virtual long GetPublishTime()
        {
            return _publishTime;
        }
    }
}