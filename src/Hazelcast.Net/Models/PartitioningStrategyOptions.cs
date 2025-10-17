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
using System.Diagnostics.CodeAnalysis;
using Hazelcast.Configuration;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration of the partitioning strategy.
/// </summary>
public class PartitioningStrategyOptions : IIdentifiedDataSerializable
{
    // note: Java also supports a PartitioningStrategy implementation class which .NET cannot support

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningStrategyOptions"/> class.
    /// </summary>
    public PartitioningStrategyOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningStrategyOptions"/> class.
    /// </summary>
    public PartitioningStrategyOptions([NotNull] PartitioningStrategyOptions config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        PartitioningStrategyClass = config.PartitioningStrategyClass;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitioningStrategyOptions"/> class.
    /// </summary>
    public PartitioningStrategyOptions(string partitioningStrategyClass)
    {
        PartitioningStrategyClass = partitioningStrategyClass;
    }

    /// <summary>
    /// Gets or sets the name of the class implementing the partitioning strategy.
    /// </summary>
    /// <returns></returns>
    public string PartitioningStrategyClass { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return "PartitioningStrategyConfig{"
                + "partitioningStrategyClass='" + PartitioningStrategyClass + '\''
                + ", partitioningStrategy=<not-supported>"
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.PartitionStrategyConfig;

    /// <inheritdoc />
    public void WriteData([NotNull] IObjectDataOutput output)
    {
        output.WriteString(PartitioningStrategyClass);
        output.WriteObject(null/*partitioningStrategy*/);
    }

    /// <inheritdoc />
    public void ReadData([NotNull] IObjectDataInput input)
    {
        PartitioningStrategyClass = input.ReadString();
        _/*partitioningStrategy*/ = input.ReadObject<object>();
    }
}
