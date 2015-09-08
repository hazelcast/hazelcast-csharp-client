using System;
using Hazelcast.IO;

namespace Hazelcast.Core
{
	/// <summary>
	/// SQL Predicate
	/// </summary>
	public class SqlPredicate: IPredicate<object,object>
	{
		private String sql;
		
		public SqlPredicate (String sql)
		{
			this.sql = sql;
		}
		
	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="output"></param>
	    public void WriteData(IObjectDataOutput output)
	    {
            output.WriteUTF(sql);
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="input"></param>
	    public void ReadData(IObjectDataInput input)
	    {
            sql = input.ReadUTF();
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <returns></returns>
	    public string GetJavaClassName()
	    {
            return "com.hazelcast.query.SqlPredicate";
	    }
	}
}

