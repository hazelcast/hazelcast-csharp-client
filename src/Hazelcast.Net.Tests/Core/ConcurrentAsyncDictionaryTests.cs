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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    [Timeout(20_000)]

    public class ConcurrentAsyncDictionaryTests
    {
        [Test]
        public async Task Test()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<int>>();
            var i = 0;

            ValueTask<ValueItem<int>> Factory(string key, CancellationToken _)
            {
                i++;
                return new ValueTask<ValueItem<int>>(new ValueItem<int>(2));
            }

            var v = await d.GetOrAddAsync("a", Factory);

            Assert.AreEqual(2, v.Value);
            Assert.AreEqual(1, i);

            v = await d.GetOrAddAsync("a", Factory);
            Assert.AreEqual(2, v.Value);
            Assert.AreEqual(1, i);

            var entries = new List<string>();
            await foreach (var (key, value) in d)
            {
                entries.Add($"{key}:{value}");
            }

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("a:2", entries[0]);

            var (hasValue, gotValue) = await d.TryGetAsync("a");
            Assert.IsTrue(hasValue);
            Assert.AreEqual(2, gotValue.Value);

            Assert.IsTrue(d.TryRemove("a"));
            Assert.IsFalse(d.TryRemove("a"));

            (hasValue, _) = await d.TryGetAsync("a");
            Assert.IsFalse(hasValue);

            static ValueTask<ValueItem<int>> DeadlyFactory(string key, CancellationToken _)
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

            (hasValue, _) = await d.TryGetAsync("a");
            Assert.IsFalse(hasValue);

            static async ValueTask<ValueItem<int>> AsyncDeadlyFactory(string key, CancellationToken _)
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

            (hasValue, _) = await d.TryGetAsync("a");
            Assert.IsFalse(hasValue);

            await d.GetOrAddAsync("a", (_, __) => new ValueTask<ValueItem<int>>(new ValueItem<int>(2)));
            await d.GetOrAddAsync("b", (_, __) => new ValueTask<ValueItem<int>>(new ValueItem<int>(3)));
            await d.GetOrAddAsync("c", (_, __) => new ValueTask<ValueItem<int>>(new ValueItem<int>(4)));

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

        [Test]
        public async Task Enumerate()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>("value_" + key.Substring("key_".Length));
                return new ValueTask<ValueItem<string>>(v);
            }

            // add a number of (key_N,value_N) pairs
            for (var i = 0; i < 20; i++)
            {
                var attempt = await d.TryAddAsync("key_" + i, CreateValueItem);
                Assert.That(attempt, Is.True);
                Assert.That(created, Is.EqualTo(i + 1));
            }

            var x = 0;

            // async-enumerate the entries, and while enumerating
            // - add a (doh_N,) entry
            // - remove entry for key_15
            var entries = new List<string>();
            await foreach (var (key, value) in d)
            {
                if (x++ == 3)
                {
                    var attempt = await d.TryAddAsync("key_99", CreateValueItem);
                    Assert.That(attempt, Is.True);
                    Assert.That(created, Is.EqualTo(21));
                }

                if (x++ == 7)
                {
                    Assert.That(d.TryRemove("key_15"), Is.True);
                }

                entries.Add($"({key},{value})");
            }

            // "The contents exposed through the enumerator may contain modifications made
            // to the dictionary after GetEnumerator was called." enumerating is safe,
            // but you get... "stuff"

            // whether key_15 and key_99 have been enumerated is implementation- and
            // timing-dependent, we cannot assume anything, however we:
            // - must have enumerated at least 19 entries (original 20 minus key_15)
            // - must have enumerated at max 21 entries (original 20 plus key_99)
            Assert.That(entries.Count, Is.GreaterThanOrEqualTo(19));
            Assert.That(entries.Count, Is.LessThanOrEqualTo(21));

            for (var i = 0; i < 20; i++)
            {
                if (i == 15) continue; // that one has been removed
                Assert.That(entries.Contains($"(key_{i},value_{i})"), Is.True, $"Missing key_{i}");
            }
        }

        [Test]
        public async Task EnumerateBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev = new ManualResetEventSlim();

            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                ev.Wait();
                throw new Exception("bogus");
            }

            var addTask = d.TryAddAsync("key", CreateValueItem);

            var enumerator = d.GetAsyncEnumerator();

            var nextTask = enumerator.MoveNextAsync();

            await Task.Delay(200);

            Assert.That(nextTask.IsCompleted, Is.False);

            ev.Set();

            try
            {
                await addTask;
                Assert.Fail("Expected an exception.");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("bogus"));
            }

            // key is gone - because it was bogus when enumerating

            Assert.That(await nextTask, Is.False);
        }

        [Test]
        public async Task TryAddConcurrentBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev = new ManualResetEventSlim();

            var created = 0;
            var exception = 0;

            // beware, *must* be an async method to throw in the task, not when creating it
            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                created++;
                ev.Wait();
                throw new Exception("bogus");
            }

            var task1 = Task.Run(async () =>
            {
                try
                {
                    var added = await d.TryAddAsync("key", CreateValueItem);
                    if (added)
                        Assert.Fail("Expected an exception.");
                }
                catch (Exception e)
                {
                    exception++;
                    Assert.That(e.Message, Is.EqualTo("bogus"));
                }
            });

            var task2 = Task.Run(async () =>
            {
                try
                {
                    var added = await d.TryAddAsync("key", CreateValueItem);
                    if (added)
                        Assert.Fail("Expected an exception.");
                }
                catch (Exception e)
                {
                    exception++;
                    Assert.That(e.Message, Is.EqualTo("bogus"));
                }
            });

            await Task.Delay(200);
            Assert.That(created, Is.EqualTo(1));

            ev.Set();

            // the one that created the item received the corresponding exception
            // the other one received nothing because it did not add anything

            await Task.WhenAll(task1, task2);
            Assert.That(created, Is.EqualTo(1));
            Assert.That(exception, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrAddConcurrentBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev = new ManualResetEventSlim();

            var created = 0;
            var exception = 0;

            // beware, *must* be an async method to throw in the task, not when creating it
            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                created++;
                ev.Wait();
                throw new Exception("bogus");
            }

            var task1 = Task.Run(async () =>
            {
                try
                {
                    var value = await d.GetOrAddAsync("key", CreateValueItem);
                    Assert.Fail("Expected an exception.");
                }
                catch (Exception e)
                {
                    exception++;
                    Assert.That(e.Message, Is.EqualTo("bogus"));
                }
            });

            var task2 = Task.Run(async () =>
            {
                try
                {
                    var value = await d.GetOrAddAsync("key", CreateValueItem);
                    Assert.Fail("Expected an exception.");
                }
                catch (Exception e)
                {
                    exception++;
                    Assert.That(e.Message, Is.EqualTo("bogus"));
                }
            });

            await Task.Delay(1000);
            Assert.That(created, Is.EqualTo(1));

            ev.Set();

            // the one that created the item received the corresponding exception
            // the other one also received the exception because ?

            await Task.WhenAll(task1, task2);
            Assert.That(created, Is.EqualTo(1));
            Assert.That(exception, Is.EqualTo(2));
        }

        [Test]
        public async Task TryAddBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            static ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                throw new Exception("bogus");
            }

            try
            {
                await d.TryAddAsync("key", CreateValueItem);
                Assert.Fail("Expected an exception.");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("bogus"));
            }
        }

        [Test]
        public async Task TryAddNull()
        {
            var d = new ConcurrentAsyncDictionary<string, string>();

            static ValueTask<string> CreateValueItem(string key, CancellationToken _)
            {
                return new ValueTask<string>((string) null);
            }

            await d.TryAddAsync("key", CreateValueItem);

            var attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.False);
        }

        [Test]
        public async Task GetOrAddBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            static ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                throw new Exception("bogus");
            }

            try
            {
                await d.GetOrAddAsync("key", CreateValueItem);
                Assert.Fail("Expected an exception.");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("bogus"));
            }
        }

        [Test]
        public async Task GetOrAddNull()
        {
            var d = new ConcurrentAsyncDictionary<string, string>();

            static ValueTask<string> CreateValueItem(string key, CancellationToken _)
            {
                return new ValueTask<string>((string) null);
            }

            var value = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(value, Is.Null);

            var attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.False);
        }

        [Test]
        public async Task TryAdd()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var attempt = await d.TryAddAsync("key", CreateValueItem);
            Assert.That(attempt, Is.True);
            Assert.That(created, Is.EqualTo(1));

            var added = await d.TryGetAsync("key");
            Assert.That(added.Success, Is.True);
            Assert.That(added.Value, Is.SameAs(value));

            attempt = await d.TryAddAsync("key", CreateValueItem);
            Assert.That(attempt, Is.False);
            Assert.That(created, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrAdd()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));

            added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));
        }

        [Test]
        public async Task TryRemove()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));

            var attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Value, Is.SameAs(value));

            Assert.That(d.TryRemove("key"), Is.True);
            Assert.That(d.TryRemove("key"), Is.False);

            attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.False);

            added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.Not.SameAs(value));
            Assert.That(created, Is.EqualTo(2));
        }

        [Test]
        public async Task TryGetAndRemove()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));

            var attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Value, Is.SameAs(value));

            attempt = await d.TryGetAndRemoveAsync("key");
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Value, Is.SameAs(value));

            attempt = await d.TryGetAndRemoveAsync("key");
            Assert.That(attempt.Success, Is.False);

            attempt = await d.TryGetAsync("key");
            Assert.That(attempt.Success, Is.False);

            added = await d.GetOrAddAsync("key", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.Not.SameAs(value));
            Assert.That(created, Is.EqualTo(2));
        }

        [Test]
        public async Task TryGetAndRemoveBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev = new ManualResetEventSlim();

            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                ev.Wait();
                throw new Exception("bogus");
            }

            var task1 = d.GetOrAddAsync("key", CreateValueItem);
            var task2 = d.TryGetAndRemoveAsync("key");

            await Task.Delay(200);

            Assert.That(task2.IsCompleted, Is.False);

            ev.Set();

            try
            {
                await task1;
                Assert.Fail("Expected an exception.");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("bogus"));
            }

            var removed = await task2;
            Assert.That(removed.Success, Is.False);
        }

        [Test]
        public async Task CountAndClear()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var added = await d.GetOrAddAsync("key1", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));

            added = await d.GetOrAddAsync("key2", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.Not.SameAs(value));
            Assert.That(created, Is.EqualTo(2));

            added = await d.GetOrAddAsync("key3", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(created, Is.EqualTo(3));

            Assert.That(d.Count, Is.EqualTo(3));

            added = await d.GetOrAddAsync("key4", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(created, Is.EqualTo(4));

            Assert.That(d.Count, Is.EqualTo(4));

            Assert.That(d.TryRemove("key1"), Is.True);

            Assert.That(d.Count, Is.EqualTo(3));

            d.Clear();

            Assert.That(d.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ContainsKey()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var created = 0;
            ValueItem<string> value = null;

            ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                created += 1;
                var v = new ValueItem<string>();
                value ??= v;
                return new ValueTask<ValueItem<string>>(v);
            }

            var added = await d.GetOrAddAsync("key1", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.SameAs(value));
            Assert.That(created, Is.EqualTo(1));

            added = await d.GetOrAddAsync("key2", CreateValueItem);
            Assert.That(added, Is.Not.Null);
            Assert.That(added, Is.Not.SameAs(value));
            Assert.That(created, Is.EqualTo(2));

            Assert.That(await d.ContainsKeyAsync("key1"), Is.True);
            Assert.That(await d.ContainsKeyAsync("key2"), Is.True);
            Assert.That(await d.ContainsKeyAsync("key3"), Is.False);
        }

        [Test]
        public async Task ContainsKey2()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev = new ManualResetEventSlim();

            // beware, makes sure it's an async method that waits and throws when the
            // task runs, not when creating it!
            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                ev.Wait();
                return new ValueItem<string>();
            }

            var task1 = d.GetOrAddAsync("key", CreateValueItem);
            var task2 = d.ContainsKeyAsync("key");

            await Task.Delay(200);

            Assert.That(task2.IsCompleted, Is.False);

            ev.Set();

            // contains key will wait for the value
            var contains = await task2;
            Assert.That(contains, Is.True);

            await task1;
        }

        [Test]
        public async Task ContainsKeyBogus()
        {
            var d = new ConcurrentAsyncDictionary<string, ValueItem<string>>();

            var ev1 = new ManualResetEventSlim();
            var ev2 = new ManualResetEventSlim();

            // beware, makes sure it's an async method that waits and throws when the
            // task runs, not when creating it!
            async ValueTask<ValueItem<string>> CreateValueItem(string key, CancellationToken _)
            {
                await Task.Yield();
                ev2.Set();
                ev1.Wait();
                throw new Exception("bogus");
            }

            var task1 = Task.Run(async () =>
            {
                try
                {
                    var value = await d.GetOrAddAsync("key", CreateValueItem);
                    Assert.Fail("Expected an exception.");
                }
                catch (Exception e)
                {
                    Assert.That(e.Message, Is.EqualTo("bogus"));
                }
            });

            ev2.Wait();
            var task2 = d.ContainsKeyAsync("key");

            await Task.Delay(200);

            Console.WriteLine(task2.IsCompleted);
            Assert.That(task2.IsCompleted, Is.False);

            ev1.Set();

            // contains key will wait for the value
            var contains = await task2;
            Assert.That(contains, Is.False);

            await task1;
        }

        [Test]
        public async Task EntryTests()
        {
            var entry = new ConcurrentAsyncDictionary<int, ValueItem<int>>.Entry(42);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var i = entry.Value;
            });

            var v1 = await entry.GetValue((x, _) => new ValueTask<ValueItem<int>>(new ValueItem<int>(x)));

            // will use a bit of code that is usually not used except in some race conditions
            var v2 = await entry.GetValue((x, _) => new ValueTask<ValueItem<int>>(new ValueItem<int>(x)));

            Assert.That(v1, Is.EqualTo(v2));

            var entry2 = new ConcurrentAsyncDictionary<int, string>.Entry(42, true, "hello");
            var value = await entry2.GetValue();
            Assert.That(value, Is.EqualTo("hello"));

            entry2 = new ConcurrentAsyncDictionary<int, string>.Entry(42, false, creating: Task.FromResult("hello"));
            value = await entry2.GetValue();
            Assert.That(value, Is.EqualTo("hello"));
        }

        [Test]
        public async Task TryGetEntryValueTests()
        {
            // create an entry with a value
            var entry = new ConcurrentAsyncDictionary<int, string>.Entry(42, true, "hello");

            // value is returned
            var attempt = await ConcurrentAsyncDictionary<int, string>.TryGetEntryValueAsync(entry);
            Assert.That(attempt.Success);
            Assert.That(attempt.Value, Is.EqualTo("hello"));

            // create an entry with a null value
            entry = new ConcurrentAsyncDictionary<int, string>.Entry(42, true);

            // entry with a null value is not returned
            attempt = await ConcurrentAsyncDictionary<int, string>.TryGetEntryValueAsync(entry);
            Assert.That(attempt.Success, Is.False);

            // put that entry in a dictionary
            var d = new ConcurrentAsyncDictionary<int, string>();
            d.AddEntry(42, new ConcurrentAsyncDictionary<int, string>.Entry(42, true));

            // entry with a null value is not enumerated
            var entries = new List<string>();
            await foreach (var (k, v) in d)
                entries.Add(v);

            Assert.That(entries.Count, Is.Zero);

            // create an entry which creates a null value
            entry = new ConcurrentAsyncDictionary<int, string>.Entry(42, false, creating: Task.FromResult((string) null));

            // entry with a null value is not returned
            attempt = await ConcurrentAsyncDictionary<int, string>.TryGetEntryValueAsync(entry);
            Assert.That(attempt.Success, Is.False);

            // create an entry which throws when creating its value
            static async Task<string> CreateValue()
            {
                await Task.Yield();
                throw new Exception("bang");
            }
            entry = new ConcurrentAsyncDictionary<int, string>.Entry(42, false, creating: CreateValue());

            // entry which throws is not returned
            attempt = await ConcurrentAsyncDictionary<int, string>.TryGetEntryValueAsync(entry);
            Assert.That(attempt.Success, Is.False);
        }

        private class ValueItem<T>
        {
            public ValueItem()
            { }

            public ValueItem(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public override string ToString() => Value.ToString();
        }
    }
}
