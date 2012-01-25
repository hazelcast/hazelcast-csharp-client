using System;
using Hazelcast.IO;

namespace Hazelcast.Impl
{
	public class CMapEntry: DataSerializable, Hazelcast.Core.MapEntry<object,object>
	{
		private long cost = 0;
    	private long expirationTime = 0;
    	private long lastAccessTime = 0;
    	private long lastUpdateTime = 0;
    	private long lastStoredTime = 0;
    	private long creationTime = 0;
    	private long version = 0;
    	private int hits = 0;
    	private bool valid = true;
    	private String name = null;
    	private object key = null;
    	private object value = null;
		
		public static String className = "com.hazelcast.impl.CMap$CMapEntry";
	
		static CMapEntry ()
		{
			Hazelcast.Client.IO.DataSerializer.register(className, typeof(CMapEntry));
			
		}
		public CMapEntry ()
		{
		}
		
		 public long getCost() {
            return cost;
        }

        public long getCreationTime() {
            return creationTime;
        }

        public long getExpirationTime() {
            return expirationTime;
        }

        public long getLastUpdateTime() {
            return lastUpdateTime;
        }

        public int getHits() {
            return hits;
        }

        public long getLastAccessTime() {
            return lastAccessTime;
        }

        public long getLastStoredTime() {
            return lastStoredTime;
        }

        public long getVersion() {
            return version;
        }

        public bool isValid() {
            return valid;
        }

        public object getKey() {
            return key;
        }
		
		public object getValue() {
            return value;
        }
		
		public object setValue(object v) {
            return v;
        }
		
		public void writeData(IDataOutput dout){
            dout.writeLong(cost);
            dout.writeLong(expirationTime);
            dout.writeLong(lastAccessTime);
            dout.writeLong(lastUpdateTime);
            dout.writeLong(creationTime);
            dout.writeLong(lastStoredTime);
            dout.writeLong(version);
            dout.writeInt(hits);
            dout.writeBoolean(valid);
        }

        public void readData(IDataInput din){
            cost = din.readLong();
            expirationTime = din.readLong();
            lastAccessTime = din.readLong();
            lastUpdateTime = din.readLong();
            creationTime = din.readLong();
            lastStoredTime = din.readLong();
            version = din.readLong();
            hits = din.readInt();
            valid = din.readBoolean();
        }
		public String javaClassName(){
			return CMapEntry.className;
		}
		
		
	}
}

