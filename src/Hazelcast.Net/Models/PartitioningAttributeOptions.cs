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
/// Represents options of a partitioning attribute.
/// </summary>
public class PartitioningAttributeOptions : IIdentifiedDataSerializable
{
    private string _attributeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningAttributeOptions"/> class.
    /// </summary>
    public PartitioningAttributeOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningAttributeOptions"/> class.
    /// </summary>
    /// <param name="config"></param>
    public PartitioningAttributeOptions(PartitioningAttributeOptions config)
    {
        _attributeName = config._attributeName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningAttributeOptions"/> class.
    /// </summary>
    /// <param name="attributeName"></param>
    public PartitioningAttributeOptions(string attributeName)
    {
        _attributeName = attributeName;
    }

    /// <summary>
    /// Gets or sets the name of the attribute.
    /// </summary>
    public string AttributeName
    {
        get => _attributeName;
        set => _attributeName = value.ThrowIfNullNorWhiteSpace();
    }

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_attributeName);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _attributeName = input.ReadString();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.PartitionAttributeConfig;

    /// <inheritdoc />
    public override string ToString()
    {
        return "PartitioningAttributeConfig{"
            + "attributeName='" + _attributeName + '\''
            + '}';
    }
}
