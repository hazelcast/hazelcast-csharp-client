// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models;

internal class PredicateConfigHolder
{
    public PredicateConfigHolder(string className, string sql, IData implementation)
    {
        ClassName = className;
        Sql = sql;
        Implementation = implementation;
    }

    public string ClassName { get; }

    public string Sql { get; }

    public IData Implementation { get; }

    public PredicateOptions ToPredicateConfig()
    {
        return ClassName != null 
            ? new PredicateOptions(ClassName) 
            : new PredicateOptions();
    }

    public static PredicateConfigHolder Of(PredicateOptions config)
    {
        return new PredicateConfigHolder(config.ClassName, config.Sql, null);
    }
}
