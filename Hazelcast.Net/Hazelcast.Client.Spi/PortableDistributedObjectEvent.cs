using System;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Spi
{

    public class PortableDistributedObjectEvent : EventArgs, IPortable
	{
		private DistributedObjectEvent.EventType eventType;

		private string name;

		private string serviceName;

		public PortableDistributedObjectEvent()
		{
		}

		public PortableDistributedObjectEvent(DistributedObjectEvent.EventType eventType, string name, string serviceName)
		{
			this.eventType = eventType;
			this.name = name;
			this.serviceName = serviceName;
		}

		public virtual DistributedObjectEvent.EventType GetEventType()
		{
			return eventType;
		}

		public virtual string GetName()
		{
			return name;
		}

		//REQUIRED
		public virtual string GetServiceName()
		{
			return serviceName;
		}

		public virtual int GetFactoryId()
		{
			return SpiPortableHook.Id;
		}

		public virtual int GetClassId()
		{
			return SpiPortableHook.DistributedObjectEvent;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteUTF("s", serviceName);
			writer.WriteUTF("t", eventType.ToString());
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			serviceName = reader.ReadUTF("s");
            Enum.TryParse(reader.ReadUTF("t"), true,out eventType);
		}
	}
}
