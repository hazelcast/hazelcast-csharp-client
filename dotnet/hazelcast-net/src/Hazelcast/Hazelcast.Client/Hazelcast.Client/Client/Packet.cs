using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
namespace Hazelcast.Client
{
	public class Packet
	{
		public static byte[] HEADER = new byte[3] { (byte)72, (byte)90, (byte)67 };

		public byte[] key;

		public byte[] value;

		public String name;

		public byte operation;

		public int threadId;

		public long ttl = -1;

		public long timeout = -1;

		public long longValue;

		public byte responseType = 2;

		public long callId = 9;

		public byte PACKET_VERSION = 6;

		public void write (Stream stream)
		{
			MemoryStream header = new MemoryStream ();
			writeHeader (header);
			byte[] headerInBytes = header.ToArray ();
			MemoryStream body = new MemoryStream ();
			
			using (BinaryWriter writer = new BinaryWriter (body)) {
				writer.Write (System.Net.IPAddress.HostToNetworkOrder (headerInBytes.Length));
				if(key==null){
					writer.Write(0);
				}else{
					writer.Write (System.Net.IPAddress.HostToNetworkOrder (key.Length));
				}
				if(value==null){
					writer.Write (0);	
				} else{
					writer.Write (System.Net.IPAddress.HostToNetworkOrder (value.Length));
				}
				writer.Write (PACKET_VERSION);
				writer.Write (headerInBytes);
				if(key!=null)
					writer.Write (key);
				if(value!=null)
					writer.Write (value);
				byte[] packetInBytes = body.ToArray ();
				stream.Write (packetInBytes, 0, packetInBytes.Length);
			}
		}

		public void read (Stream stream)
		{
			BinaryReader reader = new BinaryReader (stream);

			int headerSize = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int keySize = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int valueSize = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int packetVersion = reader.ReadByte ();
			if (packetVersion != PACKET_VERSION) {
				//throw exception, I don't know how. 
			}
			readHeader (reader);
			this.key = new byte[keySize];
			reader.Read (this.key, 0, keySize);
			this.value = new byte[valueSize];
			reader.Read (this.value, 0, valueSize);
		}

		public void readHeader (BinaryReader reader)
		{
			this.operation = reader.ReadByte ();
			int blockId = reader.ReadInt32 ();
			int threadId = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			byte booleans = reader.ReadByte ();
			if(isTrue(booleans, 1)){
				timeout = IPAddress.NetworkToHostOrder (reader.ReadInt64 ());
			}
			if(isTrue(booleans, 2)){
				ttl = IPAddress.NetworkToHostOrder (reader.ReadInt64 ());
			}
			if(isTrue(booleans, 4)){
				longValue = IPAddress.NetworkToHostOrder (reader.ReadInt64 ());
			}
			this.callId = IPAddress.NetworkToHostOrder (reader.ReadInt64 ());
			this.responseType = reader.ReadByte ();
			int nameLength = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			if (nameLength > 0) {
				byte[] b = new byte[nameLength];
				reader.Read (b, 0, nameLength);
				this.name = System.Text.Encoding.ASCII.GetString (b);
			}
			byte indexCount = reader.ReadByte ();
			int keyPartitionHash = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			int valuePartitionHash = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
		}


		public void writeHeader (MemoryStream ms)
		{
			using (BinaryWriter writer = new BinaryWriter (ms)) {
				writer.Write (operation);
				writer.Write (System.Net.IPAddress.HostToNetworkOrder (1));
				writer.Write (System.Net.IPAddress.HostToNetworkOrder (threadId));
				byte booleans = 0;
				if(timeout!=-1){
					booleans = setTrue(booleans, 1);	
				}
				if(ttl!=-1){
					booleans = setTrue(booleans, 2);	
				}
				if(longValue!=long.MinValue){
					booleans = setTrue(booleans, 4);	
				}
				booleans = setTrue(booleans, 6); //client = true
				booleans = setTrue(booleans, 7); //lockAddressNull == true
				writer.Write ((byte)booleans); // eskiden 192 idi
				if(timeout!=-1){
					writer.Write(System.Net.IPAddress.HostToNetworkOrder((long)timeout));
				}
				if(ttl!=-1){
					writer.Write(System.Net.IPAddress.HostToNetworkOrder((long)ttl));
				}
				if(longValue!=long.MinValue){
					writer.Write(System.Net.IPAddress.HostToNetworkOrder((long)longValue));
				}
				
				writer.Write (System.Net.IPAddress.HostToNetworkOrder ((long)callId));
				writer.Write (responseType);
				int nameLen = 0;
				byte[] b2 = null;
				if(name!=null){
					b2 = System.Text.Encoding.ASCII.GetBytes (name);
					nameLen = b2.Length;
				}
				writer.Write (System.Net.IPAddress.HostToNetworkOrder (nameLen));
				if(nameLen > 0)
					writer.Write (b2);
				writer.Write ((byte)0);
				
				writer.Write(System.Net.IPAddress.HostToNetworkOrder ((int)-1));
				writer.Write(System.Net.IPAddress.HostToNetworkOrder ((int)-1));
			}
			
			
		}
		
		public override string ToString ()
		{
			return string.Format ("[Packet] " + (ClusterOperation)operation);
		}
		public void set(String name, ClusterOperation operation,
                    byte[] key, byte[] value) {
        this.name = name;
        this.operation = (byte)operation;
        this.key = key;
        this.value = value;
    }
		
		private byte setTrue(byte number, int index){
			return (byte) (number | POWERS[index]);
		}
		private bool isTrue(byte number, int index){
			return  (number & POWERS[index])!=0;
		}
		
		private static byte[] POWERS = new byte[]{
            (byte)(1 << 0), (byte)(1 << 1), (byte)(1 << 2), (byte)(1 << 3), 
            (byte)(1 << 4), (byte)(1 << 5), (byte)(1 << 6), (byte)(1 << 7)
    };
		
		
	}
}

