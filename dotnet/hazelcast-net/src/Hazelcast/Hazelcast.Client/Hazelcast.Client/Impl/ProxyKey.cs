using System;
using Hazelcast.IO;

namespace Hazelcast.Client
{
	public class ProxyKey :SerializationHelper, DataSerializable
	{
		private String name;
        private Object key;
		
		public ProxyKey(){
			
		}
		
		public ProxyKey (String name, Object key)
		{
			this.name = name;
			this.key = key;
		}
		
		public String Name {
			get {
				return this.name;
			}
			set {
				name = value;
			}
		}

		public Object Key {
			get {
				return this.key;
			}
			set {
				key = value;
			}
		}
		
		public void writeData(IDataOutput dout){
			dout.writeUTF(name);
            bool keyNull = (key == null);
            dout.writeBoolean(keyNull);
            if (!keyNull) {
				writeObject(dout, key);
            }
		}

   		public void readData(IDataInput din){
			name = din.readUTF();
            bool keyNull = din.readBoolean();
            if (!keyNull) {
                key = readObject(din);
            }
		}
	}
}

