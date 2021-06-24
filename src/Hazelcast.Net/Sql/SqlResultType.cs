using System.Runtime.Serialization;

namespace Hazelcast.Sql
{
    // FIXME [Oleksii] check if [Flags] will work
    public enum SqlResultType
    {
        /// <summary>
        /// The statement may produce either rows or an update count.
        /// </summary>
        [EnumMember(Value = "ANY")]
        Any = 0,

        /// <summary>
        /// The statement must produce rows. An exception is thrown if the statement produces an update count.
        /// </summary>
        [EnumMember(Value = "ROWS")]
        Rows = 1,

        /// <summary>
        /// The statement must produce an update count. An exception is thrown if the statement produces rows.
        /// </summary>
        [EnumMember(Value = "UPDATE_COUNT")]
        UpdateCount = 2
    }
}
