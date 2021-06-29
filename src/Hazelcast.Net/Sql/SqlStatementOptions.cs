using System;

namespace Hazelcast.Sql
{
    public class SqlStatementOptions
    {
        public static readonly SqlStatementOptions Default = new SqlStatementOptions();

        private string _schema = null;
        private TimeSpan _timeout = TimeSpan.Zero;
        private int _cursorBufferSize = 4096;

        /// <summary>
        /// The schema name.
        /// The default value is <c>null</c> meaning only the default search path is used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The engine will try to resolve the non-qualified object identifiers from the statement in the
        /// given schema. If not found, the default search path will be used, which looks for objects in the predefined
        /// schemas 'partitioned' and 'public'.
        /// </para>
        /// <para>
        /// The schema name is case sensitive. For example, 'foo' and 'Foo' are different schemas
        /// </para>
        /// </remarks>
        public string Schema
        {
            get => _schema;
            set
            {
                if (value != null && string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Empty or whitespace schema is not allowed.");

                _schema = value;
            }
        }

        /// <summary>
        /// <para>
        /// Statement execution timeout.
        /// If the timeout is reached for a running statement, it will be cancelled forcefully.
        /// Defaults to <see cref="TimeSpan.Zero"/>.
        /// </para>
        /// <para>
        /// <see cref="TimeSpan.Zero"/> means that the timeout in server config will be used.
        /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> means no timeout.
        /// </para>
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                if (value < TimeSpan.Zero && value != System.Threading.Timeout.InfiniteTimeSpan)
                    throw new ArgumentException("Negative timeouts apart from Timeout.InfiniteTimeSpan are not allowed.");

                _timeout = value;
            }
        }

        /// <summary>
        /// The cursor buffer size (measured in the number of rows).
        /// Only positive values are allowed.
        /// Defaults to <c>4096</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a statement is submitted for execution, a <see cref="SqlResult"/> is returned as a result. When rows are ready to be
        /// consumed, they are put into an internal buffer of the cursor. This parameter defines the maximum number of rows in
        /// that buffer. When the threshold is reached, the backpressure mechanism will slow down the execution, possibly to a
        /// complete halt, to prevent out-of-memory.
        /// </para>
        /// <para>
        /// The default value is expected to work well for most workloads. A bigger buffer size may give you a slight performance
        /// boost for queries with large result sets at the cost of increased memory consumption.
        /// </para>
        /// </remarks>
        public int CursorBufferSize
        {
            get => _cursorBufferSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Cursor buffer size must be positive.");

                _cursorBufferSize = value;
            }
        }

        /// <summary>
        /// Expected result type of SQL query.
        /// Defaults to <see cref="SqlResultType.Any"/>.
        /// </summary>
        public SqlResultType ExpectedResultType { get; set; } = SqlResultType.Any;
    }
}
