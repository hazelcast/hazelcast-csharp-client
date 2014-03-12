using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Spi
{
    internal class PortableEntryEvent : EventArgs, IPortable
    {
        private EntryEventType eventType;
        private Data key;

        private Data oldValue;

        private string uuid;
        private Data value;

        public PortableEntryEvent()
        {
        }

        public PortableEntryEvent(Data key, Data value, Data oldValue, EntryEventType eventType, string uuid)
        {
            this.key = key;
            this.value = value;
            this.oldValue = oldValue;
            this.eventType = eventType;
            this.uuid = uuid;
        }

        public virtual int GetFactoryId()
        {
            return SpiPortableHook.Id;
        }

        public virtual int GetClassId()
        {
            return SpiPortableHook.EntryEvent;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("e", (int) eventType);
            writer.WriteUTF("u", uuid);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            IOUtil.WriteNullableData(output, value);
            IOUtil.WriteNullableData(output, oldValue);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            eventType = (EntryEventType)reader.ReadInt("e");
            uuid = reader.ReadUTF("u");
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
            value = IOUtil.ReadNullableData(input);
            oldValue = IOUtil.ReadNullableData(input);
        }

        public virtual Data GetKey()
        {
            return key;
        }

        public virtual Data GetValue()
        {
            return value;
        }

        public virtual Data GetOldValue()
        {
            return oldValue;
        }

        public virtual EntryEventType GetEventType()
        {
            return eventType;
        }

        public virtual string GetUuid()
        {
            return uuid;
        }
    }
}