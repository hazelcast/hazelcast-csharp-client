using System;
using System.Threading;
using System.IO;
using Hazelcast.IO;
namespace Hazelcast.Client.IO
	
{
	public class IOUtil
	{
		[ThreadStatic]
		private static ClientSerializer serializer;
		
		private static readonly int STRING_CHUNK_SIZE = 16 * 1024;

		public static byte[] toByte (Object obj)
		{
			if(obj == null){
				return new byte[0];
			}
			return GetSerializer().toByte (obj);
		}
		
		public static Data toData (Object obj)
		{
			return new Data(toByte(obj));
		}
		
		public static ClientSerializer GetSerializer(){
			if(serializer == null) serializer = new ClientSerializer();
			return serializer;
		}

		public static Object toObject (byte[] bytes)
		{
			if(bytes==null || bytes.Length ==0){
				return null;
			}
			return GetSerializer().toObject (bytes);
		}
		
		public static void writeUTF (BinaryWriter writer, String str)
		{
			Boolean isNull = str==null;
			writer.Write((byte) (isNull?1:0));
			if(isNull) return;
			int length = str.Length;
			int chunkSize = length / STRING_CHUNK_SIZE + 1;
			writer.Write (System.Net.IPAddress.HostToNetworkOrder (length));
			for (int i = 0; i < chunkSize; i++) {
				writer.Write ((byte)0);
				int beginIndex = Math.Max (0, i * STRING_CHUNK_SIZE - 1);
				int endIndex = Math.Min ((i + 1) * STRING_CHUNK_SIZE - 1, length);
				String sub = str.Substring (beginIndex, (endIndex-beginIndex));
				writer.Write(sub);

			}

		}
		public static String readUTF (BinaryReader reader)
		{
			Boolean isNull = (reader.ReadByte()==1);
			if(isNull) return null;
			
			String result = null;
			int length = System.Net.IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int chunkSize = length/STRING_CHUNK_SIZE + 1;
			while (chunkSize > 0) {
				reader.ReadByte ();
				//read the 0 that is required for Java compatibility;
				result += reader.ReadString ();
				chunkSize--;
			}
			return result;
		}
		
		public static void printBytes (byte[] bytes)
		{
			foreach (byte b in bytes) {
				Console.Write (b);
				Console.Write (".");
			}
			
			
		}
	}
}

