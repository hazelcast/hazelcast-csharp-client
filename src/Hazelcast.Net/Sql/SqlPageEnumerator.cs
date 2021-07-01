using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    internal class SqlPageEnumerator: IEnumerator<SqlRow>
    {
        private readonly SerializationService _serializationService;
        private readonly SqlRowMetadata _rowMetadata;
        private readonly SqlPage _page;

        private int _currentIndex = -1;
        private Lazy<SqlRow> _currentLazy;

        public SqlRow Current => _currentLazy?.Value;
        public bool IsLastPage => _page.IsLast;

        public SqlPageEnumerator(SerializationService serializationService, SqlRowMetadata rowMetadata, SqlPage page)
        {
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _rowMetadata = rowMetadata ?? throw new ArgumentNullException(nameof(rowMetadata));
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public bool MoveNext()
        {
            if (_currentIndex >= _page.RowCount)
                return false;

            var currentIndex = _currentIndex;
            _currentLazy = new Lazy<SqlRow>(() => BuildRow(currentIndex));
            return true;
        }

        private SqlRow BuildRow(int index)
        {
            var values = Enumerable.Range(0, _page.RowCount)
                .Select(i => _serializationService.ToObject(_page[index, i]))
                .ToList();

            return new SqlRow(values, _rowMetadata);
        }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset() => throw new NotSupportedException();

        void IDisposable.Dispose()
        { }
    }
}