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
using Hazelcast.Configuration;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents a hot restart configuration.
/// </summary>
[Obsolete("Use DataPersistenceConfig.", false)]
public class HotRestartOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default enabled value.
        /// </summary>
        public const bool Enabled = false;

        /// <summary>
        /// Gets the default fsync value.
        /// </summary>
        public const bool Fsync = false;
    }
#pragma warning restore CA1034

    /// <summary>
    /// Initializes a new instance of the <see cref="HotRestartOptions"/> class.
    /// </summary>
    public HotRestartOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotRestartOptions"/> class.
    /// </summary>
    public HotRestartOptions(HotRestartOptions hotRestartConfig)
    {
        Enabled = hotRestartConfig.Enabled;
        Fsync = hotRestartConfig.Fsync;
    }

    /// <summary>
    /// Whether hot restart is enabled.
    /// </summary>
    public bool Enabled { get; set; } = Defaults.Enabled;

    /// <summary>
    /// Sets whether hot restart is enabled.
    /// </summary>
    /// <param name="enabled">Whether hot restart is enabled.</param>
    /// <returns>This instance.</returns>
    public HotRestartOptions SetIsEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    /// <summary>
    /// Whether disk writes should be followed by an fsync system call.
    /// </summary>
    public bool Fsync { get; set; } = Defaults.Fsync;

    /// <summary>
    /// Sets whether disk writes should be followed by an fsync system call.
    /// </summary>
    /// <param name="fsync">Whether disk writes should be followed by an fsync system call.</param>
    /// <returns>This instance.</returns>
    public HotRestartOptions SetIsFsync(bool fsync)
    {
        Fsync = fsync;
        return this;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.HotRestartConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(Enabled);
        output.WriteBoolean(Fsync);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        Enabled = input.ReadBoolean();
        Fsync = input.ReadBoolean();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "HotRestartConfig{"
           + "enabled=" + Enabled
           + ", fsync=" + Fsync
           + '}';
    }
}
