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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration for a WAN target replication reference.
/// </summary>
[SuppressMessage("Design", "CA1002:Do not expose generic lists")] // cannot change public API
public class WanReplicationRef : IIdentifiedDataSerializable
{
    private const string DEFAULT_MERGE_POLICY_CLASS_NAME = "PassThroughMergePolicy";

    private bool _republishingEnabled = true;
    private string _name;
    private string _mergePolicyClassName = DEFAULT_MERGE_POLICY_CLASS_NAME;
    private List<string> _filters = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WanReplicationRef"/> class.
    /// </summary>
    public WanReplicationRef()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WanReplicationRef"/> class.
    /// </summary>
    public WanReplicationRef([NotNull] WanReplicationRef other)
        : this(other._name, other._mergePolicyClassName, other._filters, other._republishingEnabled)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WanReplicationRef"/> class.
    /// </summary>
    public WanReplicationRef(string name, string mergePolicyClassName, List<string> filters, bool republishingEnabled)
    {
        _name = name;
        _mergePolicyClassName = mergePolicyClassName;
        _filters = filters;
        _republishingEnabled = republishingEnabled;
    }

    /// <summary>
    /// Gets or sets the WAN replication reference name.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the merge policy class name.
    /// </summary>
    public string MergePolicyClassName
    {
        get => _mergePolicyClassName;
        set => _mergePolicyClassName = value.ThrowIfNull();
    }

    /// <summary>
    /// Adds the name of a class implementing the  CacheWanEventFilter or
    /// MapWanEventFilter for filtering outbound WAN replication events.
    /// </summary>
    /// <param name="filterClassName">The class name.</param>
    /// <returns>This instance.</returns>
    public WanReplicationRef AddFilter(string filterClassName)
    {
        _filters.Add(filterClassName.ThrowIfNull());
        return this;
    }

    /// <summary>
    /// Gets or sets the list of names of classes implementing the  CacheWanEventFilter or
    /// MapWanEventFilter for filtering outbound WAN replication events.
    /// </summary>
    public List<string> Filters
    {
        get => _filters;
        set => _filters = value.ThrowIfNull();
    }

    /// <summary>
    /// Whether incoming WAN events to this member should be republished
    /// (forwarded) to this WAN replication reference.
    /// </summary>
    public bool RepublishingEnabled
    {
        get => _republishingEnabled;
        set => _republishingEnabled = value;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.WanReplicationRef;

    /// <inheritdoc />
    public void WriteData([NotNull] IObjectDataOutput output)
    {
        output.WriteString(_name);
        output.WriteString(_mergePolicyClassName);
        output.WriteInt(_filters.Count);
        foreach (var filter in _filters) { output.WriteString(filter); }
        output.WriteBoolean(_republishingEnabled);
    }

    /// <inheritdoc />
    public void ReadData([NotNull] IObjectDataInput input)
    {
        _name = input.ReadString();
        _mergePolicyClassName = input.ReadString();
        var count = input.ReadInt();
        for (var i = 0; i < count; i++) { _filters.Add(input.ReadString()); }
        _republishingEnabled = input.ReadBoolean();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "WanReplicationRef{"
            + "name='" + _name + '\''
            + ", mergePolicy='" + _mergePolicyClassName + '\''
            + ", filters='" + string.Join(",", _filters) + '\''
            + ", republishingEnabled='" + _republishingEnabled
            + '\''
            + '}';
    }
}
