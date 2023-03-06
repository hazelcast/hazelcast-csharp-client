// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hazelcast.Sql
{
    /// <summary>
    /// Error codes used in Hazelcast SQL.
    /// </summary>
    internal enum SqlErrorCode
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
