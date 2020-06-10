using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]

    public class ConcurrentAsyncDictionaryTests
    {
        [Test]
        public async Task Test()
        {
            var d = new ConcurrentAsyncDictionary<string, int>();
            var i = 0;

            ValueTask<int> Factory(string key)
            {
                i++;
                return new ValueTask<int>(2);
            }

            var v = await d.GetOrAddAsync("a", Factory);

            Assert.AreEqual(2, v);
            Assert.AreEqual(1, i);

            v = await d.GetOrAddAsync("a", Factory);
            Assert.AreEqual(2, v);
            Assert.AreEqual(1, i);

            var entries = new List<string>();
            await foreach (var (key, value) in d)
            {
                entries.Add($"{key}:{value}");
            }

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("a:2", entries[0]);

            var (hasValue, gotValue) = await d.TryGetValue("a");
            Assert.IsTrue(hasValue);
            Assert.AreEqual(2, gotValue);

            Assert.IsTrue(d.TryRemove("a"));
            Assert.IsFalse(d.TryRemove("a"));

            (hasValue, _) = await d.TryGetValue("a");
            Assert.IsFalse(hasValue);

            static ValueTask<int> DeadlyFactory(string key)
            {
                throw new InvalidOperationException("bang");
            }

            // this does not throw
            var task = d.GetOrAddAsync("a", DeadlyFactory);

            // here it throws
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                v = await task;
            });

            (hasValue, _) = await d.TryGetValue("a");
            Assert.IsFalse(hasValue);

            static async ValueTask<int> AsyncDeadlyFactory(string key)
            {
                await Task.Yield();
                throw new InvalidOperationException("bang");
            }

            // this does not throw
            task = d.GetOrAddAsync("a", AsyncDeadlyFactory);

            // here it throws
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await task;
            });

            (hasValue, _) = await d.TryGetValue("a");
            Assert.IsFalse(hasValue);

            Assert.IsTrue(d.TryAdd("a", _ => new ValueTask<int>(2)));
            Assert.IsTrue(d.TryAdd("b", _ => new ValueTask<int>(3)));
            Assert.IsTrue(d.TryAdd("c", _ => new ValueTask<int>(4)));

            Assert.IsFalse(d.TryAdd("c", _ => new ValueTask<int>(4)));

            entries = new List<string>();
            await foreach (var (key, value) in d)
            {
                entries.Add($"{key}:{value}");
            }

            Assert.AreEqual(3, entries.Count);
            Assert.Contains("a:2", entries);
            Assert.Contains("b:3", entries);
            Assert.Contains("c:4", entries);
        }
    }
}
