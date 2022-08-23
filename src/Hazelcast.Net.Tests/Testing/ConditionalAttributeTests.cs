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
