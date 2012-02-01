using System;
using Hazelcast.IO;

namespace Hazelcast.Client.Examples
{
	public class MyCSharpClass: Hazelcast.IO.DataSerializable
	{
		String field1 = "";
		int field2;
		
		public MyCSharpClass ()
		{
		}
		
		public MyCSharpClass (String f1, int f2)
		{
			this.field1 = f1;
			this.field2 = f2;
		}
		
		public void writeData(IDataOutput dout){
			dout.writeUTF(field1);
			dout.writeInt(field2);
		}

   		public void readData(IDataInput din){
			field1 = din.readUTF();
			field2 = din.readInt();
		}
	}
}

