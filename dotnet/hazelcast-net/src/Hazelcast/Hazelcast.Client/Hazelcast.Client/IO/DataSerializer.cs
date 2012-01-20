using System;
using System.IO;
using Hazelcast.IO;
namespace Hazelcast.Client.IO
{
	public class DataSerializer : ITypeSerializer<DataSerializable>
	{
		
		private static readonly byte SERIALIZER_TYPE_DATA = 0;
		private static readonly System.Collections.Concurrent.ConcurrentDictionary<String, Type> mapper = new System.Collections.Concurrent.ConcurrentDictionary<String, Type>();
		
		static DataSerializer(){
			//looks ugly but forces static code to run on the following Classes. 
			CMapEntry.className.Equals("");	
		}
		
		public static bool register(String javaClassName, Type type){
			Console.WriteLine("Registering" + javaClassName + " : " + type);
			return mapper.TryAdd(javaClassName, type);
		}
		
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
			string name = (obj.javaClassName()!=null)? obj.javaClassName(): (string)obj.GetType().ToString();
			//Console.WriteLine("Name as " + name);
			IOUtil.writeUTF(writer, name);
			obj.writeData(new BinaryWriterDataOutput(writer));
		}
		public DataSerializable read (BinaryReader reader)
		{
			
			string name = IOUtil.readUTF(reader);
			
			
			if(name.Equals("com.hazelcast.impl.base.Pairs"))
			{
				name = "Hazelcast.Impl.Base.Pairs";
			}
			else if(name.Equals("com.hazelcast.impl.base.KeyValue"))
			{
				name = "Hazelcast.Impl.Base.KeyValue";
			}
			else if(name.Equals("com.hazelcast.impl.Keys"))
			{
				name = "Hazelcast.Impl.Keys";
			}
			
			Type type = Type.GetType(name);
			
			if(mapper.ContainsKey(name)){
				mapper.TryGetValue(name, out type);
			}
				
			Console.WriteLine("Name changed to " +name);
			Console.WriteLine("Type is: " + type);
			DataSerializable obj = (DataSerializable)Activator.CreateInstance(type);
			obj.readData(new BinaryReaderDataInput(reader));
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

