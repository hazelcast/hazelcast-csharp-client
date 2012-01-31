using System;
using Hazelcast.IO;

namespace Hazelcast.Impl
{
	public class ClientServiceException: DataSerializable {
		
		public static String className= "com.hazelcast.impl.ClientServiceException";
															
	    Exception exception;
		
		public Exception Exception {
			get {
				return this.exception;
			}
			set {
				exception = value;
			}
		}	
		
		static ClientServiceException() {
	    	Hazelcast.Client.IO.DataSerializer.register(className, typeof(ClientServiceException));
	    }
		
	    public ClientServiceException() {
	    
	    }
	
	    public void writeData(IDataOutput dout) {
	        dout.writeUTF(exception.Message);
	    }
	
	    public void readData(IDataInput din) {
	      	exception = new Exception(din.readUTF());
	    }
		
		public String javaClassName(){
			return className;
		}
	}
}

