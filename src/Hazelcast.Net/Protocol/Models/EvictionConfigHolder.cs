// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models;

internal class EvictionConfigHolder
{
    public EvictionConfigHolder(int size, string maxSizePolicy, string evictionPolicy, string comparatorClassName, IData comparator)
    {
        Size = size;
        MaxSizePolicy = maxSizePolicy;
        EvictionPolicy = evictionPolicy;
        ComparatorClassName = comparatorClassName;
        Comparator = comparator;
    }

    public int Size { get; }

    public string MaxSizePolicy { get; }

    public string EvictionPolicy { get; }

    public string ComparatorClassName { get; }

    public IData Comparator { get; }

    public EvictionOptions ToEvictionConfig()
    {
        var config = new EvictionOptions
        {
                Size = Size,
                MaxSizePolicy = Enums.ParseJava<MaxSizePolicy>(MaxSizePolicy),
                EvictionPolicy = Enums.ParseJava<EvictionPolicy>(EvictionPolicy)
        };

        if (ComparatorClassName != null)
            config.ComparatorClassName = ComparatorClassName;

        /*comparator*/

        return config;
    }

    public static EvictionConfigHolder Of(EvictionOptions config)
    {
        return new EvictionConfigHolder(
            config.Size, 
            config.MaxSizePolicy.ToJavaString(),
            config.EvictionPolicy.ToJavaString(),
            config.ComparatorClassName,
            null/*comparator*/);
    }
}