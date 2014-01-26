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
				int beginIndex = Math.Max (0, i * STRING_CHUNK_SIZE - 1);
				int endIndex = Math.Min ((i + 1) * STRING_CHUNK_SIZE - 1, length);
				String sub = str.Substring (beginIndex, (endIndex-beginIndex));
				writeShortUTF(writer, sub);
			}

		}

		private static void writeShortUTF(BinaryWriter writer, String str){
			int strlen = str.Length;
			int utflen = 0;
			int c, count = 0;
			/* use charAt instead of copying String to char array */
			for (int i = 0; i < strlen; i++) {
				c = str[i];
				if ((c >= 0x0001) && (c <= 0x007F)) {
					utflen++;
				} else if (c > 0x07FF) {
					utflen += 3;
				} else {
					utflen += 2;
				}
			}
			//        if (utflen > 65535)
			//            throw new UTFDataFormatException("encoded string too long: " + utflen + " bytes");
			byte[] bytearr = new byte[utflen];
			writer.Write (System.Net.IPAddress.HostToNetworkOrder ((short)utflen));
			int j;
			for (j = 0; j < strlen; j++) {
				c = str[j];
				if (!((c >= 0x0001) && (c <= 0x007F)))
					break;
				bytearr[count++] = (byte) c;
			}
			for (; j < strlen; j++) {
				c = str[j];
				if ((c >= 0x0001) && (c <= 0x007F)) {
					bytearr[count++] = (byte) c;
				} else if (c > 0x07FF) {
					bytearr[count++] = (byte) (0xE0 | ((c >> 12) & 0x0F));
					bytearr[count++] = (byte) (0x80 | ((c >> 6) & 0x3F));
					bytearr[count++] = (byte) (0x80 | ((c) & 0x3F));
				} else {
					bytearr[count++] = (byte) (0xC0 | ((c >> 6) & 0x1F));
					bytearr[count++] = (byte) (0x80 | ((c) & 0x3F));
				}
			}
			writer.Write(bytearr, 0, utflen);
		}

		public static String readUTF (BinaryReader reader)
		{
			Boolean isNull = (reader.ReadByte()==1);
			if(isNull) return null;
			String result = null;
			int length = System.Net.IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int chunkSize = length/STRING_CHUNK_SIZE + 1;
			while (chunkSize > 0) {
				result += readShortUTF(reader);
				chunkSize--;
			}
			return result;
		}

		public static void readFully(byte[] b, int off, int len, BinaryReader reader)
		{
			int n = 0;
			while (n < len)
			{
				int count = reader.Read(b, off + n, len - n);
				if (count < 0)
					throw new Exception("End of file");
				n += count;
			}
		}

		private static String readShortUTF(BinaryReader reader) 
		{
			int utflen = System.Net.IPAddress.NetworkToHostOrder (reader.ReadInt16 ());
			byte[] bytearr = null;
			char[] chararr = null;
			bytearr = new byte[utflen];
			chararr = new char[utflen];
			int c, char2, char3;
			int count = 0;
			int chararr_count = 0;
			readFully(bytearr, 0, utflen, reader);
			while (count < utflen) {
				c = bytearr[count] & 0xff;
				if (c > 127)
					break;
				count++;
				chararr[chararr_count++] = (char) c;
			}
			while (count < utflen) {
				c = bytearr[count] & 0xff;
				switch (c >> 4) {
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
					/* 0xxxxxxx */
					count++;
					chararr[chararr_count++] = (char) c;
					break;
				case 12:
				case 13:
					/* 110x xxxx 10xx xxxx */
					count += 2;
					if (count > utflen)
						throw new Exception("malformed input: partial character at end");
					char2 = bytearr[count - 1];
					if ((char2 & 0xC0) != 0x80)
						throw new Exception("malformed input around byte " + count);
					chararr[chararr_count++] = (char) (((c & 0x1F) << 6) | (char2 & 0x3F));
					break;
				case 14:
					/* 1110 xxxx 10xx xxxx 10xx xxxx */
					count += 3;
					if (count > utflen)
						throw new Exception("malformed input: partial character at end");
					char2 = bytearr[count - 2];
					char3 = bytearr[count - 1];
					if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
						throw new Exception("malformed input around byte " + (count - 1));
					chararr[chararr_count++] = (char) (((c & 0x0F) << 12) | ((char2 & 0x3F) << 6) | ((char3 & 0x3F) << 0));
					break;
				default:
					/* 10xx xxxx, 1111 xxxx */
					throw new Exception("malformed input around byte " + count);
				}
			}
			// The number of chars produced may be less than utflen
			return new String(chararr, 0, chararr_count);
		}
		
        //public static void printBytes (byte[] bytes)
        //{
        //    foreach (byte b in bytes) {
        //        Console.Write (b);
        //        Console.Write (".");
        //    }
			
			
        //}
	}
}

