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
using System.IO;
using System;
using Hazelcast.Core;

/* Unmerged change from project 'Hazelcast.Net (net7.0)'
Before:
using Hazelcast.Configuration;
After:
using Hazelcast.Configuration;
using Hazelcast;
using Hazelcast.Models;
using Hazelcast.NearCaching;
*/
using Hazelcast.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.NearCaching;

/// <summary>
/// Configuration for storing and preloading Near Cache keys.
/// <p>Preloader re-populates Near Cache after client/server restart to provide fast access.</p>
/// </summary>
public class NearCachePreloaderOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default initial delay for the Near Cache key storage.
        /// </summary>
        public const int StoreInitialDelaySeconds = 600;

        /// <summary>
        /// Gets the default interval for the Near Cache key storage (in seconds).
        /// </summary>
        public const int StoreIntervalSeconds = 600;
    }
#pragma warning restore CA1034

    private bool _enabled;
    private string _directory = "";
    private int _storeInitialDelaySeconds = Defaults.StoreInitialDelaySeconds;
    private int _storeIntervalSeconds = Defaults.StoreIntervalSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCachePreloaderOptions"/>.
    /// </summary>
    public NearCachePreloaderOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCachePreloaderOptions"/>.
    /// </summary>
    public NearCachePreloaderOptions([NotNull] NearCachePreloaderOptions nearCachePreloaderConfig)
        : this(nearCachePreloaderConfig._enabled, nearCachePreloaderConfig._directory)
    {
        _storeInitialDelaySeconds = nearCachePreloaderConfig._storeInitialDelaySeconds;
        _storeIntervalSeconds = nearCachePreloaderConfig._storeIntervalSeconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCachePreloaderOptions"/>.
    /// </summary>
    /// <param name="directory"></param>
    public NearCachePreloaderOptions(string directory)
        : this(true, directory)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCachePreloaderOptions"/>.
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="directory"></param>
    public NearCachePreloaderOptions(bool enabled, string directory)
    {
        _enabled = enabled;
        _directory = directory.ThrowIfNull(nameof(directory));
    }

    /// <summary>
    /// Either preloader is enabled or not.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Enables or disables preloader.
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public NearCachePreloaderOptions SetIsEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }
    
    /// <summary>
    /// Directory where keys are stored.
    /// </summary>
    public string Directory
    {
        get => _directory;
        set => _directory = value.ThrowIfNull();
    }

    /// <summary>
    /// Sets directory where keys are stored.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public NearCachePreloaderOptions SetDirectory(string directory)
    {
        Directory = directory;
        return this;
    }
    /// <summary>
    /// Initial delay for the Near Cache key storage (in seconds).
    /// </summary>
    public int StoreInitialDelaySeconds
    {
        get => _storeInitialDelaySeconds;
        set => _storeInitialDelaySeconds = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Sets initial delay for the Near Cache key storage (in seconds).
    /// </summary>
    /// <param name="storeInitialDelaySeconds"></param>
    /// <returns></returns>
    public NearCachePreloaderOptions SetStoreInitialDelaySeconds(int storeInitialDelaySeconds)
    {
        StoreInitialDelaySeconds = storeInitialDelaySeconds;
        return this;
    }

    /// <summary>
    /// Interval for the Near Cache key storage (in seconds).
    /// </summary>
    public int StoreIntervalSeconds
    {
        get => _storeIntervalSeconds;
        set => _storeIntervalSeconds = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Sets interval for the Near Cache key storage (in seconds).
    /// </summary>
    /// <param name="storeIntervalSeconds"></param>
    /// <returns></returns>
    public NearCachePreloaderOptions SetStoreIntervalSeconds(int storeIntervalSeconds)
    {
        StoreIntervalSeconds = storeIntervalSeconds;
        return this;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.NearCachePreloaderConfig;

    /// <inheritdoc />
    public void WriteData([NotNull] IObjectDataOutput output)
    {
        output.WriteBoolean(_enabled);
        output.WriteString(_directory);
        output.WriteInt(_storeInitialDelaySeconds);
        output.WriteInt(_storeIntervalSeconds);
    }

    /// <inheritdoc />
    public void ReadData([NotNull] IObjectDataInput input)
    {
        _enabled = input.ReadBoolean();
        _directory = input.ReadString();
        _storeInitialDelaySeconds = input.ReadInt();
        _storeIntervalSeconds = input.ReadInt();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "NearCachePreloaderConfig{"
            + "enabled=" + _enabled
            + ", directory=" + _directory
            + ", storeInitialDelaySeconds=" + _storeInitialDelaySeconds
            + ", storeIntervalSeconds=" + _storeIntervalSeconds
            + '}';
    }
}
