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
using System;

namespace Hazelcast.Models;

/// <summary>
/// 
/// </summary>
public class TimedExpiryPolicyFactoryOptions : IIdentifiedDataSerializable
{

    private ExpiryPolicyType _expiryPolicyType;
    private DurationOptions _durationConfig;

    public TimedExpiryPolicyFactoryOptions()
    { }

    public TimedExpiryPolicyFactoryOptions(ExpiryPolicyType expiryPolicyType, DurationOptions durationConfig)
    {
        _expiryPolicyType = expiryPolicyType;
        _durationConfig = durationConfig;
    }

    public ExpiryPolicyType ExpiryPolicyType => _expiryPolicyType;

    public DurationOptions DurationConfig => _durationConfig;

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.SimpleCacheConfigTimedExpiryPolicyFactoryConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_expiryPolicyType.ToJavaString());
        output.WriteObject(_durationConfig);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _expiryPolicyType = Enums.ParseJava<ExpiryPolicyType>(input.ReadString());
        _durationConfig = input.ReadObject<DurationOptions>();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "TimedExpiryPolicyFactoryConfig{"
            + "expiryPolicyType=" + _expiryPolicyType
            + ", durationConfig=" + _durationConfig
            + '}';
    }
}