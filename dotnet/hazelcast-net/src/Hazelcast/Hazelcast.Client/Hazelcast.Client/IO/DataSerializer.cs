using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public class DataSerializer : ITypeSerializer<DataSerializable>
	{
		
		private static readonly byte SERIALIZER_TYPE_DATA = 0;
		
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 0;
		}

		public bool isSuitable (object obj)
		{
			return obj is DataSerializable;
		}

		public byte getTypeId ()
		{
			return SERIALIZER_TYPE_DATA;
		}

		public void write (BinaryWriter writer, DataSerializable obj)
		{
			string name = (string)obj.GetType().ToString();
			//Console.WriteLine("Name as " + name);
			if (obj is Hazelcast.Impl.Keys){
				name = "com.hazelcast.impl.Keys";
			}
			IOUtil.writeUTF(writer, name);
			obj.writeData(writer);
		}
		public DataSerializable read (BinaryReader reader)
		{
			string name = IOUtil.readUTF(reader);
			
			if(name.Equals("com.hazelcast.impl.base.Pairs"))
			{
				name = "Hazelcast.Impl.Base.Pairs";
			}
			else if(name.Equals("com.hazelcast.impl.Keys"))
			{
				name = "Hazelcast.Impl.Keys";
			}
			//Console.WriteLine("Name changed to " + Type.GetType(name));
			DataSerializable obj = (DataSerializable)Activator.CreateInstance(Type.GetType(name));
			obj.readData(reader);
			return obj;
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is DataSerializable))
				throw new ArgumentException ("obj");
			this.write (writer, (DataSerializable)obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}
}

