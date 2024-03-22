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
using Hazelcast.Core;

namespace Hazelcast.Testing
{
    public static class TestUtils
    {
        public static T[] RandomArray<T>(Func<T> randFunc, int size = 0)
        {
            return RandomArray<T>(p => randFunc(), size);
        }

        public static T[] RandomArray<T>(Func<int, T> randFunc, int size = 0)
        {
            var array = new T[size == 0 ? RandomProvider.Random.Next(5) + 1 : size];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = randFunc(i);
            }
            return array;
        }

        public static bool RandomBool()
        {
            return RandomProvider.Random.Next() > 0;
        }

        public static byte RandomByte()
        {
            return (byte)RandomProvider.Random.Next();
        }

        public static byte[] RandomBytes()
        {
            var bytes = new byte[10];
            RandomProvider.Random.NextBytes(bytes);
            return bytes;
        }

        public static char RandomChar()
        {
            return (char)RandomProvider.Random.Next(0x1000); // but avoid surrogate pairs!
        }

        public static double RandomDouble()
        {
            return RandomProvider.Random.NextDouble();
        }

        public static float RandomFloat()
        {
            return (float)RandomProvider.Random.NextDouble();
        }

        public static int RandomInt()
        {
            return RandomProvider.Random.Next();
        }

        public static long RandomLong()
        {
            var buffer = new byte[8];
            RandomProvider.Random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static short RandomShort()
        {
            return (short)RandomProvider.Random.Next();
        }

        public static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }

        public static string RandomStringWithLength(int length)
        {
            var str = RandomString();
            return str.Substring(0, length < str.Length ? length : str.Length);
        }
    }
}
