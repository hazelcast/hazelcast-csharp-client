namespace Hazelcast.Sql
{
    /// <summary>
    /// Error codes used in Hazelcast SQL.
    /// </summary>
    public enum SqlErrorCode
    {
        /// <summary>
        /// Generic error.
        /// </summary>
        Generic = -1,

        /// <summary>
        /// A network connection problem between members, or between a client and a member.
        /// </summary>
        ConnectionProblem = 1001,

        /// <summary>
        /// Query was cancelled due to user request.
        /// </summary>
        CancelledByUser = 1003,

        /// <summary>
        /// Query was cancelled due to timeout.
        /// </summary>
        Timeout = 1004,

        /// <summary>
        /// A problem with partition distribution.
        /// </summary>
        PartitionDistribution = 1005,

        /// <summary>
        /// An error caused by a concurrent destroy of a map.
        /// </summary>
        MapDestroyed = 1006,

        /// <summary>
        /// Map loading is not finished yet.
        /// </summary>
        MapLoadingInProgress = 1007,

        /// <summary>
        /// Generic parsing error.
        /// </summary>
        Parsing = 1008,

        /// <summary>
        /// An error caused by an attempt to query an index that is not valid.
        /// </summary>
        IndexInvalid = 1009,

        /// <summary>
        /// An error with data conversion or transformation.
        /// </summary>
        DataException = 2000
    }
}
