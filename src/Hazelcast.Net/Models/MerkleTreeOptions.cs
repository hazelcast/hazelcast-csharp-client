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

using Hazelcast.Serialization;
using Hazelcast.Core;
using Hazelcast.Configuration;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration of a merkle tree.
/// </summary>
public class MerkleTreeOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the minimal depth of a merkle tree.
        /// </summary>
        public const int MinDepth = 2;

        /// <summary>
        /// Gets the maximal depth of a merkle tree.
        /// </summary>
        public const int MaxDepth = 27;

        /// <summary>
        /// Gets the default depth of a merkle tree.
        /// </summary>
        public const int Depth = 10;
    }
#pragma warning restore CA1034

    private bool? _enabled;
    private int _depth = Defaults.Depth;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergePolicyOptions"/> class.
    /// </summary>
    public MerkleTreeOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MergePolicyOptions"/> class.
    /// </summary>
    public MerkleTreeOptions(MerkleTreeOptions config)
    {
        _enabled = config._enabled;
        _depth = config._depth;
    }

    /// <summary>
    /// Gets or sets the depth of the merkle tree.
    /// </summary>
    public int Depth
    {
        get => _depth;
        set => _depth = value.ThrowIfOutOfRange(Defaults.MinDepth, Defaults.MaxDepth);
    }

    /// <summary>
    /// Whether the merkle tree is enabled.
    /// </summary>
    /// <returns></returns>
    public bool Enabled
    {
        get => _enabled ?? false;
        set => _enabled = value;
    }

    /// <summary>
    /// Whether the <see cref="Enabled"/> property has been set.
    /// </summary>
    public bool EnabledSet => _enabled.HasValue;

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MerkleTreeConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteNullableBoolean(_enabled);
        output.WriteInt(_depth);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _enabled = input.ReadNullableBoolean();
        _depth = input.ReadInt();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "MerkleTreeConfig{"
               + "enabled=" + _enabled
               + ", depth=" + _depth
               + '}';
    }
}
