using System;
using Hazelcast.IO;
using System.Net;

namespace Hazelcast.Core
{
	public class Address: DataSerializable
	{
		
		private byte[] ip = new byte[4];

    	private int port = -1;
		
		private String host;
		
		private IPEndPoint ipEndPoint;
		
		private readonly byte IPv4 = 4;
    	
		public Address ()
		{
		}
		
		public Address (IPEndPoint ipEndPoint)
		{
			this.ip = ipEndPoint.Address.GetAddressBytes();
			
			this.port = ipEndPoint.Port;
			this.ipEndPoint = ipEndPoint;
		}
		
		public void writeData(IDataOutput dout) {
	        dout.writeInt(port);
			dout.writeByte(IPv4);
			getHost();
			if(host == null){
				dout.writeInt(0);
			}
			else{
				
				System.Text.UTF8Encoding  encoding=new System.Text.UTF8Encoding();
    			Byte[] bytes = encoding.GetBytes(host);
				dout.writeInt(bytes.Length);
				dout.write(bytes);
			}
			
			dout.write(ip);
	        
	    }
	
	    public void readData(IDataInput din) {
	        port = din.readInt();
			din.readByte();
			int length = din.readInt();
			Byte[] bytes = new Byte[length];
			din.readFully(bytes);
			System.Text.UTF8Encoding  encoding=new System.Text.UTF8Encoding();
			host = encoding.GetString(bytes);
			string[] split = host.Split('.');
			for(int i=0;i<split.Length;i++){
				ip[i] = Byte.Parse(split[i]);
			}
	    }
		
		public String getHost(){
			if(host==null){
				host = toString(ip);
			}
			return host;
		}
		
		public int getPort(){
			return port;
		}
		
		public System.Net.IPEndPoint getIPEndPoint(){
			if(ipEndPoint==null){
				ipEndPoint = new IPEndPoint(new IPAddress(ip), port);
			}
			return ipEndPoint;			
		}
		
		public static String toString(byte[] ip) {
       	 	return (ip[0] & 0xff) + "." + (ip[1] & 0xff) + "." + (ip[2] & 0xff) + "." + (ip[3] & 0xff);
    	}
	}
}

