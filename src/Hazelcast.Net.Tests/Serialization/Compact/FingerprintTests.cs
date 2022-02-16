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

        [Test]
        public void CanFingerprintSchema1SameAsJava()
        {
            var schema = new Schema("typename", new[]
            {
                new SchemaField("fieldname", FieldKind.NullableString)
            });

            CanFingerprintSchemaSameAsJava(schema, 1);
        }

        [Test]
        public void CanFingerprintSchema2SameAsJava()
        {
            var schema = new Schema("foo", new[]
            {
                new SchemaField("value", FieldKind.Int32)
            });

            CanFingerprintSchemaSameAsJava(schema, 2);
        }

        [Test]
        public void CanFingerprintSchema3SameAsJava()
        {
            var schema = SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build();

            CanFingerprintSchemaSameAsJava(schema, 3);
        }

        private void CanFingerprintSchemaSameAsJava(Schema schema, int n)
        {
            var fingerprint = schema.Id;

            //foreach (var field in schema.Fields) Console.WriteLine(field.FieldName);

            using var run = new JavaRun()
                .WithSource($"Java/SerializationTests/Compact/FingerprintSchema{n}.java")
                .WithSource("Java/SerializationTests/Compact/RabinFingerprint.java")
                .WithLib($"hazelcast-{ServerVersion.DefaultVersion}.jar");
            run.Compile();
            var output = run.Execute($"FingerprintSchema{n}");

            Assert.That(output, Is.Not.Null);
            var javaFingerprintString = output.Trim();
            Console.WriteLine($"Parse >{javaFingerprintString}<");

            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(javaFingerprintString, out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));
        }
    }
}
