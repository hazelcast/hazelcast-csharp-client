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

#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Hazelcast.Serialization.Compact
{
    // implements Rabin fingerprint as per the Java source code, and we have tests to prove it
    // produces ulong (unsigned) fingerprints because that is what they are, really, and even
    // the initial value is not a valid long value. can always be converted to long if we need
    // to align the result with Java.
    internal static class RabinFingerprint
    {
        public const ulong InitialValue = 0xc15d213aa4d7a795UL;
        private static readonly ulong[] FpTable = new ulong[256]; // populated in static ctor

        static RabinFingerprint()
        {
            for (var i = 0UL; i < 256; i++)
            {
                var fingerprint = i;
                for (var j = 0; j < 8; j++)
                {
                    fingerprint = (fingerprint >> 1) ^ (InitialValue & (ulong) -((long) fingerprint & 1L));
                }
                FpTable[i] = fingerprint;
            }
        }

        // fingerprints a single byte value
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Fingerprint(ulong fingerprint, byte value)
            => (fingerprint >> 8) ^ FpTable[(int)(fingerprint ^ value) & 0xff];

        // fingerprints a little-endian representation of an integer value
        public static ulong Fingerprint(ulong fingerprint, int value)
        {
            fingerprint = Fingerprint(fingerprint, (byte)((value >> 0) & 0xff));
            fingerprint = Fingerprint(fingerprint, (byte)((value >> 8) & 0xff));
            fingerprint = Fingerprint(fingerprint, (byte)((value >> 16) & 0xff));
            fingerprint = Fingerprint(fingerprint, (byte)((value >> 24) & 0xff));
            return fingerprint;
        }

        // fingerprints a string value
        public static ulong Fingerprint(ulong fingerprint, string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var bytes = Encoding.UTF8.GetBytes(value);
            fingerprint = Fingerprint(fingerprint, bytes.Length);

            // trust .NET to optimize the foreach call over an array and to JIT-inline the method
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var b in bytes) fingerprint = Fingerprint(fingerprint, b);

            return fingerprint;
        }
    }
}
