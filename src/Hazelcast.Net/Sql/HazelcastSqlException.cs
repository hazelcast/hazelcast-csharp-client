using System;
using Hazelcast.Exceptions;

namespace Hazelcast.Sql
{
    public class HazelcastSqlException: HazelcastException
    {
        public HazelcastSqlException(Guid clientId, SqlErrorCode errorCode, string message) { }

        // FIXME [Oleksii] save details
        public HazelcastSqlException(SqlError error) { }
    }
}
