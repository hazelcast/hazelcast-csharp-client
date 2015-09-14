using System;
using System.Threading;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    internal static class TestSupport
    {
        private const int TimeoutSeconds = 120;
        private static readonly Random Random = new Random();

        public static void AssertOpenEventually(CountdownEvent latch, int timeoutSeconds = TimeoutSeconds, string message = null)
        {
            var completed = latch.Wait(timeoutSeconds * 1000);
            if (message == null)
            {
                Assert.IsTrue(completed,
                    string.Format("CountDownLatch failed to complete within {0} seconds , count left: {1}",
                        timeoutSeconds,
                        latch.CurrentCount));
            }
            else
            {
                Assert.IsTrue(completed,
                    string.Format("{0}, CountDownLatch failed to complete within {1} seconds , count left: {2}", message,
                        timeoutSeconds,
                        latch.CurrentCount));
            }
        }

        public static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }

        public static char RandomChar()
        {
            return RandomString()[0];
        }

        public static short RandomShort()
        {
            return (short) Random.Next();
        }

        public static byte RandomByte()
        {
            return (byte) Random.Next();
        }

        public static byte[] RandomBytes()
        {
            var bytes = new byte[10];
            Random.NextBytes(bytes);
            return bytes;
        }

        public static T[] RandomArray<T>(Func<T> randFunc)
        {
            var array = new T[Random.Next(5) +1];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = randFunc();
            }
            return array;
        } 

        public static bool RandomBool()
        {
            return Random.Next() > 0;
        }

        public static int RandomInt()
        {
            return Random.Next();
        }
        public static long RandomLong()
        {
            byte[] buffer = new byte[8];
            Random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static float RandomFloat()
        {
            return (float) Random.NextDouble();
        }

        public static double RandomDouble()
        {
            return Random.NextDouble();
        }
    }
}