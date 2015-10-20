/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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

	    public SqlPredicate()
	    {
	    }

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

