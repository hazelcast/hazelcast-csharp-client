using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;
namespace Hazelcast.Client.IO
{
	public class DefaultSerializer : ICustomSerializer
	{
		public static readonly byte SERIALIZER_TYPE_OBJECT = 10;
		public static readonly byte SERIALIZER_TYPE_BYTE_ARRAY = 1;
		public static readonly byte SERIALIZER_TYPE_INTEGER = 2;
		public static readonly byte SERIALIZER_TYPE_LONG = 3;
		//public static readonly byte SERIALIZER_TYPE_CLASS = 4;
		public static readonly byte SERIALIZER_TYPE_STRING = 5;
		//public static readonly byte SERIALIZER_TYPE_DATE = 6;
		//public static readonly byte SERIALIZER_TYPE_BIG_INTEGER = 7;
		//public static readonly byte SERIALIZER_TYPE_EXTERNALIZABLE = 8;
		//ITypeSerializer[] serializers = sort (new ITypeSerializer[5] { new ByteArraySerializer (), new StringSerializer (), new LongSerializer(), new IntegerSerializer(), new ObjectSerializer() });
		
		public static readonly byte SERIALIZER_TYPE_BOOLEAN = 9;
		
		static SortedList<ITypeSerializer, string> serializers = new SortedList<ITypeSerializer, string>();
		ITypeSerializer[] serializersByTypeId;
		
		static DefaultSerializer()
		{
			register(new ByteArraySerializer());
			register(new StringSerializer());
			register(new LongSerializer());
			register(new IntegerSerializer());
			register(new BooleanSerializer());
			register(new ObjectSerializer());
		}


		public DefaultSerializer ()
		{
			serializersByTypeId = new ITypeSerializer[20];
			foreach (ITypeSerializer serializer in serializers.Keys) {
				serializersByTypeId[serializer.getTypeId ()] = serializer;
			}
		}
		
		public static void register(ITypeSerializer serializer){
			serializers.Add(serializer, "");	
		}

		/*public static ITypeSerializer[] sort (ITypeSerializer[] serializers)
		{
			Array.Sort (serializers, delegate(ITypeSerializer t1, ITypeSerializer t2) { return t1.priority ().CompareTo (t2.priority ()); });
			return serializers;
		}
		*/

		public void write (BinaryWriter writer, Object obj)
		{
			
			foreach (ITypeSerializer serializer in serializers.Keys) {
				if (serializer.isSuitable (obj)) {
					writer.Write ((byte)serializer.getTypeId ());
					serializer.write (writer, obj);
					return;
				}
			}
			
		}

		public Object read (BinaryReader reader)
		{
			byte typeId = reader.ReadByte ();
			ITypeSerializer serializer = serializersByTypeId[typeId];
			if(serializer == null){
				Console.WriteLine("No Serializer for type id " + typeId);
				return null;
			}
			return serializer.read (reader);
		}

		public int CompareTo (ITypeSerializer t1, ITypeSerializer t2)
		{
			return t1.priority () - t2.priority ();
		}
		
		public static int CompareTo(ITypeSerializer serializer1, Object obj) {
			ITypeSerializer serializer2 = obj as ITypeSerializer;
			if(serializer2 != null)
			{
				if(serializer2.priority()==serializer1.priority())
				{
					return serializer1.getTypeId() - serializer2.getTypeId();
				}
				else
				{
					return serializer1.priority() - serializer2.priority();
				}
			}
			else
			{
				return 1;
			}
		}
	}

	public class ByteArraySerializer : ITypeSerializer<byte[]>
	{
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 100;
		}

		public bool isSuitable (Object obj)
		{
			return obj is byte[];
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_BYTE_ARRAY;
		}

		public void write (BinaryWriter writer, byte[] obj)
		{
			writer.Write (obj.Length);
			writer.Write (obj);
		}
		public byte[] read (BinaryReader reader)
		{
			int size = reader.ReadInt32 ();
			byte[] bytes = new byte[size];
			reader.Read (bytes, 0, size);
			return bytes;
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is byte[]))
				throw new ArgumentException ("obj");
			
			this.write (writer, (byte[])obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}
	
	public class LongSerializer : ITypeSerializer<long>
	{
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 200;
		}

		public bool isSuitable (Object obj)
		{
			return obj is long;
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_LONG;
		}

		public void write (BinaryWriter writer, long obj)
		{
			writer.Write ((long)System.Net.IPAddress.HostToNetworkOrder(obj));
		}
		public long read (BinaryReader reader)
		{
			return System.Net.IPAddress.NetworkToHostOrder(reader.ReadInt64());
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is long))
				throw new ArgumentException ("obj");
			
			this.write (writer, (long)obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}
	public class IntegerSerializer : ITypeSerializer<int>
	{
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 300;
		}

		public bool isSuitable (Object obj)
		{
			return obj is int;
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_INTEGER;
		}

		public void write (BinaryWriter writer, int obj)
		{
			writer.Write ((int)System.Net.IPAddress.HostToNetworkOrder(obj));
		}
		public int read (BinaryReader reader)
		{
			return System.Net.IPAddress.NetworkToHostOrder(reader.ReadInt32());
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is int))
				throw new ArgumentException ("obj");
			
			this.write (writer, (int)obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}
	
	public class BooleanSerializer : ITypeSerializer<bool>
	{
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return 300;
		}

		public bool isSuitable (Object obj)
		{
			return obj is bool;
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_BOOLEAN;
		}

		public void write (BinaryWriter writer, bool obj)
		{
			writer.Write ( (byte) (obj==true?1:0));
		}
		public bool read (BinaryReader reader)
		{
			return reader.ReadByte() == 1;
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is bool))
				throw new ArgumentException ("obj");
			
			this.write (writer, (bool)obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}

	public class StringSerializer : ITypeSerializer<String>
	{
		
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		

		public int priority ()
		{
			return 400;
		}

		public bool isSuitable (Object obj)
		{
			return obj is String;
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_STRING;
		}

		private void writeUTF (BinaryWriter writer, String str)
		{
			IOUtil.writeUTF(writer, str);
		}

		private String readUTF (BinaryReader reader)
		{
			return IOUtil.readUTF(reader);
		}

		public void write (BinaryWriter writer, String obj)
		{
			writeUTF (writer, obj);
		}


		public String read (BinaryReader reader)
		{
			return readUTF (reader);
		}

		void ITypeSerializer.write (BinaryWriter writer, object obj)
		{
			if (!(obj is string))
				throw new ArgumentException ("obj");
			
			this.write (writer, (string)obj);
		}

		object ITypeSerializer.read (BinaryReader reader)
		{
			return this.read (reader);
		}
	}
	
	public class ObjectSerializer : ITypeSerializer<Object>
	{
		
		public int CompareTo(object obj) 
		{
			return DefaultSerializer.CompareTo(this, obj);
		}
		
		public int priority ()
		{
			return Int32.MaxValue;
		}

		public bool isSuitable (Object obj)
		{
			return obj is ISerializable;
		}

		public byte getTypeId ()
		{
			return DefaultSerializer.SERIALIZER_TYPE_OBJECT;
		}

		public void write (BinaryWriter writer, Object obj)
		{
			BinaryFormatter bf =new BinaryFormatter();
			bf.Serialize(writer.BaseStream, obj);
		}
		public Object read (BinaryReader reader)
		{
			BinaryFormatter bf = new BinaryFormatter();
			Object o = bf.Deserialize(reader.BaseStream);
			return o;
		}
	}

}

