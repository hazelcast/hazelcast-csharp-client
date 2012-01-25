using System;
using Hazelcast.IO;
using System.Net;

namespace Hazelcast.Core
{
	public class Address: DataSerializable
	{
		
		private byte[] ip = new byte[4];

    	private int port = -1;
		
		public static String className = "com.hazelcast.nio.Address";
		
		private String host;
		
		private IPEndPoint ipEndPoint;
		
		static Address ()
		{
			Hazelcast.Client.IO.DataSerializer.register(className, typeof(Address));
			
		}
		
		public Address ()
		{
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
		
		public String javaClassName(){
			return className;	
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
			//if(host==null){
			//	host = toString(ip);
			//}	
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

