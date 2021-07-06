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
