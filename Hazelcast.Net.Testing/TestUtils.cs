using System;
using Hazelcast.Core;

namespace Hazelcast.Testing
{
    public static class TestUtils
    {
        public static T[] RandomArray<T>(Func<T> randFunc, int size = 0)
        {
            var array = new T[size == 0 ? RandomProvider.Random.Next(5) + 1 : size];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = randFunc();
            }
            return array;
        }

        public static bool RandomBool()
        {
            return RandomProvider.Random.Next() > 0;
        }

        public static byte RandomByte()
        {
            return (byte) RandomProvider.Random.Next();
        }

        public static byte[] RandomBytes()
        {
            var bytes = new byte[10];
            RandomProvider.Random.NextBytes(bytes);
            return bytes;
        }

        public static char RandomChar()
        {
            return RandomString()[0];
        }

        public static double RandomDouble()
        {
            return RandomProvider.Random.NextDouble();
        }

        public static float RandomFloat()
        {
            return (float) RandomProvider.Random.NextDouble();
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
            return (short) RandomProvider.Random.Next();
        }

        public static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
