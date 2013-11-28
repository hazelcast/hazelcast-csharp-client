using System.Text;
using Hazelcast.Client;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client
{
	
	public sealed class GenericError : IPortable
	{
		private string message;

		private string details;

		private int type;

		public GenericError()
		{
		}

		public GenericError(string message, int type)
		{
			this.message = message;
			this.type = type;
		}

		public GenericError(string message, string details, int type)
		{
			this.message = message;
			this.details = details;
			this.type = type;
		}

		public int GetFactoryId()
		{
			return ClientPortableHook.Id;
		}

		public int GetClassId()
		{
			return ClientPortableHook.GenericError;
		}

		public string GetMessage()
		{
			return message;
		}

		public string GetDetails()
		{
			return details;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("m", message);
			writer.WriteUTF("d", details);
			writer.WriteInt("t", type);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadPortable(IPortableReader reader)
		{
			message = reader.ReadUTF("m");
			details = reader.ReadUTF("d");
			type = reader.ReadInt("t");
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("GenericError{");
			sb.Append("message='").Append(message).Append('\'');
			sb.Append(", type=").Append(type);
			sb.Append('}');
			return sb.ToString();
		}
	}
}
