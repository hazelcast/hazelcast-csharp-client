using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Spi
{
	
	public class PortableItemEvent :EventArgs, IPortable
	{
		private Data item;

		private ItemEventType eventType;

		private string uuid;

		public PortableItemEvent()
		{
		}

		public PortableItemEvent(Data item, ItemEventType eventType, string uuid)
		{
			this.item = item;
			this.eventType = eventType;
			this.uuid = uuid;
		}

		public virtual Data GetItem()
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
			//FIXME ENUM HATASI
			//writer.writeInt("e", eventType.getType());
			writer.WriteInt("e", 1);
			writer.WriteUTF("u", uuid);
			IOUtil.WriteNullableData(writer.GetRawDataOutput(), item);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			//FIXME ENUM HATASI
			eventType = ItemEventType.Added;
			//ItemEventType.getByType(reader.readInt("e"));
			uuid = reader.ReadUTF("u");
			item = IOUtil.ReadNullableData(reader.GetRawDataInput());
		}
	}
}
