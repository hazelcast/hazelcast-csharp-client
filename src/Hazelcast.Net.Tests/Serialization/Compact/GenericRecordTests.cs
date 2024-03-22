// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NSubstitute;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
[ServerCondition("[5.2,)")]
public class GenericRecordTests : SingleMemberClientRemoteTestBase
{
    [Test]
    public void TestLocal()
    {
        var rec0 = GenericRecordBuilder.Compact("rec0")
            .SetBoolean("bool", true)
            .SetInt32("int", 123)
            .SetString("string", "hello")
            .Build();

        Assert.That(rec0.GetBoolean("bool"), Is.True);
        Assert.That(rec0.GetInt32("int"), Is.EqualTo(123));
        Assert.That(rec0.GetString("string"), Is.EqualTo("hello"));

        Assert.Throws<SerializationException>(() => rec0.GetInt32("meh"));
        Assert.Throws<SerializationException>(() => rec0.GetString("int"));

        var now = (HLocalDate) DateTime.Now;

        var rec1 = GenericRecordBuilder.Compact("rec1")
            .SetDate("date", now)
            .SetGenericRecord("rec", rec0)
            .Build();

        Assert.That(rec1.GetDate("date"), Is.EqualTo(now));

        var rec1A = rec1.GetGenericRecord("rec");
        Assert.That(rec1A, Is.Not.Null);
        if (rec1A is null) throw new Exception(); // we *know* it's not null but R# does not

        Assert.That(rec1A.GetBoolean("bool"), Is.True);
        Assert.That(rec1A.GetInt32("int"), Is.EqualTo(123));
        Assert.That(rec1A.GetString("string"), Is.EqualTo("hello"));

        var rec2 = GenericRecordBuilder.Compact("rec2")
            .SetInt32("int", 123)
            .Build();

        Assert.Throws<SerializationException>(() => rec2.NewBuilder().Build());

        var unused0 = rec2.NewBuilderWithClone().Build();
        var unused1 = rec2.NewBuilder().SetInt32("int", 567).Build();

        Assert.Throws<SerializationException>(() => rec2.NewBuilderWithClone().SetInt32("other", 567).Build());

        Assert.Throws<ArgumentException>(() =>
            GenericRecordBuilder.Compact("xxx").SetGenericRecord("xxx", Substitute.For<IGenericRecord>()).Build());
    }

    [Test]
    public async Task TestRemote()
    {
        var rec0 = GenericRecordBuilder.Compact("rec0")
            .SetBoolean("bool", true)
            .SetInt32("int", 123)
            .SetString("string", "hello")
            .Build();

        var mapOfGenericRecord = await Client.GetMapAsync<int, IGenericRecord>("generic-record-map");

        // can set the IGenericRecord value
        await mapOfGenericRecord.SetAsync(0, rec0);

        // can get the value as an IGenericRecord
        var rec0A = await mapOfGenericRecord.GetAsync(0);
        Assert.That(rec0A, Is.Not.Null);
        Assert.That(rec0A, Is.InstanceOf<CompactGenericRecordBase>());
        Assert.That(rec0A, Is.InstanceOf<CompactDictionaryGenericRecord>());
        Assert.That(rec0A.GetString("string"), Is.EqualTo("hello"));

        await mapOfGenericRecord.DisposeAsync();
        await using var mapOfObject = await Client.GetMapAsync<int, object>("generic-record-map");

        // can get the value as an object -  because we have no registration for
        // the compact type "rec0", we fall back to generic record
        var rec0B = await mapOfGenericRecord.GetAsync(0);
        Assert.That(rec0B, Is.Not.Null);
        Assert.That(rec0B, Is.InstanceOf<CompactGenericRecordBase>());
        Assert.That(rec0B, Is.InstanceOf<CompactDictionaryGenericRecord>());
        Assert.That(rec0B.GetString("string"), Is.EqualTo("hello"));

        await mapOfObject.DisposeAsync();
    }

    [Test]
    public void CoverGenericRecord()
    {
        var schema = SchemaBuilder.For("type-name")
            .WithField("field-name-1", FieldKind.Boolean)
            .WithField("field-name-2", FieldKind.Int8)
            .WithField("field-name-3", FieldKind.String)
            .Build();

        var fieldValues = new Dictionary<string, object?>
        {
            {"field-name-1", true},
            {"field-name-2", 123},
            {"field-name-3", "hello"}
        };

        var rec = new CompactDictionaryGenericRecord(schema, fieldValues);

        Assert.That(rec.GetFieldKind("field-name-1"), Is.EqualTo(FieldKind.Boolean));
        Assert.That(rec.GetFieldKind("field-name-2"), Is.EqualTo(FieldKind.Int8));
        Assert.That(rec.GetFieldKind("field-name-3"), Is.EqualTo(FieldKind.String));

        var fieldNames = rec.FieldNames;
        Assert.That(fieldNames.Count, Is.EqualTo(3));
        Assert.That(fieldNames, Does.Contain("field-name-1"));
        Assert.That(fieldNames, Does.Contain("field-name-2"));
        Assert.That(fieldNames, Does.Contain("field-name-3"));

        Assert.Throws<ArgumentNullException>(() => _ = new CompactDictionaryGenericRecord(null!, fieldValues));
        Assert.Throws<ArgumentNullException>(() => _ = new CompactDictionaryGenericRecord(schema, null!));
    }

    [Test]
    public void CoverGenericRecordBuilder()
    {
        var schema = SchemaBuilder.For("type-name")
            .WithField("field-name-1", FieldKind.Boolean)
            .WithField("field-name-2", FieldKind.Int8)
            .WithField("field-name-3", FieldKind.String)
            .Build();

        Assert.Throws<ArgumentException>(() => _ = new CompactGenericRecordBuilder((string) null!));
        Assert.Throws<ArgumentException>(() => _ = new CompactGenericRecordBuilder(""));
        Assert.Throws<ArgumentNullException>(() => _ = new CompactGenericRecordBuilder((Schema)null!));
        Assert.Throws<ArgumentNullException>(() => _ = new CompactGenericRecordBuilder(null!, new Dictionary<string, object?>()));
        Assert.Throws<ArgumentNullException>(() => _ = new CompactGenericRecordBuilder(schema, null!));

        var builder = new CompactGenericRecordBuilder(schema);

        // getting an ArgumentException with "should not be null nor empty" in both cases
        // we don't differentiate null & empty string exception to keep it simple
        Assert.Throws<ArgumentException>(() => builder.SetBoolean(null!, true));
        Assert.Throws<ArgumentException>(() => builder.SetBoolean("", true));

        builder.SetBoolean("field-name-1", true); // set it once
        Assert.Throws<SerializationException>(() => builder.SetBoolean("field-name-1", true)); // not twice

        var fieldValues = new Dictionary<string, object?>
        {
            {"field-name-1", true},
            {"field-name-2", 123},
            {"field-name-3", "hello"}
        };
        builder = new CompactGenericRecordBuilder(schema, fieldValues);
        builder.SetBoolean("field-name-1", true); // override it once
        Assert.Throws<SerializationException>(() => builder.SetBoolean("field-name-1", true)); // not twice
    }

    [Test]
    public void CoverGenericRecordSerializer()
    {
        var serializer = new CompactGenericRecordSerializer();

        Assert.Throws<NotSupportedException>(() => _ = serializer.TypeName);
    }

    [Test]
    public void CompactGenericRecordBuilderWithSchemaCanBuild()
    {
        var kinds = (FieldKind[]) Enum.GetValues(typeof(FieldKind));
        Assert.That(kinds[0], Is.EqualTo(FieldKind.NotAvailable));

        var schemaBuilder = SchemaBuilder.For("test");
        for (var i = 0; i < kinds.Length; i++)
        {
            if (kinds[i] == FieldKind.NotAvailable) continue;
            schemaBuilder = schemaBuilder.WithField($"field-{i}", kinds[i]);
        }
        var schema = schemaBuilder.Build();

        var recordBuilder = new CompactGenericRecordBuilder(schema);

        // cannot use a name that's not in the schema
        Assert.Throws<SerializationException>(() => recordBuilder.SetBoolean("no-field", true));

        // cannot use a field of the wrong type
        {
            var m = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kinds[1]}");
            Assert.That(m, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kinds[1]}");
            var v = ReflectionDataSource.GetValueOfType(m!.GetParameters()[1].ParameterType);
            var e = Assert.Throws<TargetInvocationException>(() => m.Invoke(recordBuilder, new[] { "field-2", v }));
            Assert.That(e.InnerException, Is.InstanceOf<SerializationException>());
        }

        // can set all fields once
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kind}");
            var v = ReflectionDataSource.GetValueOfType(m!.GetParameters()[1].ParameterType);
            m.Invoke(recordBuilder, new[] { $"field-{i}", v });
        }

        // cannot set any field twice
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kind}");
            var v = ReflectionDataSource.GetValueOfType(m!.GetParameters()[1].ParameterType);
            var e = Assert.Throws<TargetInvocationException>(() => m.Invoke(recordBuilder, new[] { $"field-{i}", v }));
            Assert.That(e.InnerException, Is.InstanceOf<SerializationException>());
        }

        // can build
        var rec = recordBuilder.Build();

        // can read all fields
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(IGenericRecord).GetMethod($"Get{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method IGenericRecord.Get{kind}");
            var ms = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(ms, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kind}");
            var v = ReflectionDataSource.GetValueOfType(ms!.GetParameters()[1].ParameterType);
            var r = m!.Invoke(rec, new object[] { $"field-{i}" });
            Assert.That(r, Is.EqualTo(v));
        }
    }

    [Test]
    public void CompactGenericRecordBuilderWithoutSchemaCanBuild()
    {
        var kinds = (FieldKind[])Enum.GetValues(typeof(FieldKind));
        Assert.That(kinds[0], Is.EqualTo(FieldKind.NotAvailable));

        var recordBuilder = new CompactGenericRecordBuilder("test");

        Assert.Throws<ArgumentException>(() =>
            recordBuilder.SetArrayOfGenericRecord("field-x", new[]
            {
                Substitute.For<IGenericRecord>()
            }));

        // can set all fields once
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kinds[i]}");
            var v = ReflectionDataSource.GetValueOfType(m!.GetParameters()[1].ParameterType);
            m.Invoke(recordBuilder, new[] { $"field-{i}", v });
        }

        // cannot set any field twice
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kinds[i]}");
            var v = ReflectionDataSource.GetValueOfType(m!.GetParameters()[1].ParameterType);
            var e = Assert.Throws<TargetInvocationException>(() => m.Invoke(recordBuilder, new[] { $"field-{i}", v }));
            Assert.That(e.InnerException, Is.InstanceOf<SerializationException>());
        }

        // can build
        var rec = recordBuilder.Build();

        // can read all fields
        for (var i = 1; i < kinds.Length; i++)
        {
            var kind = kinds[i] switch
            {
                FieldKind.Compact => "GenericRecord",
                FieldKind.ArrayOfCompact => "ArrayOfGenericRecord",
                _ => kinds[i].ToString()
            };
            var m = typeof(IGenericRecord).GetMethod($"Get{kind}");
            Assert.That(m, Is.Not.Null, $"Failed to get method IGenericRecord.Get{kind}");
            var ms = typeof(CompactGenericRecordBuilder).GetMethod($"Set{kind}");
            Assert.That(ms, Is.Not.Null, $"Failed to get method CompactGenericRecordBuilder.Set{kind}");
            var v = ReflectionDataSource.GetValueOfType(ms!.GetParameters()[1].ParameterType);
            var r = m.Invoke(rec, new object[] { $"field-{i}" });
            Assert.That(r, Is.EqualTo(v));
        }
    }

    [Test]
    public void DictionaryGenericRecordEdgeCase1()
    {
        // array of int can be obtained as array of int?

        var schema = SchemaBuilder.For("test").WithField("field", FieldKind.ArrayOfInt32).Build();
        var fieldValues = new Dictionary<string, object?>
        {
            {"field", new[] { 1, 2, 3 } }
        };
        var rec = new CompactDictionaryGenericRecord(schema, fieldValues);
        var a = rec.GetArrayOfInt32("field");
        Assert.That(a, Is.Not.Null);
        Assert.That(a!.Length, Is.EqualTo(3));
        Assert.That(a, Is.EquivalentTo(new[] { 1, 2, 3 }));
        var an = rec.GetArrayOfNullableInt32("field");
        Assert.That(an, Is.Not.Null);
        Assert.That(an!.Length, Is.EqualTo(3));
        Assert.That(a, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void DictionaryGenericRecordEdgeCase2()
    {
        // array of int? can be obtained as array of int

        var schema = SchemaBuilder.For("test").WithField("field", FieldKind.ArrayOfNullableInt32).Build();
        var fieldValues = new Dictionary<string, object?>
        {
            { "field", new int?[] { 1, 2, 3 } }
        };
        var rec = new CompactDictionaryGenericRecord(schema, fieldValues);
        var a = rec.GetArrayOfInt32("field");
        Assert.That(a, Is.Not.Null);
        Assert.That(a!.Length, Is.EqualTo(3));
        Assert.That(a, Is.EquivalentTo(new[] { 1, 2, 3 }));
        var an = rec.GetArrayOfNullableInt32("field");
        Assert.That(an, Is.Not.Null);
        Assert.That(an!.Length, Is.EqualTo(3));
        Assert.That(a, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void DictionaryGenericRecordEdgeCase3()
    {
        // null array of int can be obtained as (null) array of int?

        var schema = SchemaBuilder.For("test").WithField("field", FieldKind.ArrayOfInt32).Build();
        var fieldValues = new Dictionary<string, object?>
        {
            { "field", null }
        };
        var rec = new CompactDictionaryGenericRecord(schema, fieldValues);
        var a = rec.GetArrayOfInt32("field");
        Assert.That(a, Is.Null);
        var an = rec.GetArrayOfNullableInt32("field");
        Assert.That(an, Is.Null);
    }

    [Test]
    public void DictionaryGenericRecordEdgeCase4()
    {
        // null array of int? can be obtained as (null) array of int

        var schema = SchemaBuilder.For("test").WithField("field", FieldKind.ArrayOfNullableInt32).Build();
        var fieldValues = new Dictionary<string, object?>
        {
            { "field", null }
        };
        var rec = new CompactDictionaryGenericRecord(schema, fieldValues);
        var a = rec.GetArrayOfInt32("field");
        Assert.That(a, Is.Null);
        var an = rec.GetArrayOfNullableInt32("field");
        Assert.That(an, Is.Null);
    }
}