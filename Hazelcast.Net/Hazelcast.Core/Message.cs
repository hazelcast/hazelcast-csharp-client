using System;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Message for <see cref="ITopic{E}">ITopic&lt;E&gt;</see>.
    /// </summary>
    [Serializable]
    public class Message<E> : EventObject
    {
        private readonly E messageObject;

        private readonly long publishTime;

        private readonly IMember publishingMember;

        public Message(string topicName, E messageObject, long publishTime, IMember publishingMember) : base(topicName)
        {
            this.messageObject = messageObject;
            this.publishTime = publishTime;
            this.publishingMember = publishingMember;
        }

        /// <summary>Returns published message</summary>
        /// <returns>message object</returns>
        public virtual E GetMessageObject()
        {
            return messageObject;
        }

        /// <summary>Return the time when the message is published</summary>
        /// <returns>publish time</returns>
        public virtual long GetPublishTime()
        {
            return publishTime;
        }

        /// <summary>Returns the member that published the message</summary>
        /// <returns>publishing member</returns>
        public virtual IMember GetPublishingMember()
        {
            return publishingMember;
        }
    }
}