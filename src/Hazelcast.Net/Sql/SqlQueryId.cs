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

using System;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Cluster-wide unique SQL query ID.
    /// </summary>
    internal class SqlQueryId
    {
        /// <summary>
        /// Member ID: most significant bits
        /// </summary>
        public long MemberIdHigh { get; }

        /// <summary>
        /// Member ID: least significant bits.
        /// </summary>
        public long MemberIdLow { get; }

        /// <summary>
        /// Local ID: most significant bits.
        /// </summary>
        public long LocalIdHigh { get; }

        /// <summary>
        /// Local ID: least significant bits.
        /// </summary>
        public long LocalIdLow { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SqlQueryId"/> class.
        /// </summary>
        public SqlQueryId(long memberIdHigh, long memberIdLow, long localIdHigh, long localIdLow)
        {
            MemberIdHigh = memberIdHigh;
            MemberIdLow = memberIdLow;
            LocalIdHigh = localIdHigh;
            LocalIdLow = localIdLow;
        }

        public SqlQueryId(Guid memberId, Guid localId)
        {
            var memberBytes = memberId.ToByteArray();
            (MemberIdHigh, MemberIdLow) = (BitConverter.ToInt64(memberBytes, 0), BitConverter.ToInt64(memberBytes, sizeof(long)));

            var localBytes = localId.ToByteArray();
            (LocalIdHigh, LocalIdLow) = (BitConverter.ToInt64(localBytes, 0), BitConverter.ToInt64(localBytes, sizeof(long)));
        }

        public static SqlQueryId FromMemberId(Guid clientId)
        {
            var localId = Guid.NewGuid();
            return new SqlQueryId(clientId, localId);
        }
    }
}
