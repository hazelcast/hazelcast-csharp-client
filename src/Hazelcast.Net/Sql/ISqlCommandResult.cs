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
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Represents SQL command (CREATE, UPDATE, DELETE) with count of affected rows.
    /// </summary>
    public interface ISqlCommandResult: IAsyncDisposable
    {
        /// <summary>
        /// Task representing command execution.
        /// When successfully completed returns <see cref="long"/> as count of affected rows.
        /// </summary>
        public Task<long> Execution { get; }
    }
}