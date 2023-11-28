// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration for a tiered store.
/// </summary>
public class TieredStoreOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default value for whether tiered-store is enabled.
        /// </summary>
        public const bool Enabled = false;
    }
#pragma warning restore CA1034

    private bool _enabled = Defaults.Enabled;
    private MemoryTierOptions _memoryTierConfig = new();
    private DiskTierOptions _diskTierConfig = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TieredStoreOptions"/> class.
    /// </summary>
    public TieredStoreOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TieredStoreOptions"/> class.
    /// </summary>
    public TieredStoreOptions(TieredStoreOptions tieredStoreConfig)
    {
        _enabled = tieredStoreConfig._enabled;
        _memoryTierConfig = new MemoryTierOptions(tieredStoreConfig._memoryTierConfig);
        _diskTierConfig = new DiskTierOptions(tieredStoreConfig._diskTierConfig);
    }

    /// <summary>
    /// Whether tiered-store is enabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Gets or sets the memory tier configuration.
    /// </summary>
    public MemoryTierOptions MemoryTier
    {
        get => _memoryTierConfig;
        set => _memoryTierConfig = value;
    }

    /// <summary>
    /// Gets or sets the disk tier configuration.
    /// </summary>
    public DiskTierOptions DiskTier
    {
        get => _diskTierConfig;
        set => _diskTierConfig = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "TieredStoreConfig{"
                + "enabled=" + _enabled
                + ", memoryTierConfig=" + _memoryTierConfig
                + ", diskTierConfig=" + _diskTierConfig
                + '}';
    }

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(_enabled);
        output.WriteObject(_memoryTierConfig);
        output.WriteObject(_diskTierConfig);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _enabled = input.ReadBoolean();
        _memoryTierConfig = input.ReadObject<MemoryTierOptions>();
        _diskTierConfig = input.ReadObject<DiskTierOptions>();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.TieredStoreConfig;
}