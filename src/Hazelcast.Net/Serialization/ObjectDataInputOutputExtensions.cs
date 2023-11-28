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

using System;
using System.Collections.Generic;

namespace Hazelcast.Serialization;

internal static class ObjectDataInputOutputExtensions
{
    public static void WriteCollection<T>(this IObjectDataOutput output, ICollection<T> items)
    {
        output.WriteInt(items.Count);
        foreach (var item in items) output.WriteObject(item);
    }

    public static void WriteList<T>(this IObjectDataOutput output, ICollection<T> items)
        => output.WriteCollection(items);

    public static void WriteNullableCollection<T>(this IObjectDataOutput output, ICollection<T> items)
    {
        output.WriteBoolean(items != null);
        if (items != null) output.WriteCollection(items);
    }

    public static void WriteNullableList<T>(this IObjectDataOutput output, ICollection<T> items)
        => output.WriteNullableCollection(items);

    public static ICollection<T> ReadCollection<T>(this IObjectDataInput input)
        => input.ReadList<T>();

    public static List<T> ReadList<T>(this IObjectDataInput input)
    {
        var count = input.ReadInt();
        var list = new List<T>(count);
        for (var i = 0; i < count; i++) list.Add(input.ReadObject<T>());
        return list;
    }

    public static ICollection<T> ReadNullableCollection<T>(this IObjectDataInput input)
        => input.ReadNullableList<T>();

    public static List<T> ReadNullableList<T>(this IObjectDataInput input)
        => input.ReadBoolean() ? input.ReadList<T>() : null;

    public static void WriteNullableBoolean(this IObjectDataOutput output, bool? value)
    {
        if (value.HasValue) output.WriteByte(value.Value ? (byte) 1 : (byte) 0);
        else output.WriteByte(byte.MaxValue);
    }

    public static bool? ReadNullableBoolean(this IObjectDataInput input)
    {
        return input.ReadByte() switch
        {
            0 => false,
            1 => true,
            byte.MaxValue => null,
            _ => throw new NotSupportedException()
        };
    }
}