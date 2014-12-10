using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Spi
{
    internal class PortableItemEvent : EventArgs, IPortable
    {
        private ItemEventType eventType;
        private IData item;

        private string uuid;

        public PortableItemEvent()
        {
        }

        public PortableItemEvent(IData item, ItemEventType eventType, string uuid)
        {
            this.item = item;
            this.eventType = eventType;
            this.uuid = uuid;
        }

        public virtual int GetFactoryId()
        {
            return SpiPortableHook.Id;
        }

        public virtual int GetClassId()
        {
            return SpiPortableHook.ItemEvent;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("e", (int) eventType);
            writer.WriteUTF("u", uuid);
            writer.GetRawDataOutput().WriteData(item);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            eventType = (ItemEventType)reader.ReadInt("e");
            uuid = reader.ReadUTF("u");
            item = reader.GetRawDataInput().ReadData();
        }

        public virtual IData GetItem()
        {
            return item;
        }

        public virtual ItemEventType GetEventType()
        {
            return eventType;
        }

        public virtual string GetUuid()
        {
            return uuid;
        }
    }
}