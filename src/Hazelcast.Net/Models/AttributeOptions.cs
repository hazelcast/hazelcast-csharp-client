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
using System;
using System.Text.RegularExpressions;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Configuration;

namespace Hazelcast.Models;

public class AttributeOptions : IIdentifiedDataSerializable
{
    private static readonly Regex NamePattern = new Regex("^[a-zA-Z0-9][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    private string _name;
    private string _extractorClassName;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeOptions"/> class.
    /// </summary>
    public AttributeOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeOptions"/> class.
    /// </summary>
    public AttributeOptions(string name, string extractorClassName)
    {
        Name = name;
        ExtractorClassName = extractorClassName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeOptions"/> class.
    /// </summary>
    public AttributeOptions(AttributeOptions config)
    {
        _name = config._name;
        _extractorClassName = config._extractorClassName;
    }

    /// <summary>
    /// Gets or sets the name of the attribute extracted by the extractor.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));
            if (!NamePattern.IsMatch(value)) throw new ArgumentException(
                "Map attribute name is invalid. It may contain upper-case and lower-case" +
                " letters, digits and underscores but an underscore may not be located at the first position).", nameof(value));
            if (QueryConstants.IsConstant(value))
                throw new ArgumentException($"Map attribute name must not contain query constant '{value}'.");
            _name = value;
        }
    }

    /// <summary>
    /// Sets the name of the attribute extracted by the extractor.
    /// </summary>
    /// <param name="name">The name of the attribute extracted by the extractor.</param>
    /// <returns>This instance.</returns>
    public AttributeOptions SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// Gets or sets the extractor class name.
    /// </summary>
    public string ExtractorClassName
    {
        get => _extractorClassName;
        set => _extractorClassName = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Sets the extractor class name.
    /// </summary>
    /// <param name="extractorClassName">The extractor class name.</param>
    /// <returns>This instance.</returns>
    public AttributeOptions SetExtractorClassName(string extractorClassName)
    {
        ExtractorClassName = extractorClassName;
        return this;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "AttributeConfig{"
                + "name='" + _name + '\''
                + "extractorClassName='" + _extractorClassName + '\''
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MapAttributeConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_name);
        output.WriteString(_extractorClassName);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _name = input.ReadString();
        _extractorClassName = input.ReadString();
    }
}