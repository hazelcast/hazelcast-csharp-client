using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public class ClientSerializer
	{
		private ITypeSerializer cs = new CustomSerializerAdaptor(new DefaultSerializer());
		private ITypeSerializer ds = new DataSerializer();
		
		
		public byte[] toByte(Object obj){
			MemoryStream stream = new MemoryStream();
			ITypeSerializer ts = ds.isSuitable(obj)? ds: cs;
			stream.Seek(0, SeekOrigin.Begin);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((byte)ts.getTypeId());
			ts.write(writer, obj);
			return stream.ToArray();
		}
		
		public Object toObject(byte[] bytes){
			MemoryStream stream = new MemoryStream(bytes);
			BinaryReader reader = new BinaryReader(stream);
			byte typeID = reader.ReadByte();
			ITypeSerializer serializer = (ds.getTypeId()== typeID)? ds: cs;
			return serializer.read(reader);
		}
		
	}
}

