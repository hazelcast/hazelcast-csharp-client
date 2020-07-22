// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Tests.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class HConsoleTests
    {
        private static string Prefix(string prefix = null) => $"[{Thread.CurrentThread.ManagedThreadId:00}] {prefix}: ";

        [SetUp]
        [TearDown]
        public void Reset()
        {
            HConsole.Reset();
        }

#if HZ_CONSOLE

        [Test]
        public void ConfigureToString()
        {
            HConsole.Configure<object>(x => x
                .SetIndent(4)
                .SetMaxLevel(3));

            var o = new object();

            HConsole.Configure(o, x => x
                .SetPrefix("XX")
                .SetMaxLevel(4));

            Assert.That(HConsole.GetConfig(o).ToString(), Is.EqualTo("{Config: 4, \"XX\", 4}"));
        }

        [Test]
        public void Configure()
        {
            HConsole.Configure<object>(x => x
                .SetIndent(4)
                .SetPrefix("XX")
                .SetMaxLevel(3));

            var config = HConsole.GetConfig(new object());

            Assert.That(config.Prefix, Is.EqualTo("XX"));
            Assert.That(config.Indent, Is.EqualTo(4));
            Assert.That(config.MaxLevel, Is.EqualTo(3));

            HConsole.Configure<object>(x => x
                .ClearIndent()
                .ClearPrefix()
                .ClearMaxLevel());

            config = HConsole.GetConfig(new object());

            Assert.That(config.Prefix, Is.Null);
            Assert.That(config.Indent, Is.EqualTo(0));
            Assert.That(config.MaxLevel, Is.EqualTo(-1));

            HConsole.ClearConfiguration<object>();

            var o = new object();

            HConsole.Configure(o, x => x
                .SetIndent(4)
                .SetPrefix("XX")
                .SetMaxLevel(3));

            config = HConsole.GetConfig(o);

            Assert.That(config.Prefix, Is.EqualTo("XX"));
            Assert.That(config.Indent, Is.EqualTo(4));
            Assert.That(config.MaxLevel, Is.EqualTo(3));

            HConsole.ClearConfiguration(o);

            config = HConsole.GetConfig(o);

            Assert.That(config.Prefix, Is.Null);
            Assert.That(config.Indent, Is.EqualTo(0));
            Assert.That(config.MaxLevel, Is.EqualTo(-1));
        }

        [Test]
        public void MergeConfiguration()
        {
            var config1 = new HConsoleConfiguration()
                .SetIndent(3);

            var config2 = new HConsoleConfiguration()
                .SetPrefix("XX");

            var merged = config1.Merge(config2);

            Assert.That(merged.Indent, Is.EqualTo(3));
            Assert.That(merged.Prefix, Is.EqualTo("XX"));

            Assert.Throws<ArgumentNullException>(() => _ = config1.Merge(null));
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => HConsole.WriteLine(null, "text"));
            Assert.Throws<ArgumentNullException>(() => HConsole.WriteLine(null, 1));
            Assert.Throws<ArgumentNullException>(() => HConsole.WriteLine(null, 0, "text"));
            Assert.Throws<ArgumentNullException>(() => HConsole.WriteLine(null, 0, "{0}", 0));
            Assert.Throws<ArgumentNullException>(() => HConsole.WriteLine(null, 0, 1));
            Assert.Throws<ArgumentNullException>(() => HConsole.Configure(new object(), null));
            Assert.Throws<ArgumentNullException>(() => HConsole.Configure<object>(null));

            Assert.Throws<ArgumentNullException>(() => HConsole.Lines(null, 0, ""));
        }

        [Test]
        public void Lines()
        {
            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(0));

            const string source = @"aaa
bbb
ccc";

            Assert.That(HConsole.Lines(o, 1, source), Is.EqualTo(""));

            Assert.That(HConsole.Lines(o, 0, source).ToLf(), Is.EqualTo($@"
       aaa
       bbb
       ccc".ToLf()));

            HConsole.Configure<object>(config => config.SetPrefix("XX"));

            Assert.That(HConsole.Lines(o, 0, source).ToLf(), Is.EqualTo($@"
         aaa
         bbb
         ccc".ToLf()));
        }

        [Test]
        public void WritesWithPrefix()
        {
            HConsole.Configure<object>(config => config.SetPrefix("XX"));

            var capture = new ConsoleCapture();

            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(0));

            using (capture.Output())
            {
                HConsole.WriteLine(o, "text0");
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo($"{Prefix("XX")}text0\n".ToLf()));
        }

        [Test]
        public void WritesOverloads()
        {
            var capture = new ConsoleCapture();

            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(0));

            using (capture.Output())
            {
                HConsole.WriteLine(o, "text0");
                HConsole.WriteLine(o, 0, "text1");
                HConsole.WriteLine(o, 2);
                HConsole.WriteLine(o, 0, 3);
                HConsole.WriteLine(o, "-{0}-", 4);
                HConsole.WriteLine(o, 0, "-{0}-", 5);

                HConsole.WriteLine(o, 1, "text1");
                HConsole.WriteLine(o, 1, 3);
                HConsole.WriteLine(o, 1, "-{0}-", 5);
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo($@"{Prefix()}text0
{Prefix()}text1
{Prefix()}2
{Prefix()}3
{Prefix()}-4-
{Prefix()}-5-
".ToLf()));
        }

        [Test]
        public void WritesNothingByDefault()
        {
            var capture = new ConsoleCapture();

            var o = new object();

            using (capture.Output())
            {
                HConsole.WriteLine(o, "text0"); // default level is 0
                HConsole.WriteLine(o, 1, "text1");
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo("".ToLf()));
        }

        [Test]
        public void WritesLevelZeroIfConfigured()
        {
            var capture = new ConsoleCapture();

            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(0));

            using (capture.Output())
            {
                HConsole.WriteLine(o, "text0"); // default level is 0
                HConsole.WriteLine(o, 1, "text1");
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo($"{Prefix()}text0\n".ToLf()));
        }

        [Test]
        public void WritesOtherLevelsIfConfigured()
        {
            var capture = new ConsoleCapture();

            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(1));

            using (capture.Output())
            {
                HConsole.WriteLine(o, "text0"); // default level is 0
                HConsole.WriteLine(o, 1, "text1");
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo($"{Prefix()}text0\n{Prefix()}text1\n".ToLf()));
        }

        [Test]
        public void CanResetConfiguration()
        {
            var capture = new ConsoleCapture();

            var o = new object();
            HConsole.Configure(o, config => config.SetMaxLevel(1));

            using (capture.Output())
            {
                HConsole.WriteLine(o, 1, "text1");
            }

            Assert.That(capture.ReadToEnd().ToLf(), Is.EqualTo($"{Prefix()}text1\n".ToLf()));

            HConsole.Reset();

            using (capture.Output())
            {
                HConsole.WriteLine(o, 1, "text0");
            }

            Assert.That(capture.ReadToEnd(), Is.EqualTo(""));
        }

#endif
    }
}
