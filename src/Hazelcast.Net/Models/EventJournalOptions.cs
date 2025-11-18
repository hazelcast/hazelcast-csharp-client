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
using Hazelcast.Core;
using Hazelcast.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.Models;

/// <summary>
/// Represents options for a map event journal.
/// </summary>
public class EventJournalOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default IsEnabled value.
        /// </summary>
        public const bool Enabled = false;

        /// <summary>
        /// Gets the default capacity of the event journal.
        /// </summary>
        public const int Capacity = 10 * 1000;

        /// <summary>
        /// Gets the default time-to-live.
        /// </summary>
        public const int TtlSeconds = 0;
    }
#pragma warning restore CA1034

    private bool _enabled = Defaults.Enabled;
    private int _capacity = Defaults.Capacity;
    private int _timeToLiveSeconds = Defaults.TtlSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventJournalOptions"/> class.
    /// </summary>
    public EventJournalOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventJournalOptions"/> class.
    /// </summary>
    public EventJournalOptions([NotNull] EventJournalOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));

        _enabled = options._enabled;
        _capacity = options._capacity;
        _timeToLiveSeconds = options._timeToLiveSeconds;
    }

    /// <summary>
    /// Gets or sets the capacity of the event journal.
    /// </summary>
    public int Capacity
    {
        get => _capacity;
        set => _capacity = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Gets or sets the time-to-live in seconds.
    /// </summary>
    public int TimeToLiveSeconds
    {
        get => _timeToLiveSeconds;
        set => _timeToLiveSeconds = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Whether the event journal is enabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.EventJournalConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(_enabled);
        output.WriteInt(_capacity);
        output.WriteInt(_timeToLiveSeconds);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _enabled = input.ReadBoolean();
        _capacity = input.ReadInt();
        _timeToLiveSeconds = input.ReadInt();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "EventJournalConfig{"
               + "enabled=" + _enabled
               + ", capacity=" + _capacity
               + ", timeToLiveSeconds=" + _timeToLiveSeconds
               + '}';
    }
}
