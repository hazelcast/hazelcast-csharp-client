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

namespace Hazelcast.Sql;

/// <summary>
/// Represents SQL options.
/// </summary>
public class SqlOptions
{
    /// <summary>
    /// Defines cache size for partition aware SQL queries.
    /// </summary>
    public int PartitionArgumentCacheSize { get; set; } = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlOptions"/> class.
    /// </summary>
    public SqlOptions(){}
    
    /// <summary>
    /// Defines threshold to cache for partition aware SQL queries. Eviction is triggered after threshold is exceeded. 
    /// </summary>
    public int PartitionArgumentCacheThreshold { get; set; } = 150;

    private SqlOptions(SqlOptions other)
    {
        PartitionArgumentCacheSize = other.PartitionArgumentCacheSize;
        PartitionArgumentCacheThreshold = other.PartitionArgumentCacheThreshold;
    }

    /// <summary>
    /// Clone the options.
    /// </summary>
    public SqlOptions Clone() => new SqlOptions(this);
}
