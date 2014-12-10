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
        private IData key;

        private IData oldValue;

        private string uuid;
        private int numberOfAffectedEntries = 1;

        private IData value;

        public PortableEntryEvent()
        {
        }

        public PortableEntryEvent(IData key, IData value, IData oldValue, EntryEventType eventType, string uuid)
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
            // Map Event and Entry Event is merged to one event, because when cpp client get response
            // from node, it first creates the class then fills the class what comes from wire. Currently
            // it can not handle more than one type response.
            writer.WriteInt("e", (int)eventType);
            writer.WriteUTF("u", uuid);
            writer.WriteInt("n", numberOfAffectedEntries);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
            output.WriteData(value);
            output.WriteData(oldValue);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            eventType = (EntryEventType)reader.ReadInt("e");
            uuid = reader.ReadUTF("u");
            numberOfAffectedEntries = reader.ReadInt("n");
            IObjectDataInput input = reader.GetRawDataInput();
            key = input.ReadData();
            value = input.ReadData();
            oldValue = input.ReadData();
        }

        public virtual IData GetKey()
        {
            return key;
        }

        public virtual IData GetValue()
        {
            return value;
        }

        public virtual IData GetOldValue()
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
        public int GetNumberOfAffectedEntries()
        {
            return numberOfAffectedEntries;
        }
    }
}