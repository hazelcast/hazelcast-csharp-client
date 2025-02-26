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
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents options for a map eviction.
/// </summary>
public class EvictionOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default size.
        /// </summary>
        public const int Size = 10000;

        /// <summary>
        /// Gets the default max-size policy.
        /// </summary>
        public const MaxSizePolicy MaxSizePolicy = Models.MaxSizePolicy.PerNode;

        /// <summary>
        /// Gets the default eviction policy.
        /// </summary>
        public const EvictionPolicy EvictionPolicy = NearCaching.EvictionPolicy.None;

        /// <summary>
        /// Gets the default comparator class name.
        /// </summary>
        public const string ComparatorClassName = null;
    }
#pragma warning restore CA1034

    private int _size = Defaults.Size;
    private EvictionPolicy _evictionPolicy = Defaults.EvictionPolicy;
    private string _comparatorClassName = Defaults.ComparatorClassName;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvictionOptions"/> class.
    /// </summary>
    public EvictionOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EvictionOptions"/> class.
    /// </summary>
    public EvictionOptions(EvictionOptions config)
    {
        _size = config._size;
        MaxSizePolicy = config.MaxSizePolicy;
        _evictionPolicy = config._evictionPolicy;
        _comparatorClassName = config._comparatorClassName;
    }

    /// <summary>
    /// Gets or sets the size which is used by the <see cref="MaxSizePolicy"/>.
    /// </summary>
    public int Size
    {
        get => _size;
        set => _size = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Gets or sets the <see cref="MaxSizePolicy"/> of this eviction configuration.
    /// </summary>
    public MaxSizePolicy MaxSizePolicy { get; set; } = Defaults.MaxSizePolicy;

    /// <summary>
    /// Gets or sets the <see cref="EvictionPolicy"/> of this eviction configuration.
    /// </summary>
    public EvictionPolicy EvictionPolicy
    {
        get => _evictionPolicy;
        set => _evictionPolicy = value.ThrowIfUndefined();
    }

    /// <summary>
    /// Gets the <see cref="EvictionStrategyType"/> of this eviction configuration.
    /// </summary>
    public EvictionStrategyType EvictionStrategyType => EvictionStrategyType.SamplingBasedEviction;

    /// <summary>
    /// Gets or sets the class name of the configured EvictionPolicyComparator implementation.
    /// </summary>
    public string ComparatorClassName
    {
        get => _comparatorClassName;
        set => _comparatorClassName = value.ThrowIfNullNorWhiteSpace();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.EvictionConfig;


    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteInt(_size);
        output.WriteString(MaxSizePolicy.ToJavaString());
        output.WriteString(_evictionPolicy.ToJavaString());
        output.WriteString(_comparatorClassName);
        output.WriteObject(null/*_comparator*/);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _size = input.ReadInt();
        MaxSizePolicy = Enums.ParseJava<MaxSizePolicy>(input.ReadString());
        _evictionPolicy = Enums.ParseJava<EvictionPolicy>(input.ReadString());
        _comparatorClassName = input.ReadString();
        _/*comparator*/ = input.ReadObject<object>();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "EvictionConfig{"
               + "size=" + _size
               + ", maxSizePolicy=" + MaxSizePolicy
               + ", evictionPolicy=" + _evictionPolicy
               + ", comparatorClassName=" + _comparatorClassName
               + ", comparator=" + "<not-supported>"/*_comparator*/
               + '}';
    }
}
