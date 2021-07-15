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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public interface ISqlService
    {
        Task<ISqlQueryResult> ExecuteQueryAsync(string sql, object[] parameters = null, SqlStatementOptions options = null);
        Task<long> ExecuteCommandAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default);
    }
}
