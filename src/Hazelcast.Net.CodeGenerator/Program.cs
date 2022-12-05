// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Text;
using System.Text.RegularExpressions;
using Hazelcast.CodeGenerator;

var slnPath = Directory.GetCurrentDirectory();
while (Path.GetFileName(slnPath) != "src") slnPath = Path.GetDirectoryName(slnPath);
slnPath = Path.GetDirectoryName(slnPath);
Console.WriteLine($"SolutionPath: {slnPath}");

var fileEditor = new FileEditor(slnPath);

// read the FieldKind enumeration source code
var fieldKindText = File.ReadAllText(Path.Combine(slnPath, "src/Hazelcast.Net/Serialization/FieldKind.cs"));

// parse the FieldKind numeration source code into names: <enum int value> -> <enum field info>
var fieldKinds = new Dictionary<int, KindInfo>();
var regex = new Regex("^\\s*([A-Za-z0-9]+)\\s*=\\s*(-?[0-9]+),?\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
foreach (var m in regex.Matches(fieldKindText).OfType<Match>())
{
    var fullName = m.Groups[1].Value;
    var id = int.Parse(m.Groups[2].Value);

    if (KindInfo.TryParse(fullName, id, out var kindInfo)) fieldKinds[id] = kindInfo;
}

// report
foreach (var (id, fieldKind) in fieldKinds)
{
    Console.WriteLine($"{id:000} {fieldKind.FullName} {fieldKind.Name} {fieldKind.ClrType} {(fieldKind.IsArray ? '+' : '-')}array {(fieldKind.IsNullable ? '+' : '-')}nullable {(fieldKind.IsValueType ? '+' : '-')}valueType");
}

// ----------------------------------------------------------------

void GenerateHasFieldOfKind(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        text.Append($@"        /// <summary>
        /// Determines whether a <see cref=""FieldKind.{kindInfo.FullName}""/> field is available.
        /// </summary>
        /// <param name=""reader"">The reader.</param>
        /// <param name=""name"">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref=""FieldKind.{kindInfo.FullName}""/> field with the
        /// specified <paramref name=""name""/>; otherwise <c>false</c>.</returns>
        public static bool Has{kindInfo.FullName}(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.{kindInfo.FullName});

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactReaderExtensions.HasFieldOfKind.cs", 
    GenerateHasFieldOfKind);

void GenerateReadOrDefault(StringBuilder text)
{
    text.Append(@"
        // NOTE
        // ReadInt8 can read a Int8 or NullableInt8 field, provided that the value is not null.
        // For consistency purposes, ReadInt8OrDefault *also* can read a Int8 or NullableInt8
        // field, and if the field is NullableInt8 and the value is null, it throws just as
        // ReadInt8 (does not return the default value).

");
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;

        var genericType = "";
        var genericDoc = "";
        if (type == "object")
        {
            type = "T";
            genericType = "<T>";
            genericDoc = $@"
        /// <typeparam name=""T"">The expected type of the object{(kindInfo.IsArray ? "s" : "")}.</typeparam>";
        }

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        var orHas = "";
        if (kindInfo.IsValueType && !kindInfo.IsNullableOnly)
        {
            if (kindInfo.IsNullable)
                orHas = $" || safeReader.Has{(kindInfo.IsArray ? "ArrayOf" : "")}{kindInfo.Name}(name)";
            else
                orHas = $" || safeReader.Has{(kindInfo.IsArray ? "ArrayOf" : "")}Nullable{kindInfo.Name}(name)";
        }

        text.Append($@"        /// <summary>
        /// Reads a <see cref=""FieldKind.{kindInfo.FullName}""/> field.
        /// </summary>{genericDoc}
        /// <param name=""reader"">The reader.</param>
        /// <param name=""name"">The name of the field.</param>
        /// <param name=""defaultValue"">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name=""defaultValue""/>.</returns>
        public static {type} Read{kindInfo.FullName}OrDefault{genericType}(this ICompactReader reader, string name, {type} defaultValue = default)
        {{
            var safeReader = reader.NotNull();
            return safeReader.Has{kindInfo.FullName}(name){orHas} ? safeReader.Read{kindInfo.FullName}{genericType}(name) : defaultValue;
        }}

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactReaderExtensions.ReadOrDefault.cs", 
    GenerateReadOrDefault);

void GenerateCompactReader(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;

        var genericType = "";
        var genericDoc = "";
        if (type == "object")
        {
            type = "T"; // reading
            genericType = "<T>";
            genericDoc = $@"
        /// <typeparam name=""T"">The expected type of the object{(kindInfo.IsArray ? "s" : "")}.</typeparam>";
        }

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        var orOther = "";
        var exceptionDoc = "";
        if (kindInfo.IsValueType && !kindInfo.IsNullableOnly)
        {
            if (kindInfo.IsNullable)
            {
                orOther = $" or <see cref=\"FieldKind.{(kindInfo.IsArray ? "ArrayOf" : "")}{kindInfo.Name}\"/>";
                exceptionDoc = "";
            }
            else
            {
                orOther = $" or <see cref=\"FieldKind.{(kindInfo.IsArray ? "ArrayOf" : "")}Nullable{kindInfo.Name}\"/>";
                var aOrThe = kindInfo.IsArray ? "a" : "the";
                exceptionDoc = @$"
        /// <remarks>
        /// <para>Throws a <see cref=""SerializationException"" /> if the field is nullable and {aOrThe} value is <c>null</c>.</para>
        /// </remarks>";
            }
        }

        text.Append($@"        /// <summary>Reads a <see cref=""FieldKind.{kindInfo.FullName}""/>{orOther} field.</summary>{genericDoc}
        /// <param name=""name"">The name of the field.</param>
        /// <returns>The value of the field.</returns>{exceptionDoc}
        {type} Read{kindInfo.FullName}{genericType}(string name);

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/ICompactReader.cs", 
    GenerateCompactReader);

void GenerateCompactWriter(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;

        var genericType = "";
        var genericDoc = "";

        if (type == "object")
        {
            type = "T";
            genericType = "<T>";
            genericDoc = $@"
        /// <typeparam name=""T"">The type of the object{(kindInfo.IsArray ? "s" : "")}.</typeparam>";
        }

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"        /// <summary>Writes a <see cref=""FieldKind.{kindInfo.FullName}""/> field.</summary>{genericDoc}
        /// <param name=""name"">The name of the field.</param>
        /// <param name=""value"">The value of the field.</param>
        void Write{kindInfo.FullName}{genericType}(string name, {type} value);

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/ICompactWriter.cs", 
    GenerateCompactWriter);

void GenerateReflectionWriters(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;
        if (type == "object") continue; // these have special methods
        var type0 = type;

        if (kindInfo.IsNullable || kindInfo.ClrType == "string")
        {
            type = $"{type}?";
            if (kindInfo.IsValueType) type0 = type;
        }

        if (kindInfo.IsArray)
        {
            type = $"{type}[]?";
            type0 = $"{type0}[]";
        }

        var castobj = kindInfo.IsArray || kindInfo.IsNullable || kindInfo.ClrType == "string" ? $"({type})o" : $"UnboxNonNull<{type}>(o)";

        //        if (isValueTypeNullableOnly)
        //            text.Append($@"                {{ typeof ({type00}), (w, n, o) => w.Write{name}(n, {castobj}) }},
        //");

        text.Append($@"                {{ typeof ({type0}), (w, n, o) => w.Write{kindInfo.FullName}(n, {castobj}) }},
");
    }
}

void GenerateReflectionReaders(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;
        if (type == "object") continue; // these have special methods
        var type0 = type;

        if (kindInfo.IsNullable || kindInfo.ClrType == "string")
        {
            type = $"{type}?";
            if (kindInfo.IsValueType) type0 = type;
        }

        if (kindInfo.IsArray)
        {
            type = $"{type}[]?";
            type0 = $"{type0}[]";
        }

        //        if (isValueTypeNullableOnly)
        //            text.Append($@"                {{ typeof ({type00}), (r, n) => ({type}) ValueNonNull(r.Read{name}(n)) }},
        //");

        text.Append($@"                {{ typeof ({type0}), (r, n) => ({type}) r.Read{kindInfo.FullName}(n) }},
");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/ReflectionSerializer.cs", 
    GenerateReflectionWriters, 
    GenerateReflectionReaders);

void GenerateSchemaBuilderWriter(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        var type = kindInfo.ClrType;

        var genericType = "";

        if (type == "object")
        {
            type = "T?";
            genericType = "<T>";
        }
        else if (kindInfo.IsNullable || kindInfo.IsNullableOnly || kindInfo.ClrType == "string") type = $"{type}?";

        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"         public void Write{kindInfo.FullName}{genericType}(string name, {type} value) => AddField(name, FieldKind.{kindInfo.FullName});
");
    }

}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/SchemaBuilderWriter.cs", 
    GenerateSchemaBuilderWriter);

void GenerateCompactReaderHasFieldOfKindExtensionsTests(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        text.Append($@"            (FieldKind.{kindInfo.FullName}, (x, y) => x.Has{kindInfo.FullName}(y)),
");
    }
}

void GenerateCompactReaderExtensionsTestsTestCases(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (!SampleData.TypeValues.TryGetValue(kindInfo.ClrType, out var values)) continue;

        var t = kindInfo.ClrType;
        if (kindInfo.IsNullable && kindInfo.IsValueType) t += "?";

        if (kindInfo.IsArray)
            text.Append($@"            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, new {t}[]{{{values[0]}}}, new {t}[]{{{values[1]}}}),
            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, new {t}[]{{{values[0]}}}, ({kindInfo.ClrType}[])null),
            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, null, new {t}[]{{{values[1]}}}),
            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, null, null),
");
        else
            text.Append($@"            (typeof ({t}), FieldKind.{kindInfo.FullName}, {values[0]}, {values[1]}),
");
        if (kindInfo.IsNullable)
        {
            if (kindInfo.IsArray)
                text.Append($@"            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, new {t}[]{{{values[0]}}}, new {t}[]{{null}}),
            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, new {t}[]{{null}}, new {t}[]{{{values[1]}}}),
            (typeof ({t}[]), FieldKind.{kindInfo.FullName}, new {t}[]{{null}}, new {t}[]{{null}}),
");
            else
                text.Append($@"            (typeof ({t}), FieldKind.{kindInfo.FullName}, {values[0]}, null),
            (typeof ({t}), FieldKind.{kindInfo.FullName}, null, {values[1]}),
            (typeof ({t}), FieldKind.{kindInfo.FullName}, null, null),
");
        }
    }
}

fileEditor.EditFile("src/Hazelcast.Net.Tests/Serialization/Compact/CompactReaderExtensionsTests.cs",
    GenerateCompactReaderHasFieldOfKindExtensionsTests,
    GenerateCompactReaderExtensionsTestsTestCases);

void GenerateGenericRecord(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        var type = kindInfo.ClrType;

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"    /// <summary>
    /// Gets the value of a <see cref=""FieldKind.{kindInfo.FullName}""/> field.
    /// </summary>
    /// <param name=""fieldname"">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref=""SerializationException"">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public {type} Get{kindInfo.FullName}(string fieldname);

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/IGenericRecord.GeneratedCode.cs",
    GenerateGenericRecord);

void GenerateCompactDictionaryGenericRecord(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        var type = kindInfo.ClrType;
        var type0 = type;
        if (kindInfo.IsNullable) type = $"{type}?";
        var type1 = type;
        if (kindInfo.IsArray) type = $"{type}[]?";

        // need to determine whether we support the null/non-null thingy
        var altKind = "";
        var ret = "";
        if (kindInfo.IsValueType && !kindInfo.IsNullableOnly)
        {
            if (kindInfo.IsNullable)
            {
                altKind = $", FieldKind.{(kindInfo.IsArray ? "ArrayOf" : "")}{kindInfo.Name}";
                ret = kindInfo.IsArray
                    ? $"GetArrayOfNullableOf<{type0}>(_fieldValues[fieldname])"
                    : $"({type}) _fieldValues[fieldname]";
            }
            else
            {
                altKind = $", FieldKind.{(kindInfo.IsArray ? "ArrayOf" : "")}Nullable{kindInfo.Name}";
                ret = kindInfo.IsArray
                    ? $"GetArrayOf<{type0}>(_fieldValues[fieldname])"
                    : $"_fieldValues[fieldname] is {type} value ? value : throw new SerializationException($\"Null value for field '{{fieldname}}'.\")";
            }
        }
        else
        {
            ret = $"({type}) _fieldValues[fieldname]";
        }

        text.Append($@"    /// <inheritdoc />
    public {type} Get{kindInfo.FullName}(string fieldname)
    {{
        ValidateField(fieldname, FieldKind.{kindInfo.FullName}{altKind});
        return {ret};
    }}

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactDictionaryGenericRecord.GeneratedCode.cs",
    GenerateCompactDictionaryGenericRecord);

void GenerateIGenericRecordBuilder(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        var type = kindInfo.ClrType;

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"    /// <summary>
    /// Adds a <see cref=""FieldKind.{kindInfo.FullName}""/> field to the record.
    /// </summary>
    /// <param name=""fieldname"">The name of the field.</param>
    /// <param name=""value"">The value of the field.</param>
    /// <returns>This <see cref=""IGenericRecordBuilder""/>.</returns>
    IGenericRecordBuilder Set{kindInfo.FullName}(string fieldname, {type} value);

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/IGenericRecordBuilder.GeneratedCode.cs",
    GenerateIGenericRecordBuilder);

void GenerateCompactGenericRecordBuilder(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        var type = kindInfo.ClrType;

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"    /// <inheritdoc />
    public IGenericRecordBuilder Set{kindInfo.FullName}(string fieldname, {type} value) => SetField(fieldname, value, FieldKind.{kindInfo.FullName});

");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactGenericRecordBuilder.GeneratedCode.cs",
    GenerateCompactGenericRecordBuilder);

void GenerateCompactReaderReadAny(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        text.Append($@"            case FieldKind.{kindInfo.FullName}: return reader.Read{kindInfo.FullName}(name);
");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactReaderExtensions.GeneratedCode.cs",
    GenerateCompactReaderReadAny);

void GenerateCompactWriterWriteAny(StringBuilder text)
{
    foreach (var kindInfo in fieldKinds.Values)
    {
        if (kindInfo.Name == "Compact" || kindInfo.Name == "ArrayOfCompact") continue;

        var type = kindInfo.ClrType;

        if (kindInfo.IsNullable) type = $"{type}?";
        if (kindInfo.IsArray) type = $"{type}[]?";

        text.Append($@"            case FieldKind.{kindInfo.FullName}: writer.Write{kindInfo.FullName}(fieldname, ({type}) value); break;
");
    }
}

fileEditor.EditFile("src/Hazelcast.Net/Serialization/Compact/CompactWriterExtensions.GeneratedCode.cs",
    GenerateCompactWriterWriteAny);
