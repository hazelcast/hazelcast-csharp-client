using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public class CustomSerializerAdaptor: ITypeSerializer
	{
		ICustomSerializer customSerializer;
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public CustomSerializerAdaptor (ICustomSerializer customSerializer)
		{
			this.customSerializer = customSerializer;
		}
		
		public int priority ()
		{
			return 100;
		}

		public bool isSuitable (Object obj)
		{
			return true;
		}

		public byte getTypeId ()
		{
			return 1;
		}

		public void write (BinaryWriter writer, Object obj)
		{
			customSerializer.write(writer, obj);
		}
		public Object read (BinaryReader reader)
		{
			return customSerializer.read(reader);
		}
	}
}

