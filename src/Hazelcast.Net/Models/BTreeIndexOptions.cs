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
#nullable enable

using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

// Configures indexing options for <see cref="IndexType.BTree"/> indexes.
public class BTreeIndexOptions : IIdentifiedDataSerializable
{
    private Capacity? _pageSize;

    /// <summary>
    /// Gets the default page size.
    /// </summary>
    public static Capacity DefaultPageSize { get; } = new(16, MemoryUnit.KiloBytes);

    /// <summary>
    /// Initializes a new instance of the <see cref="BTreeIndexOptions"/> class.
    /// </summary>
    public BTreeIndexOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BTreeIndexOptions"/> class.
    /// </summary>
    public BTreeIndexOptions(BTreeIndexOptions indexOptions)
    {
        PageSize = indexOptions.PageSize;
        MemoryTier = new MemoryTierOptions(indexOptions.MemoryTier);
    }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public Capacity PageSize
    {
        get => _pageSize ?? DefaultPageSize;
        set => _pageSize = value;
    }

    /// <summary>
    /// Gets or sets the memory tier options.
    /// </summary>
    public MemoryTierOptions MemoryTier { get; set; } = new();

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteLong(PageSize.Value);
        output.WriteString(PageSize.Unit.ToJavaString());
        output.WriteObject(MemoryTier);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        PageSize = Capacity.Of(input.ReadLong(), Enums.ParseJava<MemoryUnit>(input.ReadString()));
        MemoryTier = input.ReadObject<MemoryTierOptions>();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.BtreeIndexConfig;
}