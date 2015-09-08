using System;
using System.Threading;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    internal static class TestSupport
    {
        private const int TimeoutSeconds = 120;

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
    }
}