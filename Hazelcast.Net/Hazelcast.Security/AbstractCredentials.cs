using System;
using Hazelcast.IO;

namespace Hazelcast.Security
{
	public abstract class AbstractCredentials: Credentials, DataSerializable
	{
		
		private String endpoint;
    	private String principal;
		
		public AbstractCredentials ()
		{
		}
		
		public String getEndpoint(){
			return endpoint;
		}
	
		public void setEndpoint(String endpoint){
			this.endpoint = endpoint;
		}
		
		public String getPrincipal(){
			return principal;
		}
		
		public void setPrincipal(String principal) {
    	    this.principal = principal;
		}
		public void writeData(IDataOutput dout){
	        dout.writeUTF(principal);
	        dout.writeUTF(endpoint);
	        writeDataInternal(dout);
	    }
	
	    public void readData(IDataInput din) {
	        principal = din.readUTF();
	        endpoint = din.readUTF();
	        readDataInternal(din);
	    }
	
	    protected abstract void writeDataInternal(IDataOutput dout);
	
	    protected abstract void readDataInternal(IDataInput din);
	}
}

