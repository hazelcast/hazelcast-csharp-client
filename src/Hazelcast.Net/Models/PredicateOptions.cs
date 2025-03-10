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

namespace Hazelcast.Models;

/// <summary>
/// Represents a predicate configuration.
/// </summary>
public class PredicateOptions : IIdentifiedDataSerializable
{
    private string _className;
    private string _sql;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateOptions"/> class.
    /// </summary>
    public PredicateOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateOptions"/> class.
    /// </summary>
    public PredicateOptions(string className)
    {
        ClassName = className;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateOptions"/> class.
    /// </summary>
    public PredicateOptions(PredicateOptions config)
    {
        _className = config._className;
        _sql = config._sql;
    }

    /// <summary>
    /// Gets the sets the name of the class of the predicate.
    /// </summary>
    public string ClassName
    {
        get => _className;
        set
        {
            _className = value.ThrowIfNullNorWhiteSpace();
            _sql = null;
        }
    }

    /// <summary>
    /// Gets or sets the sql string.
    /// </summary>
    public string Sql
    {
        get => _sql;
        set
        {
            _sql = value;
            _className = null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "PredicateConfig{"
                + "className='" + _className + '\''
                + ", sql='" + _sql + '\''
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.PredicateConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_className);
        output.WriteString(_sql);
        output.WriteObject(null/*implementation*/);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _className = input.ReadString();
        _sql = input.ReadString();
        _ = input.ReadObject<object>();
    }
}