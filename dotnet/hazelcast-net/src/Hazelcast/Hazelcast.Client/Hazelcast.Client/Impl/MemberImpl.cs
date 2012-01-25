using System;
using Hazelcast.IO;
using Hazelcast.Core;
using System.Text;


namespace Hazelcast.Impl
{
	public class MemberImpl: Member, DataSerializable 
	{
		protected Address address;
    	protected NodeType nodeType;
		protected String uuid;
		
		public static String className = "com.hazelcast.impl.MemberImpl";
		
		static MemberImpl ()
		{
			Hazelcast.Client.IO.DataSerializer.register(className, typeof(MemberImpl));
			
		}
		
		public MemberImpl ()
		{
		}
		public void readData(IDataInput din){
	        address = new Address();
	        address.readData(din);
	        nodeType = (NodeType)(din.readInt());
	        if (din.readBoolean()) {
	            uuid = din.readUTF();
	        }
	    }
	
	    public void writeData(IDataOutput dout){
	        address.writeData(dout);
	        dout.writeInt((int)nodeType);
	        bool hasUuid = uuid != null;
	        dout.writeBoolean(hasUuid);
	        if (hasUuid) {
	            dout.writeUTF(uuid);
	        }
	    }
		
		public String javaClassName(){
			return className;
		}
		
		public bool isLiteMember(){
			return nodeType.Equals(NodeType.LITE_MEMBER);
		}
		
		public System.Net.IPEndPoint getIPEndPoint(){
			return address.getIPEndPoint();
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder("Member [");
	        sb.Append(address.getHost());
	        sb.Append(":");
	        sb.Append(address.getPort());
	        sb.Append("]");

	        if (nodeType == NodeType.LITE_MEMBER) {
	            sb.Append(" lite");
	        }
	        return sb.ToString();
		}
		

	}
}

