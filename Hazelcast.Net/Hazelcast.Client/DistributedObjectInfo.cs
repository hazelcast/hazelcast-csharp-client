using Hazelcast.Client;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client
{
	public class DistributedObjectInfo : IPortable
	{
		private string serviceName;

		private string name;

		internal DistributedObjectInfo()
		{
		}

		public virtual int GetFactoryId()
		{
			return ClientPortableHook.Id;
		}

		public virtual int GetClassId()
		{
			return ClientPortableHook.DistributedObjectInfo;
		}

		//REQUIRED
		public virtual string GetServiceName()
		{
			return serviceName;
		}

		public virtual string GetName()
		{
			return name;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("sn", serviceName);
			writer.WriteUTF("n", name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			serviceName = reader.ReadUTF("sn");
			name = reader.ReadUTF("n");
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}
			if (o == null || GetType() != o.GetType())
			{
				return false;
			}
			DistributedObjectInfo that = (DistributedObjectInfo)o;
			if (name != null ? !name.Equals(that.name) : that.name != null)
			{
				return false;
			}
			if (serviceName != null ? !serviceName.Equals(that.serviceName) : that.serviceName != null)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int result = serviceName != null ? serviceName.GetHashCode() : 0;
			result = 31 * result + (name != null ? name.GetHashCode() : 0);
			return result;
		}
	}
}
