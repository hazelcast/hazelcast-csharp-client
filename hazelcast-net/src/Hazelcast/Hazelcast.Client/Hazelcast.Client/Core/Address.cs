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
	        dout.write(ip);
	        dout.writeInt(port);
	    }
	
	    public void readData(IDataInput din) {
	        din.readFully(ip);
	        port = din.readInt();
	        // setHost();
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

