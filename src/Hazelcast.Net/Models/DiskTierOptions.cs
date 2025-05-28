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
using Hazelcast.Serialization;
using System;
using Hazelcast.Core;
using Hazelcast.Configuration;

namespace Hazelcast.Models;

/// <summary>
/// Represents the disk-tier configuration of the tiered-store.
/// </summary>
public class DiskTierOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default value for whether disk tiered-store is enabled.
        /// </summary>
        public const bool Enabled = false;

        /// <summary>
        /// Gets the default device name.
        /// </summary>
        public const string DeviceName = Constants.LocalDeviceDefaultDeviceName;
    }
#pragma warning restore CA1034

    private string _deviceName = Defaults.DeviceName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskTierOptions"/> class.
    /// </summary>
    public DiskTierOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskTierOptions"/> class.
    /// </summary>
    public DiskTierOptions(DiskTierOptions diskTierConfig)
    {
        Enabled = diskTierConfig.Enabled;
        _deviceName = diskTierConfig._deviceName;
    }

    /// <summary>
    /// Whether disk-tier is enabled.
    /// </summary>
    public bool Enabled { get; set; } = Defaults.Enabled;

    /// <summary>
    /// Gets or sets the device name of this disk tier.
    /// </summary>
    public string DeviceName
    {
        get => _deviceName;
        set => _deviceName = value.ThrowIfNull();
    }

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(Enabled);
        output.WriteString(_deviceName);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        Enabled = input.ReadBoolean();
        _deviceName = input.ReadString();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.DiskTierConfig;

    /// <inheritdoc />
    public override string ToString()
    {
        return "DiskTierConfig{"
            + "enabled=" + Enabled
            + ", deviceName='" + _deviceName + '\''
            + '}';
    }
}
