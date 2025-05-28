// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization;

[TestFixture]
public class GuidPolyglotTests : SingleMemberClientRemoteTestBase
{
    // BytesExtensions define ReadGuid/WriteGuid methods that can read/write Guid values
    // from/to an array of bytes, with an optional extra leading byte that is a boolean
    // which is used to flag empty Guid values - these extensions are used by codecs to
    // encode/decode Guid values
    //
    // Java uses FixedSizeTypesCode.encodeUUID/.decodeUUID which handle the flag and then,
    //
    // encodeUUID is:
    //   long mostSigBits = value.getMostSignificantBits();
    //   long leastSigBits = value.getLeastSignificantBits();
    //   encodeLong(buffer, pos + BOOLEAN_SIZE_IN_BYTES, mostSigBits);
    //   encodeLong(buffer, pos + BOOLEAN_SIZE_IN_BYTES + LONG_SIZE_IN_BYTES, leastSigBits);
    //
    //   note: encodeLong uses Bits.writeLongL (little-endian)
    //
    // decodeUUID is:
    //   long mostSigBits = decodeLong(buffer, pos + BOOLEAN_SIZE_IN_BYTES);
    //   long leastSigBits = decodeLong(buffer, pos + BOOLEAN_SIZE_IN_BYTES + LONG_SIZE_IN_BYTES);
    //   return new UUID(mostSigBits, leastSigBits);

    // GuidSerializer define Read/Write methods that can read/write Guid values from an
    // IObjectDataInput or to an IObjectDataOutput, without the leading flag - so it's always
    // the full Guid - and this is consistent with Java's UUIDSerializer -  this serializer
    // is used to serialize or deserialize Guid values into IData
    //
    // Java read is:
    //   return new UUID(in.readLong(), in.readLong());
    //
    // Java write is:
    //   out.writeLong(uuid.getMostSignificantBits());
    //   out.writeLong(uuid.getLeastSignificantBits());
    //
    //   note: writeLong uses Bits.writeLong(..., isBigEndian)

    // issue https://github.com/hazelcast/hazelcast/issues/19371 reported that when using
    // Guid/UUID on maps, Java was seeing wrong values. this was fixed by flipping the
    // field offsets in JavaUuidOrder to respect the natural (string) order, as tested by
    // the AssertGuidStructOrder test - and this is a good thing - but it broke the
    // BytesExtensions methods. This is because the codec values are always encoded as
    // little-endian, whereas the IData value respect the specified endianness (kinda,
    // because it does not swap *all* the bytes but only the two pairs of longs).

    // so the idea is:
    // - JavaUuidOrder Xn fields are in natural (string) order if Value is
    //   694fa83a-a282-449c-b2c0-9cc1f9f550c1, then X0 is 69, X1 is 4f, X2 is a8, X3 is
    //   3a, etc.
    // - JavaUuidOrder has ReadBytes/WriteBytes methods that read/write from/to an array
    //   of bytes using the Xn fields and getting/putting them in the right order for
    //   Java to be happy
    // - BytesExtensions methods (for codecs) use the JavaUuidOrder methods
    // - GuidSerializer methods (for IData) use the ByteExtensions methods
    // - AND we implement proper endianness support so it works in all cases
    //
    // this is verified by the tests in this class


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
    public async Task CanDeserializeJavaCodecGuid()
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
    public async Task CanSerializeJavaCodecGuid()
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

    [Test]
    public async Task CanSerializeJavaValueDataGuid()
    {
        // we set a Guid in the map, and then use a script to retrieve it as a string
        // which means we are getting Java's representation of the Guid - and then we
        // parse it and compare it to the original Guid, thus ensuring that Java sees
        // the same Guid as .NET

        var mapName = CreateUniqueName();
        await using var map = await Client.GetMapAsync<string, Guid>(mapName);
        var guid = Guid.NewGuid();
        await map.SetAsync("key", guid);

        Assert.That(await map.GetAsync("key"), Is.EqualTo(guid));

        var script = $@"
                result = """" + instance_0.getMap(""{mapName}"").get(""key"")
            ";

        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Result, Is.Not.Null);
        var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
        Console.WriteLine(guid.ToString("D"));
        Console.WriteLine(resultString);
        Assert.That(Guid.TryParse(resultString, out var resultGuid));
        Assert.That(resultGuid, Is.EqualTo(guid));
    }

    [Test]
    public async Task CanSerializeJavaKeyDataGuid()
    {
        // we use a Guid as a key, and then use a script to retrieve the value for
        // that key, passing the key as a string - and we ensure that we indeed get
        // a value, thus ensuring that Java sees the same Guid as .NET

        var mapName = CreateUniqueName();
        await using var map = await Client.GetMapAsync<Guid, string>(mapName);
        var guid = Guid.NewGuid();
        await map.SetAsync(guid, "value");

        Assert.That(await map.GetAsync(guid), Is.EqualTo("value"));

        var script = $@"
                var UUID = Java.type(""java.util.UUID"")
                var key = UUID.fromString(""{guid:D}"")
                result = """" + instance_0.getMap(""{mapName}"").get(key)
            ";

        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Result, Is.Not.Null);
        var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
        Console.WriteLine(resultString);
        Assert.That(resultString, Is.EqualTo("value"));
    }
}