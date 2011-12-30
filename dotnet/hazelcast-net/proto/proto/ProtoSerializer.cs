using System;
using System.IO;
using ProtoBuf;
using System.Reflection;
namespace Hazelcast.Client.IO
{
	public class ProtoSerializer: ITypeSerializer
	{
		private StringSerializer stringSerializer = new StringSerializer();
		public ProtoSerializer ()
		{
		}
		
		private static readonly byte SERIALIZER_TYPE_PROTO = 9;
		
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 700;
		}

		public bool isSuitable (Object obj)
		{
			return true;
		}

		public byte getTypeId ()
		{
			return SERIALIZER_TYPE_PROTO;
		}

		
		public void write (BinaryWriter writer, object obj)
		{
			stringSerializer.write(writer, obj.GetType().ToString().Replace('+', '$'));
			Serializer.NonGeneric.Serialize(writer.BaseStream, obj);
		}

		public object read (BinaryReader reader)
		{
			String typeName = stringSerializer.read(reader);
			Type type = Type.GetType(typeName.Replace('$','+'));
			Object obj = Serializer.NonGeneric.Deserialize(type, reader.BaseStream);
			return obj;
		}
	}
}

