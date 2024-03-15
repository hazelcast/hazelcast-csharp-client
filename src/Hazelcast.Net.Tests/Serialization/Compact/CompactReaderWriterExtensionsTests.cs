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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
public class CompactReaderWriterExtensionsTests
{
    [Test]
    public void ReadWriteAny()
    {
        var kinds = (FieldKind[])Enum.GetValues(typeof(FieldKind));
        Assert.That(kinds[0], Is.EqualTo(FieldKind.NotAvailable));

        var schemaBuilder = SchemaBuilder.For("test");
        for (var i = 1; i < kinds.Length; i++)
        {
            schemaBuilder = schemaBuilder.WithField($"field-{kinds[i]}", kinds[i]);
        }
        var schema = schemaBuilder.Build();

        var orw = new ReflectionSerializerTests.ObjectReaderWriter(new ReflectionSerializer());
        var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
        var writer = new CompactWriter(orw, output, schema);

        for (var i = 1; i < kinds.Length; i++)
        {
            if (kinds[i] == FieldKind.Compact || kinds[i] == FieldKind.ArrayOfCompact) continue;
            writer.WriteAny($"field-{kinds[i]}", kinds[i], ReflectionDataSource.GetValueOfKind(kinds[i]));
        }

        Assert.Throws<NotSupportedException>(() => writer.WriteAny("field", FieldKind.NotAvailable, 1234));

        writer.Complete();

        var input = new ObjectDataInput(output.ToByteArray(), orw, Endianness.LittleEndian);
        var reader = new CompactReader(orw, input, schema, typeof(object));

        for (var i = 1; i < kinds.Length; i++)
        {
            if (kinds[i] == FieldKind.Compact || kinds[i] == FieldKind.ArrayOfCompact) continue;
            var expected = ReflectionDataSource.GetValueOfKind(kinds[i]);
            var value = reader.ReadAny($"field-{kinds[i]}", kinds[i]);
            Assert.That(value, Is.EqualTo(expected));
        }

        Assert.Throws<NotSupportedException>(() => reader.ReadAny("field", FieldKind.NotAvailable));
    }
}
