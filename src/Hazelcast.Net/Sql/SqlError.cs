// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    /// A server-side error that is propagated to the client.
    /// </summary>
    internal class SqlError
    {
        public int Code { get; }
        public string Message { get; }
        public Guid OriginatingMemberId { get; }

        public string Suggestion { get; }

        /// <summary>
        /// Creates a new instance of <see cref="SqlError"/> class.
        /// </summary>
        public SqlError(int code, string message, Guid originatingMemberId, bool hasSuggestion, string suggestion)
        {
            Code = code;
            Message = message;
            OriginatingMemberId = originatingMemberId;
            if (hasSuggestion) Suggestion = suggestion;
        }
    }
}