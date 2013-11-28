using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Client.Spi
{
	
	[System.Serializable]
	public class ObjectNamespace : IDataSerializable
	{
		private string service;

		private string objectName;

		public ObjectNamespace()
		{
		}

		public ObjectNamespace(string serviceName, string objectName)
		{
			this.service = serviceName;
			this.objectName = objectName;
		}

		public virtual string GetServiceName()
		{
			return service;
		}

		public virtual string GetObjectName()
		{
			return objectName;
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
			ObjectNamespace that = (ObjectNamespace)o;
			if (objectName != null ? !objectName.Equals(that.objectName) : that.objectName != null)
			{
				return false;
			}
			if (service != null ? !service.Equals(that.service) : that.service != null)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int result = service != null ? service.GetHashCode() : 0;
			result = 31 * result + (objectName != null ? objectName.GetHashCode() : 0);
			return result;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WriteData(IObjectDataOutput output)
		{
			output.WriteUTF(service);
			output.WriteObject(objectName);
		}

		// writing as object for backward-compatibility
		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadData(IObjectDataInput input)
		{
			service = input.ReadUTF();
			objectName = input.ReadObject<string>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("ObjectNamespace");
			sb.Append("{service='").Append(service).Append('\'');
			sb.Append(", objectName=").Append(objectName);
			sb.Append('}');
			return sb.ToString();
		}
	}
}
