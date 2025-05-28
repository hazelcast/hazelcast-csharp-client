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
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the options for a duration.
/// </summary>
public class DurationOptions : IIdentifiedDataSerializable
{
    private long _durationAmount;
    private TimeUnit _timeUnit;

    public DurationOptions()
    { }

    public DurationOptions(long durationAmount, TimeUnit timeUnit)
    {
        _durationAmount = durationAmount;
        _timeUnit = timeUnit;
    }

    public long DurationAmount => _durationAmount;

    public TimeUnit TimeUnit => _timeUnit;

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.SimpleCacheConfigDurationConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteLong(_durationAmount);
        output.WriteString(_timeUnit.ToJavaString());
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _durationAmount = input.ReadLong();
        _timeUnit = Enums.ParseJava<TimeUnit>(input.ReadString());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "DurationConfig{"
               + "durationAmount=" + _durationAmount
               + ", timeUnit=" + _timeUnit
               + '}';
    }
}
