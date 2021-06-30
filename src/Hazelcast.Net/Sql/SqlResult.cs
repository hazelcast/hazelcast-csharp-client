using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public class SqlResult : IAsyncEnumerable<SqlRow>, IEnumerable<SqlRow>, IAsyncDisposable
    {
        private readonly SqlService _sqlService;
        private readonly SqlQueryId _queryId;

        private readonly SqlRowMetadata _rowMetadata;
        private readonly SqlPage _page;
        private readonly long _updateCount;

        public SqlResult(SqlService sqlService, SqlQueryId queryId,
            SqlRowMetadata rowMetadata, SqlPage page, long updateCount
        )
        {
            _sqlService = sqlService;
            _queryId = queryId;

            _rowMetadata = rowMetadata;
            _page = page;
            _updateCount = updateCount;
        }

        public IEnumerator<SqlRow> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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