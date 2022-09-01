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

using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Testing;
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
            HConsole.Configure(x => x.Configure<object>()
                .SetIndent(4)
                .SetLevel(3));

            var o = new object();

            HConsole.Configure(x => x.Configure(o)
                .SetPrefix("XX")
                .SetLevel(4));

            Assert.That(HConsole.Options.GetOptions(o).ToString(), Is.EqualTo("indent = 4, prefix = \"XX\", level = 4"));
        }

        [Test]
        public void Configure()
        {
            HConsole.Configure(x => x.Configure<object>()
                .SetIndent(4)
                .SetPrefix("XX")
                .SetLevel(3));

            var config = HConsole.Options.GetOptions(new object());

            Assert.That(config.Prefix, Is.EqualTo("XX"));
            Assert.That(config.Indent, Is.EqualTo(4));
            Assert.That(config.Level, Is.EqualTo(3));

            HConsole.Configure(x => x.Configure()
                .SetIndent(33)
                .SetPrefix("YY")
                .SetLevel(44));

            config = HConsole.Options.GetOptions(new object());

            Assert.That(config.Prefix, Is.EqualTo("YY"));
            Assert.That(config.Indent, Is.EqualTo(33));
            Assert.That(config.Level, Is.EqualTo(44));

            HConsole.Configure(x => x.Configure<object>()
                .ClearIndent()
                .ClearPrefix()
                .ClearLevel());

            config = HConsole.Options.GetOptions(new object());

            Assert.That(config.Prefix, Is.Null);
            Assert.That(config.Indent, Is.EqualTo(0));
            Assert.That(config.Level, Is.EqualTo(-1));

            HConsole.Configure(x => x.Clear<object>());

            var o = new object();

            HConsole.Configure(x => x.Configure(o)
                .SetIndent(4)
                .SetPrefix("XX")
                .SetLevel(3));

            config = HConsole.Options.GetOptions(o);

            Assert.That(config.Prefix, Is.EqualTo("XX"));
            Assert.That(config.Indent, Is.EqualTo(4));
            Assert.That(config.Level, Is.EqualTo(3));

            HConsole.Configure(x => x.Clear(o));

            config = HConsole.Options.GetOptions(o);

            Assert.That(config.Prefix, Is.Null);
            Assert.That(config.Indent, Is.EqualTo(0));
            Assert.That(config.Level, Is.EqualTo(-1));

            HConsole.Reset();
            config = HConsole.Options.GetOptions(new object());

            Assert.That(config.Prefix, Is.Null);
            Assert.That(config.Indent, Is.EqualTo(0));
            Assert.That(config.Level, Is.EqualTo(-1));

            HConsole.Configure(x => x.Configure().SetMaxLevel());
            config = HConsole.Options.GetOptions(new object());
            Assert.That(config.Level, Is.EqualTo(int.MaxValue));

            HConsole.Configure(x => x.Configure().SetMinLevel());
            config = HConsole.Options.GetOptions(new object());
            Assert.That(config.Level, Is.EqualTo(-1));
        }

        [Test]
        public void MergeConfiguration()
        {
            var config1 = new HConsoleTargetOptions(null)
                .SetIndent(3);

            var config2 = new HConsoleTargetOptions(null)
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

            Assert.Throws<ArgumentNullException>(() => HConsole.Lines(null, 0, ""));
        }

        [Test]
        public void Lines()
        {
            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(0));

            const string source = @"aaa
bbb
ccc";

            Assert.That(HConsole.Lines(o, 1, source), Is.EqualTo(""));

            Assert.That(HConsole.Lines(o, 0, source).ToLf(), Is.EqualTo(@"
       aaa
       bbb
       ccc".ToLf()));

            HConsole.Configure(x => x.Configure<object>().SetPrefix("XX"));

            Assert.That(HConsole.Lines(o, 0, source).ToLf(), Is.EqualTo($@"
         aaa
         bbb
         ccc".ToLf()));
        }

        [Test]
        public void WritesWithPrefix()
        {
            HConsole.Configure(x => x.Configure<object>().SetPrefix("XX"));

            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(0));

            HConsole.WriteLine(o, "text0");

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo($"{Prefix("XX")}text0\n".ToLf()));
        }

        [Test]
        public void WritesOverloads()
        {
            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(0));

            HConsole.WriteLine(o, "text0");
            HConsole.WriteLine(o, 0, "text1");
            HConsole.WriteLine(o, 2);
            HConsole.WriteLine(o, 0, 3);
            HConsole.WriteLine(o, "-{0}-", 4);
            HConsole.WriteLine(o, 0, "-{0}-", 5);

            HConsole.WriteLine(o, 1, "text1");
            HConsole.WriteLine(o, 1, 3);
            HConsole.WriteLine(o, 1, "-{0}-", 5);

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo($@"{Prefix()}text0
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
            var o = new object();

            HConsole.WriteLine(o, "text0"); // default level is 0
            HConsole.WriteLine(o, 1, "text1");

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo("".ToLf()));
        }

        [Test]
        public void WritesLevelZeroIfConfigured()
        {
            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(0));

            HConsole.WriteLine(o, "text0"); // default level is 0
            HConsole.WriteLine(o, 1, "text1");

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo($"{Prefix()}text0\n".ToLf()));
        }

        [Test]
        public void WritesOtherLevelsIfConfigured()
        {
            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(1));

            HConsole.WriteLine(o, "text0"); // default level is 0
            HConsole.WriteLine(o, 1, "text1");

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo($"{Prefix()}text0\n{Prefix()}text1\n".ToLf()));
        }

        [Test]
        public void CanResetConfiguration()
        {
            var o = new object();
            HConsole.Configure(x => x.Configure(o).SetLevel(1));

            HConsole.WriteLine(o, 1, "text1");

            Assert.That(HConsole.Text.ToLf(), Is.EqualTo($"{Prefix()}text1\n".ToLf()));

            HConsole.Reset();

            HConsole.WriteLine(o, 1, "text0");

            Assert.That(HConsole.Text, Is.EqualTo(""));
        }

        [Test]
        public void WriteAndClear()
        {
            var capture = new ConsoleCapture();

            HConsole.WriteAndClear();
            Assert.That(HConsole.Text.Length, Is.Zero);

            HConsole.WriteAndClear();
            Assert.That(HConsole.Text.Length, Is.Zero);

            HConsole.Configure(x => x.Configure().SetMaxLevel());
            HConsole.WriteLine(this, "meh");
            Assert.That(HConsole.Text.Length, Is.GreaterThan(0));

            using (capture.Output())
            {
                HConsole.WriteAndClear();
            }

            Assert.That(HConsole.Text.Length, Is.Zero);
            Assert.That(capture.ReadToEnd(), Does.EndWith("meh" + Environment.NewLine));

            HConsole.WriteLine(this, "meh");
            Assert.That(HConsole.Text.Length, Is.GreaterThan(0));

            using (capture.Output())
            {
                HConsole.Clear();
            }

            Assert.That(HConsole.Text.Length, Is.Zero);
            Assert.That(capture.ReadToEnd().Length, Is.Zero);

            HConsole.WriteLine(this, "meh");
            using (capture.Output())
            {
                using (HConsole.Capture()) { }
            }

            Assert.That(HConsole.Text.Length, Is.Zero);
            Assert.That(capture.ReadToEnd(), Does.EndWith("meh" + Environment.NewLine));
        }

#endif
    }
}
