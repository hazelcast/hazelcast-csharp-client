using System;
using Hazelcast.IO;

namespace Hazelcast.Security
{
	public class UsernamePasswordCredentials : AbstractCredentials
	{
		
		private byte[] password;
		
		public static String className = "com.hazelcast.security.UsernamePasswordCredentials";
		
		public UsernamePasswordCredentials ()
		{
		}
		
		 public String getUsername() {
	        return getPrincipal();
	    }
	
	    public byte[] getPassword() {
	        return password;
	    }
	
	    public void setUsername(String username) {
	        setPrincipal(username);
	    }
	
	    public void setPassword(String password) {
			System.Text.ASCIIEncoding  encoding=new System.Text.ASCIIEncoding();
	        this.password = encoding.GetBytes(password);
	    }
		
		protected override void writeDataInternal(IDataOutput dout) {
	        dout.writeInt(password != null ? password.Length : 0);
	        if (password != null) {
	            dout.write(password);
	        }
	    }
	
	    protected override void readDataInternal(IDataInput din){
	        int s = din.readInt();
	        if (s > 0) {
	            password = new byte[s];
	            din.readFully(password);
	        }
	    }
		
		public override String javaClassName(){
			return className;
		}
	}
}

