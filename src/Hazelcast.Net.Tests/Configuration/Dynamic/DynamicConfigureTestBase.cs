// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration.Dynamic;

public class DynamicConfigureTestBase : SingleMemberRemoteTestBase
{
    public const string ResultIsJavaMessageBytes = @"
var ArrayOfBytes = Java.type(""byte[]"")
var bytes = new ArrayOfBytes(message.getBufferLength())
var frame = message.getStartFrame()
var ix = 0;
while (frame != null)
{
    //writeIntL(frame.content.length + 6)
    var len = frame.content.length + 6
    bytes[ix++] = len & 0xff
    bytes[ix++] = (len >>> 8) & 0xff
    bytes[ix++] = (len >>> 16) & 0xff
    bytes[ix++] = (len >>> 24) & 0xff
    var flags = frame.flags
    if (frame == message.getEndFrame()) flags |= 8192 // IS_FINAL_FLAG
    //writeShortL(flags)
    bytes[ix++] = flags & 0xff
    bytes[ix++] = (flags >>> 8) & 0xff
    //writeBytes(frame.content)
    for (var i = 0; i < frame.content.length; i++) bytes[ix++] = frame.content[i]
    frame = frame.next
}

function h2s(b) {
    if (b < 0) b = 0xFF + b + 1
    var s = b.toString(16)
    if (s.length < 2) s = ""0"" + s
    return s
}

var result = """"
for (var i = 0; i < bytes.length; i++) result += h2s(bytes[i])
";

    public async Task<byte[]> ScriptToBytes(string script)
    {
        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Success, $"message: {response.Message}");
        Assert.That(response.Result, Is.Not.Null);
        var stringData = Encoding.ASCII.GetString(response.Result);
        var javaBytesLength = stringData.Length / 2;
        var javaBytes = new byte[javaBytesLength];
        for (var i = 0; i < javaBytesLength; i++)
            javaBytes[i] = byte.Parse(stringData.Substring(i * 2, 2), NumberStyles.HexNumber);
        return javaBytes;
    }

    internal byte[] MessageToBytes(ClientMessage message)
    {
        var dotnetBytesLength = 0;
        for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
            dotnetBytesLength += frame.Length;

        var dotnetBytes = new byte[dotnetBytesLength];
        var ix = 0;
        for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
        {
            //writeIntL(frame.content.length + 6)
            var len = frame.Length;
            dotnetBytes.WriteIntL(ix, len);
            ix += BytesExtensions.SizeOfInt;
            var flags = frame.Flags;
            if (frame == message.LastFrame) flags |= FrameFlags.Final;
            //writeShortL(flags)
            dotnetBytes.WriteShortL(ix, (short)flags);
            ix += BytesExtensions.SizeOfShort;
            //writeBytes(frame.content)
            for (var i = 0; i < frame.Bytes.Length; i++) dotnetBytes[ix++] = frame.Bytes[i];
        }

        return dotnetBytes;
    }

    public void AssertMessagesAreIdentical(byte[] bytes0, byte[] bytes1)
    {
        Assert.That(bytes1.Length, Is.EqualTo(bytes0.Length), "Lengths are different");

        var position = 0;
        var f = 0;
        while (position < bytes0.Length && position < bytes1.Length)
        {
            var frameLength0 = bytes0.ReadIntL(position) - 6;
            var frameLength1 = bytes1.ReadIntL(position) - 6;
            position += BytesExtensions.SizeOfInt;

            if (frameLength0 != frameLength1)
            {
                Console.WriteLine($"FRAME {f:000}: len {frameLength0} != {frameLength1}");
                for (var k = 0; k < 64; k++)
                    Console.Write($"{bytes0[position + k]:X2} ");
                Console.WriteLine();
                for (var k = 0; k < 64; k++)
                    Console.Write($"{bytes1[position + k]:X2} ");
                Console.WriteLine();
            }
            Assert.That(frameLength1, Is.EqualTo(frameLength0), $"Frames {f} have different lengths");

            var frameFlags0 = bytes0.ReadShortL(position);
            var frameFlags1 = bytes0.ReadShortL(position);
            position += BytesExtensions.SizeOfShort;

            if (frameFlags0 != frameFlags1)
            {
                Console.WriteLine($"FRAME {f:000}: flags {frameFlags1} != {frameFlags0}");
            }
            Assert.That(frameFlags1, Is.EqualTo(frameFlags0), $"Frames {f} have different flags");

            Console.Write($"FRAME {f++:000}: {frameFlags0:X4} {frameLength0:00000000} ");
            var i = 0;
            while (i < frameLength0)
            {
                if (bytes0[position + i] != bytes1[position + i]) break;
                i++;
            }
            if (i == frameLength0) Console.WriteLine("OK");
            else
            {
                Console.WriteLine($"ERR at byte {i}");
                Console.Write(" -  ");
                for (var j = 0; j < frameLength0; j++) Console.Write($"{bytes0[position + j]:X2} ");
                Console.WriteLine();
                Console.Write(" -  ");
                for (var j = 0; j < frameLength1; j++) Console.Write($"{bytes1[position + j]:X2} ");
                Console.WriteLine();
                Console.Write("    ");
                for (var j = 0; j < i; j++) Console.Write($"   ");
                Console.WriteLine("^^");
            }
            Assert.That(i, Is.EqualTo(frameLength0), $"Frames {f} have different bytes");

            position += frameLength0;
        }
    }
}
