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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    [ServerCondition("[5.2,)")]
    public class FingerprintTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public void CannotFingerprintNullString()
        {
            Assert.Throws<ArgumentNullException>(() => RabinFingerprint.Fingerprint(RabinFingerprint.InitialValue, (string)null));
        }

        [Test]
        public async Task CanFingerprintStringSameAsJava()
        {
            var text = @"// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.";

            const string scriptTemplate = @"
// import types
var rabin = Java.type(""com.hazelcast.internal.serialization.impl.compact.RabinFingerprint"")

// alas, the method that fingerprints a string is private and thus, we have to cheat
var m = rabin.class.getDeclaredMethod(""fingerprint64"", Java.type(""long"").class, Java.type(""java.lang.String"").class)
m.setAccessible(true)

// same for the initial value (a field)
var f = rabin.class.getDeclaredField(""INIT"")
f.setAccessible(true)

var fingerprint = m.invoke(null, f.get(null), ""$$STRING$$"")

result = """" + fingerprint
";

            text = text.Replace("\r", "");

            // fingerprint on .NET
            var fingerprint = (long) RabinFingerprint.Fingerprint(RabinFingerprint.InitialValue, text);

            // fingerprint on Java
            var script = scriptTemplate
                .Replace("$$STRING$$", text.Replace("\n", "\\n").Replace("\"", "\\\""));

            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            Assert.That(response.Success, $"message: {response.Message}");
            Assert.That(response.Result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
            Console.WriteLine($"Parse >{resultString}<");

            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(resultString, out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));
        }

        private static IEnumerable<object[]> CanFingerprintSchemaSameAsJavaSource()
        {
            yield return new object[]
            {
                new Schema("typename", new[]
                {
                    new SchemaField("fieldname", FieldKind.String)
                }),
                "fieldname,STRING"
            };

            yield return new object[]
            {
                new Schema("foo", new[]
                {
                    new SchemaField("value", FieldKind.Int32)
                }),
                "value,INT32"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("thing")
                    .WithField("name", FieldKind.String)
                    .WithField("value", FieldKind.Int32)
                    .Build(),
                "name,STRING;value,INT32"
            };
        }

        [TestCaseSource(nameof(CanFingerprintSchemaSameAsJavaSource))]
        public async Task CanFingerprintSchemaSameAsJava(object schemaObject, string javaFields)
        {
            var schema = schemaObject as Schema;

            const string scriptTemplate = @"
// import types
var Schema = Java.type(""com.hazelcast.internal.serialization.impl.compact.Schema"")
var FieldDescriptor = Java.type(""com.hazelcast.internal.serialization.impl.compact.FieldDescriptor"")
var FieldKind = Java.type(""com.hazelcast.nio.serialization.FieldKind"")

// Java generic are magic, just ignore them
var ArrayList = Java.type(""java.util.ArrayList"")

// create a schema & fingerprint it
var fields = new ArrayList()
$$FIELDS$$
var schema = new Schema(""$$TYPENAME$$"", fields)
result = """" + schema.getSchemaId() // as a string
";
            
            var fingerprint = schema.Id;

            var javaFieldsDictionary = javaFields
                .Split(';')
                .Select(x => x.Split(','))
                .ToDictionary(x => x[0], x => x[1]);

            var script = scriptTemplate
                .Replace("$$TYPENAME$$", schema.TypeName)
                .Replace("$$FIELDS$$", string.Join("\n",
                    javaFieldsDictionary.Select(x => @$"fields.add(new FieldDescriptor(""{x.Key}"", FieldKind.{x.Value}))")));

            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);

            Assert.That(response.Success, $"message: {response.Message}");
            Assert.That(response.Result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
            Console.WriteLine($"Parse >{resultString}<");

            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(resultString, out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));
        }
    }
}
