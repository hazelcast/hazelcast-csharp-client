using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Tests.Sandbox
{
    public class AsyncEnumerate
    {
        public async Task Test()
        {
            var sql = new SqlClient();

            await using var result = sql.Query<int>("SELECT INT");

            var cancellationSource = new CancellationTokenSource();

            await foreach (var i in result.WithCancellation(cancellationSource.Token))
            {
                Console.WriteLine(i);
            }
        }
    }

    public class SqlClient
    {
        private readonly SqlService _sql = new SqlService();

        public SqlResult<T> Query<T>(string query)
        {
            var fetching = _sql.FetchAsync<T>("query-id", query);
            return new SqlResult<T>(_sql, "query-id", fetching);
        }
    }

    public class SqlService
    {
        public Task<QueryResponse<T>> FetchAsync<T>(string queryId, string query)
        {
            return Task.FromResult(new QueryResponse<T> { IsLast = false, Items = new List<T>() });
        }

        public Task<QueryResponse<T>> FetchMoreAsync<T>(string queryId)
        {
            return Task.FromResult(new QueryResponse<T> { IsLast = true, Items = new List<T>() });
        }

        public Task CloseAsync(string queryId)
        {
            return Task.CompletedTask;
        }
    }

    public class QueryResponse<T>
    {
        public bool IsLast { get; set; }

        public IList<T> Items { get; set; }
    }

    public class SqlResult<T> : IAsyncEnumerable<T>, IAsyncDisposable
    {
        private readonly SqlService _sql;
        private readonly string _queryId;
        private readonly Task<QueryResponse<T>> _query;

        private int _enumerated;
        private int _closed;

        public SqlResult(SqlService sql, string queryId, Task<QueryResponse<T>> query)
        {
            _sql = sql;
            _queryId = queryId;
            _query = query;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            if (Interlocked.CompareExchange(ref _enumerated, 1, 0) == 1)
                throw new InvalidOperationException("Can only enumerate once.");

            return new AsyncEnumerator(this, cancellationToken);
        }

        private class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly SqlResult<T> _result;
            private readonly CancellationToken _cancellationToken;
            private readonly Task<QueryResponse<T>> _cancellationTask;
            private CancellationTokenRegistration _cancellationRegistration;

            private IList<T> _items;
            private bool _complete;
            private int _index;
            private T _current;

            public AsyncEnumerator(SqlResult<T> result, CancellationToken cancellationToken)
            {
                _result = result;
                _cancellationToken = cancellationToken;

                if (_cancellationToken != default)
                {
                    var tcs = new TaskCompletionSource<QueryResponse<T>>();
                    _cancellationTask = tcs.Task;
                    _cancellationRegistration = _cancellationToken.Register(() => tcs.TrySetResult(default));
                }
            }

            private async Task<QueryResponse<T>> WithCancellation(Task<QueryResponse<T>> task)
                => await (_cancellationToken == default ? task : await Task.WhenAny(task, _cancellationTask));

            public async ValueTask DisposeAsync()
            {
                await _result.CloseAsync();

                // oh my... https://github.com/dotnet/runtime/issues/19827
                // _cancellationRegistration.Unregister() is .NET Core+ only

                _cancellationRegistration.Dispose(); // this is why the field is not readonly
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_cancellationToken.IsCancellationRequested) return false;

                if (_items == null)
                {
                    if (_result._closed == 1) return false;
                    var response = await WithCancellation(_result._query);
                    if (response == null) return false;
                    _items = response.Items;
                    _complete = response.IsLast;
                    _index = 0;
                }
                else if (_index == _items.Count && !_complete)
                {
                    if (_result._closed == 1) return false;
                    var response = await WithCancellation(_result._sql.FetchMoreAsync<T>(_result._queryId));
                    if (response == null) return false;
                    _items = response.Items;
                    _complete = response.IsLast;
                    _index = 0;
                }

                if (_index == _items.Count) return false;

                _current = _items[_index++];

                return !_cancellationToken.IsCancellationRequested;
            }

            // as per .NET docs, Current is unspecified before the first call to MoveNext or if the last call to MoveNext returns false
            // so... this should be fine
            public T Current => _current;
        }

        private async ValueTask CloseAsync()
        {
            if (Interlocked.CompareExchange(ref _closed, 1, 0) == 1)
                return;

            try
            {
                await _sql.CloseAsync(_queryId);
            }
            catch
            {
                // nothing - shall we log?
            }
        }

        public async ValueTask DisposeAsync()
        {
            await CloseAsync();
        }
    }
}
