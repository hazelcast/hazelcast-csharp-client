using System;
using Hazelcast.IO;

namespace Hazelcast.Impl
{
	public class ClientServiceException: DataSerializable {
		Exception exception;
		
		public Exception Exception {
			get {
				return this.exception;
			}
			set {
				exception = value;
			}
		}	
		
	    public ClientServiceException() {
	    
	    }
	
	    public void writeData(IDataOutput dout) {
	        dout.writeUTF(exception.Message);
	    }
	
	    public void readData(IDataInput din)
	    {
	        bool isDS=din.readBoolean();
	      	exception = new Exception("Exception on Server: "+din.readUTF());
	    }
	}
}

