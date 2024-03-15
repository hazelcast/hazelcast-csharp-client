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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration of a ringbuffer.
/// </summary>
public class RingbufferOptions : IIdentifiedDataSerializable, INamedOptions
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
        public const int Capacity = 10 * 1000;

        /// <summary>
        /// Gets the default synchronous backup count.
        /// </summary>
        public const int SyncBackupCount = 1;

        /// <summary>
        /// Gets the default asynchronous backup count.
        /// </summary>
        public const int AsyncBackupCount = 0;

        /// <summary>
        /// Gets the default time-to-live.
        /// </summary>
        public const int TtlSeconds = 0;

        /// <summary>
        /// Gets the default in-memory format.
        /// </summary>
        public const InMemoryFormat InMemoryFormat = Core.InMemoryFormat.Binary;
    }
#pragma warning restore CA1034


    private string _name;
    private int _capacity = Defaults.Capacity;
    private int _backupCount = Defaults.SyncBackupCount;
    private int _asyncBackupCount = Defaults.AsyncBackupCount;
    private int _timeToLiveSeconds = Defaults.TtlSeconds;
    private InMemoryFormat _inMemoryFormat = Defaults.InMemoryFormat;
    private RingbufferStoreOptions _ringbufferStoreOptions = new() { Enabled = false };
    private string _splitBrainProtectionName;
    private MergePolicyOptions _mergePolicyOptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferOptions"/> class.
    /// </summary>
    public RingbufferOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferOptions"/> class.
    /// </summary>
    /// <param name="name">The name of the ringbuffer.</param>
    public RingbufferOptions(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferOptions"/> class.
    /// </summary>
    /// <param name="options">Another instance.</param>
    public RingbufferOptions(RingbufferOptions options)
    {
        options.ThrowIfNull();

        _name = options._name;
        _capacity = options._capacity;
        _backupCount = options._backupCount;
        _asyncBackupCount = options._asyncBackupCount;
        _timeToLiveSeconds = options._timeToLiveSeconds;
        _inMemoryFormat = options._inMemoryFormat;
        if (options._ringbufferStoreOptions != null)
            _ringbufferStoreOptions = new RingbufferStoreOptions(options._ringbufferStoreOptions);
        _mergePolicyOptions = new MergePolicyOptions(options._mergePolicyOptions);
        _splitBrainProtectionName = options._splitBrainProtectionName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferOptions"/> class.
    /// </summary>
    /// <param name="name">The name of the ringbuffer.</param>
    /// <param name="options">Another instance.</param>
    public RingbufferOptions(string name, RingbufferOptions options)
        : this(options)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the name of the ringbuffer.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value.ThrowIfNullNorWhiteSpace();
    }

    /*
     * Gets the capacity of the ringbuffer.
     * <p>
     * The capacity is the total number of items in the ringbuffer. The items
     * will remain in the ringbuffer, but the oldest items will eventually be
     * overwritten by the newest items.
     *
     * @return the capacity
     */
    public int Capacity
    {
        get => _capacity;
        set => _capacity = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Gets or sets the number of synchronous backups.
    /// </summary>
    public int BackupCount
    {
        get => _backupCount;
        set => _backupCount = Preconditions.ValidateNewBackupCount(value, _asyncBackupCount);
    }

    /// <summary>
    /// Gets or sets the number of asynchronous backups.
    /// </summary>
    public int AsyncBackupCount
    {
        get => _asyncBackupCount;
        set => _asyncBackupCount = Preconditions.ValidateNewAsyncBackupCount(_backupCount, value);
    }

    /// <summary>
    /// Gets the total number of backups.
    /// </summary>
    public int TotalBackupCount => _backupCount + _asyncBackupCount;

    /// <summary>
    /// Gets or sets the time-to-live in seconds.
    /// </summary>
    public int TimeToLiveSeconds
    {
        get => _timeToLiveSeconds;
        set => _timeToLiveSeconds = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Gets or sets the in-memory format.
    /// </summary>
    public InMemoryFormat InMemoryFormat
    {
        get => _inMemoryFormat;
        set
        {
            if (value == InMemoryFormat.Native)
                throw new ArgumentException("Native format is not supported.", nameof(value));
            _inMemoryFormat = value.ThrowIfUndefined();
        }
    }

    /*
     * Get the RingbufferStore (load and store ringbuffer items from/to a database)
     * configuration.
     *
     * @return the ringbuffer store configuration
     */
    public RingbufferStoreOptions RingbufferStore
    {
        get => _ringbufferStoreOptions;
        set => _ringbufferStoreOptions = value;
    }

    /// <summary>
    /// Gets or sets the split-brain protection name.
    /// </summary>
    public string SplitBrainProtectionName
    {
        get => _splitBrainProtectionName;
        set => _splitBrainProtectionName = value;
    }

    /// <summary>
    /// Gets or sets the merge policy options.
    /// </summary>
    public MergePolicyOptions MergePolicy
    {
        get => _mergePolicyOptions;
        set => _mergePolicyOptions = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "RingbufferConfig{"
                + "name='" + _name + '\''
                + ", capacity=" + _capacity
                + ", backupCount=" + _backupCount
                + ", asyncBackupCount=" + _asyncBackupCount
                + ", timeToLiveSeconds=" + _timeToLiveSeconds
                + ", inMemoryFormat=" + _inMemoryFormat
                + ", ringbufferStoreConfig=" + _ringbufferStoreOptions
                + ", splitBrainProtectionName=" + _splitBrainProtectionName
                + ", mergePolicyConfig=" + _mergePolicyOptions
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.RingbufferConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_name);
        output.WriteInt(_capacity);
        output.WriteInt(_backupCount);
        output.WriteInt(_asyncBackupCount);
        output.WriteInt(_timeToLiveSeconds);
        output.WriteString(_inMemoryFormat.ToJavaString());
        output.WriteObject(_ringbufferStoreOptions);
        output.WriteString(_splitBrainProtectionName);
        output.WriteObject(_mergePolicyOptions);
    }
    
    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _name = input.ReadString();
        _capacity = input.ReadInt();
        _backupCount = input.ReadInt();
        _asyncBackupCount = input.ReadInt();
        _timeToLiveSeconds = input.ReadInt();
        _inMemoryFormat = Enums.ParseJava<InMemoryFormat>(input.ReadString());
        _ringbufferStoreOptions = input.ReadObject<RingbufferStoreOptions>();
        _splitBrainProtectionName = input.ReadString();
        _mergePolicyOptions = input.ReadObject<MergePolicyOptions>();
    }
}