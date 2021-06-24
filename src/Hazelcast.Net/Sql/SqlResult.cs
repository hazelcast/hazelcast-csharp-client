using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    public class SqlResult: IAsyncEnumerable<SqlRow>, IAsyncDisposable
    {
        private readonly SqlService _sqlService;
        private readonly SerializationService _serializationService;
        private readonly SqlQueryId _queryId;

        /// <summary>
        /// The page size used for pagination
        /// </summary>
        private readonly uint _cursorBufferSize;

        /** If true, SqlResult is an object iterable, otherwise SqlRow iterable */
        private readonly bool _returnRawResult;

        private readonly Guid _clientId;

        public IAsyncEnumerator<SqlRow> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
