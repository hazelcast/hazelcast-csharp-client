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
using System;
using System.Globalization;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization;

[TestFixture]
public class PolyglotTests : SingleMemberClientRemoteTestBase
{
    [Test]
    public void AssertGuidStructOrder()
    {
        var guid = Guid.NewGuid();

        var s = guid.ToString("N");
        var bytes = new byte[16];
        for (var i = 0; i < 16; i++) bytes[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);

        var v = "";
        for (var i = 0; i < 16; i++) v += $"{bytes[i]:x2}";
        Assert.That(v, Is.EqualTo(s)); // verification of the byte array

        // assert that the UUID bytes X0, X1... are in the MSB-first Guid order
        var uuid = new JavaUuidOrder { Value = guid };
        var b = 0;
        Assert.That(uuid.X0, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X1, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X2, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X3, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X4, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X5, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X6, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X7, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X8, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.X9, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XA, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XB, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XC, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XD, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XE, Is.EqualTo(bytes[b++]));
        Assert.That(uuid.XF, Is.EqualTo(bytes[b++]));
    }

    [Test]
    public async Task CanDeserializeJavaGuid()
    {
        const string script = @"

var UUID = Java.type(""java.util.UUID"")
var uuid = UUID.randomUUID()

var codec = Java.type(""com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec"")
var ArrayOfBytes = Java.type(""byte[]"")
var bytes = new ArrayOfBytes(32)
codec.encodeUUID(bytes, 0, uuid)

function h2s(b) {
    if (b < 0) b = 0xFF + b + 1
    var s = b.toString(16)
    if (s.length < 2) s = ""0"" + s
    return s
}

var s = """"
for (var i = 0; i < 16; i++) s += h2s(bytes[i+1])

result = """" + s + "" "" + uuid.toString()
"
        ;

        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Success, $"message: {response.Message}");
        Assert.That(response.Result, Is.Not.Null);
        var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
        Console.WriteLine($"Returns >{resultString}<");

        var parts = resultString.Split(' ');
        var bytesValue = parts[0];
        var stringValue = parts[1];

        var bytes = new byte[32];
        bytes[0] = 0;
        for (var i = 0; i < 16; i++)
        {
            bytes[i + 1] = byte.Parse(bytesValue.Substring(i * 2, 2), NumberStyles.HexNumber);
            Console.Write($"{bytes[i + 1]:x2}");
        }

        Console.WriteLine();

        var guid = bytes.ReadGuidL(0);
        var s = guid.ToString("D");
        Console.WriteLine(s);

        Assert.That(s, Is.EqualTo(stringValue));
    }

    [Test]
    public async Task CanSerializeJavaGuid()
    {
        const string scriptTemplate = @"

var Byte = Java.type(""byte"")
var ArrayOfBytes = Java.type(""byte[]"")
var bytes = new ArrayOfBytes(32)
var s = ""$$BYTES$$""
bytes[0] = 0;
for (var i = 0; i < s.length/2; i++) bytes[i] = parseInt(s.substr(i*2,2), 16)

var codec = Java.type(""com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec"")
var uuid = codec.decodeUUID(bytes, 0)

function h2s(b) {
    if (b < 0) b = 0xFF + b + 1
    var s = b.toString(16)
    if (s.length < 2) s = ""0"" + s
    return s
}

var r = """"
for (var i = 0; i < 16; i++) r += h2s(bytes[i+1])


result = """" + r + "" "" + uuid.toString()
"
            ;

        var guid = Guid.NewGuid();
        Console.WriteLine($"{guid:D}");
        var bytes = new byte[32];
        bytes.WriteGuidL(0, guid);
        var s = "";
        for (var i = 0; i < bytes.Length; i++) s += bytes[i].ToString("x2");
        Console.WriteLine(s);

        var script = scriptTemplate.Replace("$$BYTES$$", s);

        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Success, $"message: {response.Message}");
        Assert.That(response.Result, Is.Not.Null);
        var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
        Console.WriteLine($"Returns >{resultString}<");

        var parts = resultString.Split(' ');
        
        Assert.That(parts[1], Is.EqualTo(guid.ToString("D")));
    }
}