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
using System;
using System.Diagnostics.Tracing;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models;

internal class ListenerConfigHolder
{
    // note: we need to have listenerImplementation as a ctor parameter even
    // though we don't support it, as codecs will expect the ctor to exist

    public ListenerConfigHolder(int listenerType, IData listenerImplementation, string className, bool includeValue, bool local)
        : this(((ListenerConfigType) listenerType).ThrowIfUndefined(), listenerImplementation, className, includeValue, local)
    { }

    public ListenerConfigHolder(ListenerConfigType listenerType, IData listenerImplementation, string className, bool includeValue, bool local)
    {
        ListenerType = listenerType;
        ClassName = className;
        IsIncludeValue = includeValue;
        IsLocal = local;
    }

    public string ClassName { get; }

    public ListenerConfigType ListenerType { get; }

    public bool IsIncludeValue { get; }

    public bool IsLocal { get; }

    public IData ListenerImplementation { get; }

    // FIXME missing mappings

    public T ToListenerConfig<T>()
        where T : ListenerOptions
    {
        if (ClassName == null)
            throw new InvalidOperationException("ClassName must not be null.");

        return ListenerType switch
        {
            //ListenerConfigType.Item => new ItemListenerConfig(ClassName, IsIncludeValue).MustBe<T>(),
            ListenerConfigType.Entry => new EntryListenerOptions(ClassName, IsLocal, IsIncludeValue).MustBe<T>(),
            //ListenerConfigType.SplitBrainProtection => new SplitBrainProtectionListenerConfig(ClassName).MustBe<T>(),
            //ListenerConfigType.CachePartitionLost => new CachePartitionLostListenerConfig(ClassName).MustBe<T>(),
            ListenerConfigType.MapPartitionLost => new MapPartitionLostListenerOptions(ClassName).MustBe<T>(),
            ListenerConfigType.Generic => new ListenerOptions(ClassName).MustBe<T>(),
            _ => null
        };
    }

    public static ListenerConfigHolder Of(ListenerOptions config)
    {
        var listenerType = config switch
        {
            //ItemListenerConfig => ListenerConfigType.Item,
            EntryListenerOptions => ListenerConfigType.Entry,
            //SplitBrainProtectionListenerConfig => ListenerConfigType.SplitBrainProtection,
            //CachePartitionLostListenerConfig => ListenerConfigType.CachePartitionLost,
            MapPartitionLostListenerOptions => ListenerConfigType.MapPartitionLost,
            _ => ListenerConfigType.Generic
        };

        return new ListenerConfigHolder(listenerType, null, config.ClassName, config.IncludeValue, config.Local);
    }
}