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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class FingerprintTests
    {
        [Test]
        public void CanFingerprintNullString()
        {
            var fingerprint = RabinFingerprint.InitialValue;
            fingerprint = RabinFingerprint.Fingerprint(fingerprint, (string)null);
            Assert.That(fingerprint, Is.EqualTo(16130581598588476612L)); // yes - that magic number
        }

        [Test]
        public void CanFingerprintStringSameAsJava()
        {
            const string text = @"Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.";

            var bytes = Encoding.UTF8.GetBytes(text);
            var fingerprint = (long) RabinFingerprint.Fingerprint(RabinFingerprint.InitialValue, bytes);

            // then, push to Java and compare
            using var run = new JavaRun()
                .WithSource("Java/SerializationTests/Compact/FingerprintString.java")
                .WithSource("Java/SerializationTests/Compact/RabinFingerprint.java");
            run.Compile();
            var output = run.Execute("FingerprintString", bytes);

            Assert.That(output, Is.Not.Null);
            var javaFingerprintString = output.Trim();
            Console.WriteLine($"Parse >{javaFingerprintString}<");

            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(javaFingerprintString, out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));
        }

        private static IEnumerable<object[]> CanFingerprintSchemaSameAsJavaSource()
        {
            yield return new object[]
            {
                new Schema("typename", new[]
                {
                    new SchemaField("fieldname", FieldKind.NullableString)
                }),
                @"
                    fields.put(""fieldname"", new FieldDescriptor(""fieldname"", FieldKind.STRING));
"
            };

            yield return new object[]
            {
                new Schema("foo", new[]
                {
                    new SchemaField("value", FieldKind.Int32)
                }),
                @"
                    fields.put(""value"", new FieldDescriptor(""value"", FieldKind.INT32));
"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("thing")
                    .WithField("name", FieldKind.NullableString)
                    .WithField("value", FieldKind.Int32)
                    .Build(),
                @"
                    fields.put(""name"", new FieldDescriptor(""name"", FieldKind.STRING));
                    fields.put(""value"", new FieldDescriptor(""value"", FieldKind.INT32));
"
            };
        }

        [TestCaseSource(nameof(CanFingerprintSchemaSameAsJavaSource))]
        public void CanFingerprintSchemaSameAsJava(Schema schema, string javaFields)
        {
            var fingerprint = schema.Id;

            var text = TestFiles.ReadAllText(this, "Java/SerializationTests/Compact/FingerprintSchema.java");
            text = text.Replace("$$TYPENAME$$", schema.TypeName);
            text = text.Replace("$$CLASSNAME$$", "FingerprintSchema");
            text = text.Replace("$$FIELDS$$", javaFields);

            using var run = new JavaRun()
                .WithSourceText("FingerprintSchema", text)
                .WithSource("Java/SerializationTests/Compact/RabinFingerprint.java")
                .WithLib($"hazelcast-{ServerVersion.DefaultVersion}.jar");
            run.Compile();
            var output = run.Execute("FingerprintSchema");

            Assert.That(output, Is.Not.Null);
            var javaFingerprintString = output.Trim();
            Console.WriteLine($"Parse >{javaFingerprintString}<");

            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(javaFingerprintString, out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));
        }
    }
}
