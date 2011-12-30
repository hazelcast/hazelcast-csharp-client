using System;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Client
{
	public class DataMessage<E>:Message<E>
	{
		readonly Data data;
		
		
		
		public Data Data {
			get {
				return this.data;
			}
		}		
		public DataMessage (Data data):base(default(E))
		{
			this.data = data;
		}
		
		public override E getMessageObject() {
	        return (E)IOUtil.toObject(data.Buffer);
	    }
		
		
	}
}

