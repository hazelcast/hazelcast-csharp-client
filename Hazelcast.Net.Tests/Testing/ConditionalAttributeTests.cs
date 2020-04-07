#define LOG_A

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class ConditionalAttributeTests
    {
        public ConditionalAttributeTests()
        {
            LogA.Prefix(this, "XXXA ");
            LogB.Prefix(this, "XXXB ");
        }

        [Test]
        public void Do()
        {
            var capture = new ConsoleCapture();
            using (capture.Output())
            {
                LogA.WriteLine(this, "BLAHA");
                LogB.WriteLine(this, "BLAHB");
            }
            Assert.AreEqual("XXXA BLAHA\n", capture.ReadToEnd().ToLf());
        }

        // LOB_A is defined, calls to this are compiled and run
        private static class LogA
        {
#if LOG_A
            private static readonly ConditionalWeakTable<object, string> _prefixes
                = new ConditionalWeakTable<object, string>();
#endif

            [Conditional("LOG_A")]
            public static void Prefix(object o, string prefix)
            {
#if LOG_A
                _prefixes.Add(o, prefix);
#endif
            }

            [Conditional("LOG_A")]
            public static void WriteLine(object o, string text)
            {
#if LOG_A
                _prefixes.TryGetValue(o, out var prefix);
                Console.WriteLine(prefix + text);
#endif
            }
        }

        // LOG_B is *not* defined, this does nothing
        // and calls to this are *not* compiled
        private static class LogB
        {
#if LOG_B
        private static readonly ConditionalWeakTable<object, string> _prefixes
            = new ConditionalWeakTable<object, string>();
#endif

            [Conditional("LOG_B")]
            public static void Prefix(object o, string prefix)
            {
#if LOG_B
            _prefixes.Add(o, prefix);
#endif
            }

            [Conditional("LOG_B")]
            public static void WriteLine(object o, string text)
            {
#if LOG_B
            _prefixes.TryGetValue(o, out var prefix);
            Console.WriteLine(prefix + text);
#endif
            }
        }
    }
}
