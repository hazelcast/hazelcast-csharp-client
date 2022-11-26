// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;

#nullable enable

// TODO: consider emitting the property setters (dynamic IL, faster) + caching more things

namespace Hazelcast.Serialization.Compact;

/// <summary>
/// Implements a reflection-based compact serializer.
/// </summary>
internal partial class ReflectionSerializer : ICompactSerializer<object>
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

    /// <inheritdoc />
    public string TypeName => throw new NotSupportedException();

    private static TArray?[]? ToArray<TArray>(object? o, Func<object?, TArray?> convert) where TArray : struct
    {
        if (o is not Array { Rank: 1 } source) return null;
        var array = new TArray?[source.Length];
        var i = 0;
        foreach (var element in source) array[i++] = convert(element);
        return array;
    }

    private static TArray?[]? ToArray<TArray>(object? o, Func<object?, TArray> convert) where TArray : struct
    {
        if (o is not Array { Rank: 1 } source) return null;
        var array = new TArray?[source.Length];
        var i = 0;
        foreach (var element in source) array[i++] = convert(element);
        return array;
    }

    private static TArray[]? ToArray<TSource, TArray>(TSource[]? source, Func<TSource, TArray> convert) where TSource : struct where TArray : struct
    {
        if (source == null) return null;
        var array = new TArray[source.Length];
        for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
        return array;
    }

    private static TArray[]? ToArray<TSource, TArray>(TSource?[]? source, Func<TSource?, TArray> convert) where TSource : struct where TArray : struct
    {
        if (source == null) return null;
        var array = new TArray[source.Length];
        for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
        return array;
    }

    private static TArray?[]? ToArrayOfNullable<TSource, TArray>(TSource?[]? source, Func<TSource?, TArray?> convert) where TSource : struct where TArray : struct
    {
        if (source == null) return null;
        var array = new TArray?[source.Length];
        for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
        return array;
    }

    // for writing, cannot cast an object to e.g. an HBigDecimal because the explicit conversion cannot
    // be resolved at compile time. and, this is clearer (at IL level) than (HBigDecimal?)(decimal?)o

    private static short CharToShort(object? o) => (short)(ushort)ConvertEx.UnboxNonNull<char>(o);
    private static short? NullableCharToNullableShort(object? o) => o is char value ? (short)(ushort)value : null;

    private static short[]? CharsToShorts(object? o)
    {
        if (o == null) return null;
        if (o is not char[] x) throw new InvalidCastException();
        var shorts = new short[x.Length];
        for (var i = 0; i < x.Length; i++) shorts[i] = (short)(ushort)x[i];
        return shorts;
    }

    private static short?[]? NullableCharsToNullableShorts(object? o)
    {
        if (o == null) return null;
        if (o is not char?[] x) throw new InvalidCastException();
        var shorts = new short?[x.Length];
        for (var i = 0; i < x.Length; i++)
        {
            var c = x[i];
            shorts[i] = c is null ? null : (short)(ushort)c;
        }
        return shorts;
    }

    private static HBigDecimal? DecimalToBigDecimal(object? o) => o is decimal value ? new HBigDecimal(value) : null;
    private static HLocalTime? TimeSpanToTime(object? o) => o is TimeSpan value ? new HLocalTime(value) : null;
    private static HLocalDateTime? DateTimeToTimeStamp(object? o) => o is DateTime value ? new HLocalDateTime(value) : null;
    private static HOffsetDateTime? DateTimeOffsetToTimeStampWithTimeZone(object? o) => o is DateTimeOffset value ? new HOffsetDateTime(value) : null;
#if NET6_0_OR_GREATER
        private static HLocalTime? TimeOnlyToTime(object? o) => o is TimeOnly value ? new HLocalTime(value) : null;
        private static HLocalDate? DateOnlyToDate(object? o) => o is DateOnly value ? new HLocalDate(value) : null;
#endif

    #region Writer

    private static void WriteAsArray(ICompactWriter writer, string name, Type elementType, object? value)
    {
        var arrayType = elementType.MakeArrayType();

        // the writer methods expect an array - so we are converting the value, which we know is
        // at least an IEnumerable<>, to an array, using the Linq ToArray() extension method - from
        // a memory & performance standpoint this is not good, but Java does the same
        //
        // in order to be more efficient we would need to change ICompactWriter methods to write
        // values as ICollection<T> instead of T[] and that would be a breaking change - or we could
        // have them on CompactWriter only and not on the interface - but beware! here, writer may
        // not be a CompactWriter (when producing the schema) - then again in this case, we don't
        // care about the value and can pass an empty array
        //
        // but then... internally CompactWriter uses IObjectDataOutput.WriteXxxArray() methods
        // which *also* want a true array, so we would have to *also implement methods writing
        // ICollection<T> on ObjectDataOutput (not the interface) and use these methods. later.

        var toArray = typeof(Enumerable)
            .GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)!
            .MakeGenericMethod(elementType);

        var array = value == null ? null : toArray.Invoke(null, new[] { value });
        GetWriter(arrayType)(writer, name, array);
    }

    private static bool TryWriteCustomEnum(ICompactWriter writer, string name, Type type, object? value)
    {
        var isEnum = type.IsEnum;
        var isNullableEnum = type.IsNullableOfT(out var t0) && t0.IsEnum;

        if (isEnum || isNullableEnum)
        {
            var stringValue = value?.ToString(); // write enum values as strings
            GetWriter(typeof(string))(writer, name, stringValue);
            return true;
        }

        var isArray = type.IsArray && type.GetArrayRank() == 1;
        var isArrayOfEnum = isArray && type.GetElementType()!.IsEnum;
        var isArrayOfNullableEnum = isArray && type.GetElementType().IsNullableOfT(out var t1) && t1.IsEnum;

        if (isArrayOfEnum || isArrayOfNullableEnum)
        {
            string?[]? stringValues = null;
            if (value is Array a)
            {
                stringValues = new string?[a.Length];
                for (var i = 0; i < a.Length; i++) stringValues[i] = a.GetValue(i)?.ToString();
            }
            GetWriter(typeof(string[]))(writer, name, stringValues);
            return true;
        }

        return false;
    }

    private static bool TryWriteCustomArray(ICompactWriter writer, string name, Type type, object? value)
    {
        if (!type.IsArray || type.GetArrayRank() != 1) 
            return false;

        // is an array, and not a known builtin array, else we wouldn't be here

        var elementType = type.GetElementType()!; // cannot be null, type is an array

        // if the element type is one of the supported custom generics (list, set, map)... well
        // we don't support nesting them and it is not even sure it is possible at all -  a
        // list of lists could end up being an array of arrays, that would work, but a dictionary
        // ends up being two fields in the schema and how a list of dictionaries would then work?
        //
        // but if we throw here, the user gets a 'cannot serialize List<int>' error instead of
        // the full 'cannot serialize List<List<int>>' error and that is confusing - so do *not*
        // test here but test in TryWriteCustomGeneric

        // is an array, and not a known builtin array, so treat is as an array of compact objects
        var writeObject = writer.GetType().GetMethod(nameof(ICompactWriter.WriteArrayOfCompact));
        var writeObjectOfType = writeObject!.MakeGenericMethod(elementType);
        writeObjectOfType.Invoke(writer, new[] { name, value });
        return true;
    }

    private static void EnsureNoNestedCustomGeneric(Type type, Type elementType)
    {
        if (IsList(elementType) || IsSet(elementType) || IsDictionary(elementType))
            throw new SerializationException($"Nested generic type {type} is not supported.");
    }

    private static bool TryWriteCustomGeneric(ICompactWriter writer, string name, Type type, object? value)
    {
        // is a generic type, could be a list, a set, or a dictionary
        if (IsList(type) || IsSet(type))
        {
            // is explicitly a list or a set, treat is as an array of elements
            var elementType = type.GetGenericArguments()[0];
            EnsureNoNestedCustomGeneric(type, elementType);
            WriteAsArray(writer, name, elementType, value);
            return true;
        }

        if (IsDictionary(type))
        {
            // is explicitly a dictionary, treat it as two arrays of elements
            var keyType = type.GetGenericArguments()[0];
            EnsureNoNestedCustomGeneric(type, keyType);
            var valueType = type.GetGenericArguments()[1];
            EnsureNoNestedCustomGeneric(type, valueType);

            if (value == null)
            {
                WriteAsArray(writer, name + "!keys", keyType, null);
                WriteAsArray(writer, name + "!values", valueType, null);
            }
            else
            {
                var keysProperty = value.GetType().GetProperty("Keys");
                if (keysProperty == null) throw new HazelcastException($"Internal error: cannot get {type}.Keys property.");
                var valuesProperty = value.GetType().GetProperty("Values");
                if (valuesProperty == null) throw new HazelcastException($"Internal error: cannot get {type}.Values property.");

                WriteAsArray(writer, name + "!keys", keyType, keysProperty.GetValue(value));
                WriteAsArray(writer, name + "!values", valueType, valuesProperty.GetValue(value));
            }

            return true;
        }

        return false;
    }

    private static void WriteCustom(ICompactWriter writer, string name, Type type, object? value)
    {
        if (TryWriteCustomEnum(writer, name, type, value)) return;
        if (TryWriteCustomArray(writer, name, type, value)) return;
        if (TryWriteCustomGeneric(writer, name, type, value)) return;

        // is anything else, treat is as compact object
        var writeObject = writer.GetType().GetMethod(nameof(ICompactWriter.WriteCompact));
        var writeObjectOfType = writeObject!.MakeGenericMethod(type);
        writeObjectOfType.Invoke(writer, new[] { name, value });
    }

    private static Action<ICompactWriter, string, object?> GetWriter(Type type)
    {
        // either use one of the builtin writers, or our custom writer
        return Writers.TryGetValue(type, out var write)
            ? write
            : (writer, name, value) => WriteCustom(writer, name, type, value);
    }

    #endregion

    #region Reader

    private static Array? ReadAsArray(ICompactReader reader, string name, Type elementType)
    {
        var arrayType = elementType.MakeArrayType();
        return (Array?) GetReader(arrayType)(reader, name);
    }

    private static bool TryReadCustomEnum(ICompactReader reader, string name, Type type, out object? value)
    {
        var isEnum = type.IsEnum;
        var isNullableEnum = type.IsNullableOfT(out var t0) && t0.IsEnum;

        if (isEnum || isNullableEnum)
        {
            var enumType = isEnum ? type : t0;
            var stringValue = GetReader(typeof(string))(reader, name); // read enum values as strings
            if (stringValue is string s)
            {
                value = Enum.Parse(enumType, s);
                return true;
            }
            if (!isNullableEnum) throw new SerializationException("Read null value for non-nullable enum.");
            value = null;
            return true;
        }

        Type? t1 = null;
        var isArray = type.IsArray && type.GetArrayRank() == 1;
        var isArrayOfEnum = isArray && type.GetElementType()!.IsEnum;
        var isArrayOfNullableEnum = isArray && type.GetElementType().IsNullableOfT(out t1) && t1.IsEnum;

        if (isArrayOfEnum || isArrayOfNullableEnum)
        {
            var enumType = isArrayOfEnum ? type.GetElementType()! : t1!;

            var stringValues = GetReader(typeof(string[]))(reader, name); // read enum values as strings
            if (stringValues is not Array a)
            {
                value = null; // if it's a null array, return null
                return true;
            }

            var elementType = isArrayOfNullableEnum ? typeof(Nullable<>).MakeGenericType(enumType) : enumType;
            var array = Array.CreateInstance(elementType, a.Length);

            // if it's an array of nullable, we need to construct the Nullable<> instances
            var elementCtor = isArrayOfNullableEnum ? elementType.GetConstructor(new[] { enumType }) : null;

            for (var i = 0; i < a.Length; i++)
            {
                if (a.GetValue(i) is not string s) // null value (if ok) = just don't initialize
                {
                    if (!isArrayOfNullableEnum) throw new SerializationException("Read null value for non-nullable enum.");
                    continue;
                }

                var parsed = Enum.Parse(enumType, s);
                array.SetValue(isArrayOfEnum ? parsed : elementCtor!.Invoke(new[] { parsed }), i);
            }

            value = array;
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryReadCustomArray(ICompactReader reader, string name, Type type, out object? value)
    {
        value = null;
        if (!type.IsArray || type.GetArrayRank() != 1) 
            return false;

        var elementType = type.GetElementType()!; // cannot be null, type is an array
        if (elementType.IsInterface)
            throw new SerializationException($"Interface type {elementType} is not supported by reflection serialization.");
        var readObject = reader.GetType().GetMethod(nameof(ICompactReader.ReadArrayOfCompact));
        var readObjectOfType = readObject!.MakeGenericMethod(elementType);
        value = readObjectOfType.Invoke(reader, new object[] { name });
        return true;
    }

    private static bool TryReadCustomGeneric(ICompactReader reader, string name, Type type, out object? value)
    {
        var objGenericType = 
            IsList(type) ? typeof (List<>) :
            IsSet(type) ? typeof (HashSet<>) :
            null;

        if (objGenericType != null)
        {
            // is explicitly a list or a set, treat is as an array of elements
            var elementType = type.GetGenericArguments()[0];
            var array = ReadAsArray(reader, name, elementType);
            if (array == null)
            {
                value = null;
            }
            else
            {
                var objType = objGenericType.MakeGenericType(elementType);
                var obj = Activator.CreateInstance(objType);
                var add = objType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                if (add == null) throw new HazelcastException($"Internal exception: cannot get {objType}.Add method.");
                for (var i = 0; i < array.Length; i++)
                    add.Invoke(obj, new[] { array.GetValue(i) });
                value = obj;
            }

            return true;
        }

        if (IsDictionary(type))
        {
            // is explicitly a dictionary, treat it as two arrays of elements
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var keysArray = ReadAsArray(reader, name + "!keys", keyType);
            var valuesArray = ReadAsArray(reader, name + "!values", valueType);
            if (keysArray == null && valuesArray == null)
            {
                value = null;
                return true;
            }

            if (keysArray == null || valuesArray == null)
                throw new SerializationException("Cannot read corrupt dictionary.");
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictionary = Activator.CreateInstance(dictionaryType);
            var addPair = dictionaryType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
            if (addPair == null) throw new HazelcastException($"Internal exception: cannot get {dictionaryType}.Add method.");
            for (var i = 0; i < keysArray.Length; i++)
            {
                var k = keysArray.GetValue(i);
                var v = valuesArray.GetValue(i);
                addPair.Invoke(dictionary, new[] { k, v });
            }

            value = dictionary;
            return true;
        }

        value = null;
        return false;
    }

    private static object? ReadCustom(ICompactReader reader, string name, Type type)
    {
        if (TryReadCustomEnum(reader, name, type, out var value))
            return value;
        if (TryReadCustomArray(reader, name, type, out value))
            return value;
        if (TryReadCustomGeneric(reader, name, type, out value))
            return value;

        if (type.IsInterface)
            throw new SerializationException($"Interface type {type} is not supported by reflection serialization.");

        // read as compact object
        var readObject = reader.GetType().GetMethod(nameof(ICompactReader.ReadCompact));
        var readObjectOfType = readObject!.MakeGenericMethod(type);
        return readObjectOfType.Invoke(reader, new object[] { name });
    }

    private static Func<ICompactReader, string, object?> GetReader(Type type)
    {
        return Readers.TryGetValue(type, out var read)
            ? read
            : (reader, name) => ReadCustom(reader, name, type);
    }

    #endregion

    // gets (and cache) the list of properties defined by a type, that are public, instance-level
    // (not static), and are both readable and writable via reflection.
    private static PropertyInfo[] GetProperties(Type objectType)
        => Properties.GetOrAdd(objectType,
            type => type
                // "[GetProperties] returns all public instance and static properties, both those defined
                // by the type represented by the current Type object as well as those inherited from its
                // base types."
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.CanWrite)
                .ToArray()
        );

    #region Read

    /// <inheritdoc />
    public virtual object Read(ICompactReader ireader)
    {
        // we can only read from our internal CompactReader
        var reader = ireader.MustBe<CompactReader>(nameof(ireader));

        var obj = ConstructObject(reader.ObjectType, reader, out var ctorFields);

        foreach (var property in GetProperties(reader.ObjectType))
        {
            // exclude properties that correspond to fields that have
            // already been initialized by the constructor
            if (ctorFields != null && ctorFields.Contains(property.Name))
                continue;

            // exclude properties that no not correspond to a field (case-
            // insensitive) and for those that are not excluded, get the
            // properly-cased field name
            if (!GetValidFieldName(reader, property.PropertyType, property.Name, out var fieldName))
                continue;

            // read the field value and set the property value accordingly
            var fieldType = property.PropertyType;
            property.SetValue(obj, GetReader(fieldType)(reader, fieldName));
        }

        return obj;
    }

    // constructs an object of a specified type, using the empty constructor if possible,
    // otherwise using the constructor with most (and all) parameters matching field names
    // (case-sensitive) - and then ctorFields contains the fields that have been consumed
    private static object ConstructObject(Type type, CompactReader reader, out HashSet<string>? ctorFields)
    {
        // value types can always be created by the activator regardless of their constructors
        // and, they don't implicitly implement an empty constructor - so we *want* to use the activator
        if (type.IsValueType)
        {
            ctorFields = null;
            try
            {
                var obj = Activator.CreateInstance(type);
                if (obj == null)
                    throw new SerializationException($"Failed to create an instance of type {type} (Activator.CreateInstance returned null).");
                    obj = ctor.Invoke(p);
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to create an instance of type {type} (Activator.CreateInstance has thrown, see inner exception).", e);
        }
        var ctors = type.GetConstructors();

            // TODO: consider emitting the property setters
            foreach (var property in GetProperties(typeOfObj))
        if (emptyCtor != null)
            try
            {
                ctorFields = null;
                return emptyCtor.Invoke(Array.Empty<object>());
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to create an instance of type {type} (Invoking the empty constructor has thrown, see inner exception).", e);
            }
        }

        // look for the constructor with most (and all) parameters matching field names (case-sensitive)
        var ctor = ctors
            .Where(ctor => ctor.GetParameters().All(x => reader.ValidateFieldName(x.Name)))
            .OrderBy(x => x.GetParameters())
            .LastOrDefault();

        if (ctor == null)
            throw new SerializationException($"Failed to create an instance of type {type} (Could not find a constructor with parameters matching fields).");

        // read the values for the constructor parameters
        var parameters = ctor.GetParameters();
        var values = new object?[parameters.Length];
        ctorFields = new HashSet<string>();
        for (var i = 0; i < parameters.Length; i++)
        {
            var fieldName = parameters[i].Name;
            var fieldType = parameters[i].ParameterType;
            ctorFields.Add(fieldName);
            values[i] = GetReader(fieldType)(reader, fieldName);
        }

        // and invoke the constructor with the parameters
        try
        {
            return ctor.Invoke(values);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to create an instance of type {type} (Invoking the constructor with '{string.Join(", ", parameters.Select(x => x.ParameterType))}' parameters has thrown, see inner exception).", e);
        }
    }

    #endregion

    #region Write

    /// <inheritdoc />
    public virtual void Write(ICompactWriter writer, object obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        VerifyTypeIsSupported(obj);

        foreach (var property in GetProperties(obj.GetType()))
        {
            if (!GetValidFieldName(writer, property.PropertyType, property.Name, out var fieldName)) 
                continue;

            GetWriter(property.PropertyType)(writer, fieldName, property.GetValue(obj));
        }
    }

    private static bool GetValidFieldName(ICompactWriter iwriter, Type propertyType, string propertyName, [NotNullWhen(true)] out string? fieldName)
    {
        // writer here can be a true CompactWriter, which holds a schema, and is able to provide
        // 'valid' field names (mostly mapping different casing in names e.g. 'Name' to 'name'),
        // but it also can be a SchemaBuilderWriter in which case we are writing the schema, and
        // have to accept the property names as field names.

        fieldName = propertyName;
        return iwriter is not CompactReaderWriterBase writer || GetValidFieldName(writer, propertyType, propertyName, out fieldName);
    }

    private static bool GetValidFieldName(CompactReaderWriterBase rw, Type propertyType, string propertyName, [NotNullWhen(true)] out string? fieldName)
    {
        // dictionaries are special because they map to two properties, named
        // name!keys and name!values, so validation has to be different

        if (!IsDictionary(propertyType))
            return rw.ValidateFieldNameInvariant(propertyName, out fieldName);

        // special case of dictionaries: the property "Foo" is serialized as *two*
        // schema fields, "Foo!keys" and "Foo!values" 

        fieldName = null;
        if (!rw.ValidateFieldNameInvariant(propertyName + "!keys", out var keysFieldName))
            return false;
        if (!rw.ValidateFieldNameInvariant(propertyName + "!values", out var valuesFieldName))
            return false;
        fieldName = keysFieldName.TrimEnd("!keys");
        return valuesFieldName.TrimEnd("!values") == fieldName;
    }

    #endregion

    private static void VerifyTypeIsSupported(object o)
    {
        // for FullName to be null, the type would need to be derived from an open generic somehow,
        // which makes no sense since it is the actual type of a concrete object. we can assume that
        // type is not going to be null.
        var type = o.GetType();
        var name = type.FullName!;

        if (name.StartsWith("<", StringComparison.Ordinal))
            throw new SerializationException($"The {type} type cannot be serialized via zero-configuration "
                                             + "Compact serialization because anonymous types are not supported.");

        if (NonSupportedNamespaces.Any(x => name.StartsWith(x, StringComparison.Ordinal)))
            throw new SerializationException($"The {name} type is not supported by zero-configuration Compact "
                                             + "serialization. Consider writing a custom ICompactSerializer for this type.");
    }

    // for now, we do *not* support all System.* namespaces -  we may want to add more later on
    private static readonly string[] NonSupportedNamespaces = { "System." };

    private static bool IsDictionary(Type type)
    {
        if (!type.IsGenericType) return false;
        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>);
    }

    private static bool IsList(Type type)
    {
        if (!type.IsGenericType) return false;
        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(List<>) || genericType == typeof(IList<>);
    }

    private static bool IsSet(Type type)
    {
        if (!type.IsGenericType) return false;
        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(HashSet<>) || genericType == typeof(ISet<>);
    }
}
