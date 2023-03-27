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
    private SqlOptions(SqlOptions other)
    {
        PartitionArgumentIndexCacheSize = other.PartitionArgumentIndexCacheSize;
        PartitionArgumentIndexCacheThreshold = other.PartitionArgumentIndexCacheThreshold;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlOptions"/> class.
    /// </summary>
    public SqlOptions(){}
    
    /// <summary>
    /// Defines cache size for partition aware SQL queries.
    /// </summary>
    public int PartitionArgumentIndexCacheSize { get; set; } = 100;

    /// <summary>
    /// Defines threshold to cache for partition aware SQL queries. Eviction is triggered after threshold is exceeded. 
    /// </summary>
    public int PartitionArgumentIndexCacheThreshold { get; set; } = 150;

    /// <summary>
    /// Clone the options.
    /// </summary>
    public SqlOptions Clone() => new SqlOptions(this);
}
