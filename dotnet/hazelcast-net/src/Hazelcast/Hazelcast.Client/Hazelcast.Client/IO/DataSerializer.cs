using System;
using System.IO;
using Hazelcast.IO;
using Hazelcast.Impl.Base;
using Hazelcast.Impl;
using Hazelcast.Core;
using Hazelcast.Cluster;
using Hazelcast.Security;
namespace Hazelcast.Client.IO
{
	public class DataSerializer : ITypeSerializer<DataSerializable>
	{
		
		private static readonly byte SERIALIZER_TYPE_DATA = 0;
		
		static DataSerializer(){
			TypeRegistry.register("com.hazelcast.impl.MemberImpl", typeof(MemberImpl));
			TypeRegistry.register("com.hazelcast.nio.Address", typeof(Address));		
			TypeRegistry.register("com.hazelcast.impl.CMap$CMapEntry", typeof(CMapEntry));		
			TypeRegistry.register("com.hazelcast.impl.base.Values", typeof(Values));		
			TypeRegistry.register("com.hazelcast.impl.FactoryImpl$ProxyKey", typeof(ProxyKey));		
			TypeRegistry.register("com.hazelcast.impl.ClientServiceException", typeof(ClientServiceException));		
			TypeRegistry.register("com.hazelcast.impl.Keys", typeof(Keys));		
			TypeRegistry.register("com.hazelcast.impl.base.KeyValue", typeof(KeyValue));
			TypeRegistry.register("com.hazelcast.impl.base.Pairs", typeof(Pairs));
			TypeRegistry.register("com.hazelcast.cluster.Bind", typeof(Bind));
			TypeRegistry.register("com.hazelcast.security.UsernamePasswordCredentials", typeof(UsernamePasswordCredentials));
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
			string name = TypeRegistry.getJavaName(obj.GetType());
			
			
			if(name == null){
				name = (string)obj.GetType().ToString();
			}
			
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
			
			
			DataSerializable obj = (DataSerializable)createInstance(name);
			obj.readData(new BinaryReaderDataInput(reader));
			return obj;
		}
		
		public static Object createInstance(String name){
			Type type = TypeRegistry.getType(name);
			if(type==null){
				type = Type.GetType(name);
			}

			return Activator.CreateInstance(type);
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

