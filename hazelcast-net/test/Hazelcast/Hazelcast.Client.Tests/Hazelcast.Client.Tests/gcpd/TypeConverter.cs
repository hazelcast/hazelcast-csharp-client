using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.IO;
using Hazelcast.Query;

namespace Hazelcast.Client.Tests.gcpd.HazelcastClientTest
{
    class TypeConverter : ITypeConverter
    {
        private const String SqlPredicateJavaName = "com.hazelcast.query.SqlPredicate";
        private const String DataWrapperJavaName = "com.hazelcast.gcpd.DataWrapper";

        public string getJavaName(Type type)
        {
            String javaName = null;

            if (type.Equals(typeof(SqlPredicate)))
            {
                javaName = SqlPredicateJavaName;
            }
            else if (type.Equals(typeof(DataWrapper)))
            {
                javaName = DataWrapperJavaName;
            }

            return javaName;
        }

        public Type getType(string javaName)
        {
            Type type = null;

            if (SqlPredicateJavaName.Equals(javaName))
            {
                type = typeof(SqlPredicate);
            }
            else if (DataWrapperJavaName.Equals(javaName))
            {
                type = typeof(DataWrapper);
            }

            return type;
        }
    }
}
