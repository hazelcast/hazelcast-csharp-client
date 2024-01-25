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

using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Configures indexing options for <see cref="IndexType.Bitmap"/> indexes.
/// </summary>
public class BitmapIndexOptions : IIdentifiedDataSerializable
{
    private string _uniqueKey = Query.Predicates.KeyName;
    private UniqueKeyTransformation _transformation = UniqueKeyTransformation.Object;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitmapIndexOptions"/> class.
    /// </summary>
    public BitmapIndexOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitmapIndexOptions"/> class.
    /// </summary>
    public BitmapIndexOptions(BitmapIndexOptions bitmapIndexOptions)
    {
        UniqueKey = bitmapIndexOptions.UniqueKey;
        UniqueKeyTransformation = bitmapIndexOptions.UniqueKeyTransformation;
    }

    /// <summary>
    /// Gets or sets the unique key.
    /// </summary>
    public string UniqueKey
    {
        get => _uniqueKey;
        set => _uniqueKey = value.ThrowIfNull();
    }

    /// <summary>
    /// Sets the unique key.
    /// </summary>
    /// <param name="uniqueKey">The unique key.</param>
    /// <returns>This instance.</returns>
    public BitmapIndexOptions SetUniqueKey(string uniqueKey)
    {
        UniqueKey = uniqueKey;
        return this;
    }

    /// <summary>
    /// Gets or sets the <see cref="UniqueKeyTransformation"/> which will be
    /// applied to the <see cref="UniqueKey"/> value.
    /// </summary>
    public UniqueKeyTransformation UniqueKeyTransformation
    {
        get => _transformation;
        set => _transformation = value.ThrowIfUndefined();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.BitmapIndexOptions;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_uniqueKey);
        output.WriteInt((int) _transformation);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _uniqueKey = input.ReadString();
        _transformation = ((UniqueKeyTransformation) input.ReadInt()).ThrowIfUndefined();
    }
}