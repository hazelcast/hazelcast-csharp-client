using System;

namespace Hazelcast.Sql
{
    /// <summary>
    /// SQL column metadata.
    /// </summary>
    public class SqlColumnMetadata
    {
        /// <summary>
        /// Column name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Column type.
        /// </summary>
        public SqlColumnType Type { get; }

        /// <summary>
        /// Column nullability. If true, the column values can be null.
        /// </summary>
        public bool IsNullable { get; }

        public SqlColumnMetadata(string name, SqlColumnType type, bool isNullable)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Type = type;
            IsNullable = isNullable;
        }

        public override string ToString() => $"{Name} {Type}";
    }
}