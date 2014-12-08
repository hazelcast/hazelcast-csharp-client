using System;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Topic
{
    internal class PortableMessage : EventArgs, IPortable
    {
        private IData message;

        private long publishTime;

        private string uuid;

        public PortableMessage()
        {
        }

        public PortableMessage(IData message, long publishTime, string uuid)
        {
            this.message = message;
            this.publishTime = publishTime;
            this.uuid = uuid;
        }

        public virtual int GetFactoryId()
        {
            return TopicPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return TopicPortableHook.PortableMessage;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            //writer.WriteLong("pt", publishTime);
            //writer.WriteUTF("u", uuid);
            //message.WriteData(writer.GetRawDataOutput());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            publishTime = reader.ReadLong("pt");
            uuid = reader.ReadUTF("u");
            message = reader.GetRawDataInput().ReadData();
        }

        public virtual IData GetMessage()
        {
            return message;
        }

        public virtual long GetPublishTime()
        {
            return publishTime;
        }

        public virtual string GetUuid()
        {
            return uuid;
        }
    }
}