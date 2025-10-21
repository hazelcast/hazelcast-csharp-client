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
using System.Diagnostics.CodeAnalysis;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents options of a tiered-store.
/// </summary>
public class MemoryTierOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default capacity.
        /// </summary>
        public static readonly Capacity Capacity = new(256, MemoryUnit.MegaBytes);
    }
#pragma warning restore CA1034

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTierOptions"/> class.
    /// </summary>
    public MemoryTierOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTierOptions"/> class.
    /// </summary>
    /// <param name="options"></param>
    public MemoryTierOptions([NotNull] MemoryTierOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        Capacity = options.Capacity;
    }

    /// <summary>
    /// Gets or sets the memory capacity.
    /// </summary>
    public Capacity Capacity { get; set; } = Defaults.Capacity;

    /// <inheritdoc />
    public override string ToString()
    {
        return "MemoryTierConfig{"
               + "capacity=" + Capacity
               + '}';
    }

    /// <inheritdoc />
    public void WriteData([NotNull] IObjectDataOutput output)
    {
        output.WriteLong(Capacity.Value);
        output.WriteString(Capacity.Unit.ToJavaString());
    }

    /// <inheritdoc />
    public void ReadData([NotNull] IObjectDataInput input)
    {
        Capacity = Capacity.Of(input.ReadLong(), Enums.ParseJava<MemoryUnit>(input.ReadString()));
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MemoryTierConfig;
}
