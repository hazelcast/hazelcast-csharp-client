﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

public class MergePolicyOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default merge policy.
        /// </summary>
        public const string MergePolicy = "com.hazelcast.spi.merge.PutIfAbsentMergePolicy";

        /// <summary>
        /// Gets the default batch size.
        /// </summary>
        public const int BatchSize = 100;
    }
#pragma warning restore CA1034

    private string _policy = Defaults.MergePolicy;
    private int _batchSize = Defaults.BatchSize;

    public MergePolicyOptions()
    { }

    public MergePolicyOptions(string policy, int batchSize)
    {
        Policy = policy;
        BatchSize = batchSize;
    }

    public MergePolicyOptions(MergePolicyOptions mergePolicyConfig)
    {
        _policy = mergePolicyConfig._policy;
        _batchSize = mergePolicyConfig._batchSize;
    }

    /// <summary>
    /// Gets or sets the classname of the split brain merge policy.
    /// </summary>
    public string Policy
    {
        get => _policy;
        set => _policy = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Gets or sets the batch size.
    /// </summary>
    public int BatchSize
    {
        get => _batchSize;
        set => _batchSize = value.ThrowIfLessThanZero();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MergePolicyConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_policy);
        output.WriteInt(_batchSize);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _policy = input.ReadString();
        _batchSize = input.ReadInt();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "MergePolicyConfig{"
               + "policy='" + _policy + '\''
               + ", batchSize=" + _batchSize
               + '}';
    }
}
