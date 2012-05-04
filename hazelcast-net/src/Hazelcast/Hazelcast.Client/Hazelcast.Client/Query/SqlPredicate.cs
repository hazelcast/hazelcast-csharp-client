using System;
using Hazelcast.IO;

namespace Hazelcast.Query
{
	public class SqlPredicate: Predicate, DataSerializable
	{
		private String sql;
		
		public SqlPredicate (String sql)
		{
			this.sql = sql;
		}
		
		public void writeData(IDataOutput dout){
			dout.writeUTF(sql);	
		}

   		public void readData(IDataInput din){
			sql = din.readUTF();
		}
		
		public String javaClassName(){
			return "com.hazelcast.query.SqlPredicate";
		}
	}
}

