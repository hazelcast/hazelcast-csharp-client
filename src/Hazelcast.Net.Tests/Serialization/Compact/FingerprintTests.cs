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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
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
        [TestCase("a,b,c")]
        [TestCase("A,b,c")]
        [TestCase("I,b,c")]
        [TestCase("I,ô,c")]
        [TestCase("I,$,c,_,e45,F34,é,ç,à")]
        [TestCase("É,Ç,À,é,ç,à")]
        public async Task CanOrderSameAsJava(string valuesString)
        {
            const string scriptTemplate = @"
// import types
var Comparator = Java.type(""java.util.Comparator"")

// Java generic are magic, just ignore them
var ArrayList = Java.type(""java.util.ArrayList"")

function f(x) { return x }

// create a schema & fingerprint it
var values = new ArrayList()
$$VALUES$$
var comparator = Comparator.comparing(f);
values.sort(comparator);
result = """" + values // as a string
";
            var values = valuesString.Split(',').Select(x => x.Trim()).ToList();
            var script = scriptTemplate.Replace("$$VALUES$$", string.Join("\n", values.Select(x => $"values.add(\"{x}\")")));
            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            Assert.That(response.Success, $"message: {response.Message}");
            Assert.That(response.Result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
            var resultValues = resultString.TrimStart('[').TrimEnd(']').Split(',').Select(x => x.Trim()).ToList();

            // Ordinal is what works, InvariantCulture does not work, even for the most basic cases
            // e.g. Java "I, b, c" is "b, c, I" for dotnet
            var dotnetValues = values.OrderBy(x => x, StringComparer.Ordinal).ToList();

            Assert.That(resultValues.Count, Is.EqualTo(values.Count));
            Assert.That(dotnetValues.Count, Is.EqualTo(values.Count));
            Console.WriteLine($"Java:   {string.Join(",", resultValues)}");
            Console.WriteLine($"Dotnet: {string.Join(",", dotnetValues)}");
            for (var i = 0; i < values.Count; i++)
                Assert.That(resultValues[i], Is.EqualTo(dotnetValues[i]));
        }

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

            yield return new object[]
            {
                SchemaBuilder
                    .For("Trade")
                    .WithField("ID", FieldKind.Int32)
                    .WithField("action", FieldKind.String)
                    .WithField("sourceTradeId", FieldKind.String)
                    .Build(),
                "ID,INT32;action,STRING;sourceTradeId,STRING"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("xxx")
                    .WithField("i", FieldKind.Int32)
                    .WithField("b", FieldKind.String)
                    .WithField("c", FieldKind.String)
                    .Build(),
                "i,INT32;b,STRING;c,STRING"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("xxx")
                    .WithField("I", FieldKind.Int32)
                    .WithField("b", FieldKind.String)
                    .WithField("c", FieldKind.String)
                    .Build(),
                "I,INT32;b,STRING;c,STRING"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("xxx")
                    .WithField("a", FieldKind.String)
                    .WithField("b", FieldKind.String)
                    .WithField("c", FieldKind.String)
                    .Build(),
                "a,STRING;b,STRING;c,STRING"
            };

            yield return new object[]
            {
                SchemaBuilder
                    .For("xxx")
                    .WithField("I", FieldKind.String)
                    .WithField("b", FieldKind.String)
                    .WithField("c", FieldKind.String)
                    .Build(),
                "I,STRING;b,STRING;c,STRING"
            };
        }

        [TestCaseSource(nameof(CanFingerprintSchemaSameAsJavaSource))]
        public async Task CanFingerprintSchemaSameAsJava(object schemaObject, string javaFields)
        {
            var schema = schemaObject.MustBe<Schema>();

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
result += ""|""
for each (var fd in schema.getFields()) {
  result += fd.getFieldName() + "","" + fd.getOffset() + "","" + fd.getIndex() + "";""
}
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

            var resultStringParts = resultString.Split('|');
            Assert.That(resultStringParts.Length, Is.EqualTo(2));
            // beware! Java fingerprints are long, not ulong
            Assert.That(long.TryParse(resultStringParts[0], out var javaFingerprint));
            Assert.That(javaFingerprint, Is.EqualTo(fingerprint));

            var resultFields = resultStringParts[1].Split(';');
            var resultOffsets = new Dictionary<string, int>();
            var resultIndexes = new Dictionary<string, int>();
            foreach (var fp in resultFields)
            {
                var fpp = fp.Split(',');
                if (fpp.Length != 3) continue;
                Assert.That(int.TryParse(fpp[1], out var o));
                resultOffsets[fpp[0]] = o;
                Assert.That(int.TryParse(fpp[2], out var i));
                resultIndexes[fpp[0]] = i;
            }

            foreach (var field in schema.Fields)
            {
                Assert.That(resultOffsets.TryGetValue(field.FieldName, out var offset));
                Assert.That(resultIndexes.TryGetValue(field.FieldName, out var index));
                Assert.That(field.Offset, Is.EqualTo(offset), $"Field {field.FieldName} offset error.");
                Assert.That(field.Index, Is.EqualTo(index), $"Field {field.FieldName} index error.");
            }
        }
    }
}
