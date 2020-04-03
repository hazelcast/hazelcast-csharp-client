#define zzLOG

using System.Diagnostics;
using NUnit.Framework;

namespace AsyncTests1
{
    [TestFixture]
    public class ConditionalAttributeTests
    {
        public ConditionalAttributeTests()
        {
            Log.Prefix(this, "BLAH ");
        }

        [Test]
        public void Do()
        {
            Log.WriteLine(this, "b");
        }
    }

    public static class Log
    {
#if LOG
        private static readonly ConditionalWeakTable<object, string> _prefixes
            = new ConditionalWeakTable<object, string>();
#endif

        [Conditional("LOG")]
        public static void Prefix(object o, string prefix)
        {
#if LOG
            _prefixes.Add(o, prefix);
#endif
        }

        [Conditional("LOG")]
        public static void WriteLine(object o, string text)
        {
#if LOG
            _prefixes.TryGetValue(o, out var prefix);
            Console.WriteLine(prefix + text);
#endif
        }
    }
}
