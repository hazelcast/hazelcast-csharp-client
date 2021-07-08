// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;

namespace Hazelcast.Sql
{
    public class HazelcastSqlException : HazelcastException
    {
        // FIXME [Oleksii] clarify naming - client or member
        public Guid ClientId { get; }

        internal int ErrorCode { get; }

        internal HazelcastSqlException(Guid clientId, int errorCode, string message) : base(message)
        {
            ClientId = clientId;
            ErrorCode = errorCode;
        }

        internal HazelcastSqlException(Guid clientId, SqlErrorCode errorCode, string message) : this(clientId, (int)errorCode, message)
        { }

        internal HazelcastSqlException(SqlError error) : this(error.OriginatingMemberId, error.Code, error.Message)
        { }
    }
}