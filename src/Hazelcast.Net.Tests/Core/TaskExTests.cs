using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class TaskExTests : ObservingTestBase
    {
        [SetUp]
        [TearDown]
        public void Reset()
        {
            AsyncContext.Reset();
        }

        [Test]
        public async Task WithNewContext()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await TaskEx.WithNewContext(() =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return Task.CompletedTask;
            });

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResult()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await TaskEx.WithNewContext(() =>
                Task.FromResult(AsyncContext.CurrentContext.Id));

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await TaskEx.WithNewContext(token =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return Task.CompletedTask;
            }, CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResultToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await TaskEx.WithNewContext(token =>
                Task.FromResult(AsyncContext.CurrentContext.Id), CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithTimeout0()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<CancellationToken, Task>) null, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout(token => null, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout(token => Task.CompletedTask, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout(token => Task.CompletedTask, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout(token => Delay(token), TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout(token => Throw(), Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelTask(), Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelOperation(), Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeout0Value()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<CancellationToken, ValueTask>) null, TimeSpan.Zero, -1));

            await TaskEx.WithTimeout(token => new ValueTask(), TimeSpan.Zero, -1);
            await TaskEx.WithTimeout(token => new ValueTask(), TimeSpan.Zero, 60_000);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout(token => DelayValue(token), TimeSpan.Zero, 1);
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout(token => ThrowValue(), TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelValueTask(), TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelValueOperation(), TimeSpan.Zero, -1);
            });
        }

        [Test]
        public async Task WithTimeout0ToDefault()
        {
            // TimeSpan.Zero -> default timeout ms
            // going to test it only for Task, works the same for all
            //
            // if defaultTimeoutMilliseconds is not provided then it's -1
            // we're not going to wait an infinite amount of time to be sure
            // just look at the code

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<CancellationToken, Task>) null, TimeSpan.Zero, -1));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout(token => null, TimeSpan.Zero, -1));

            await TaskEx.WithTimeout(token => Task.CompletedTask, TimeSpan.Zero, -1);
            await TaskEx.WithTimeout(token => Task.CompletedTask, TimeSpan.Zero, 60_000);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout(token => Delay(token), TimeSpan.Zero, 1);
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout(token => Throw(), TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelTask(), TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelOperation(), TimeSpan.Zero, -1);
            });
        }

        [Test]
        public async Task WithTimeout1()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, CancellationToken, Task>) null, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((i, token) => null, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, token) => Task.CompletedTask, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, token) => Task.CompletedTask, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => Delay(token), 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => Throw(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelTask(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelOperation(), 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeout1Value()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, CancellationToken, ValueTask>) default, 0, TimeSpan.Zero, -1));

            await TaskEx.WithTimeout((i, token) => new ValueTask(), 0, TimeSpan.Zero, -1);
            await TaskEx.WithTimeout((i, token) => new ValueTask(), 0, TimeSpan.Zero, 60_000);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => DelayValue(token), 0, TimeSpan.Zero, 1);
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => ThrowValue(), 0, TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelValueTask(), 0, TimeSpan.Zero, -1);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelValueOperation(), 0, TimeSpan.Zero, -1);
            });
        }

        [Test]
        public async Task WithTimeout2()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, CancellationToken, Task>)null, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((i, j, token) => null, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, token) => Task.CompletedTask, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, token) => Task.CompletedTask, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => Delay(token), 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => Throw(), 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelTask(), 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelOperation(), 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeout3()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, CancellationToken, Task>)null, 0, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((i, j, k, token) => null, 0, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, k, token) => Task.CompletedTask, 0, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, k, token) => Task.CompletedTask, 0, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => Delay(token), 0, 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => Throw(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelTask(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelOperation(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult0()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<CancellationToken, Task<int>>) null, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<CancellationToken, Task<int>>) (token => null), Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout(token => Task.FromResult(0), Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout(token => Task.FromResult(0), TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout(token => DelayResult(token), TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout(token => ThrowResult(), Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelTaskResult(), Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout(token => CancelOperationResult(), Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult1()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, CancellationToken, Task<int>>) null, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, CancellationToken, Task<int>>) ((i, token) => null), 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, token) => Task.FromResult(0), 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, token) => Task.FromResult(0), 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => DelayResult(token), 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => ThrowResult(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelTaskResult(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelOperationResult(), 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult1Value()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, CancellationToken, ValueTask<int>>) null, 0, Timeout.InfiniteTimeSpan));


            await TaskEx.WithTimeout((i, token) => new ValueTask<int>(0), 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, token) => new ValueTask<int>(0), 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => DelayValueResult(token), 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => ThrowValueResult(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelValueTaskResult(), 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, token) => CancelValueOperationResult(), 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult2()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, CancellationToken, Task<int>>) null, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, CancellationToken, Task<int>>) ((i, j, token) => null), 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, token) => Task.FromResult(0), 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, token) => Task.FromResult(0), 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => DelayResult(token), 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => ThrowResult(), 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelTaskResult(), 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelOperationResult(), 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResultCancel2()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, CancellationToken, Task<int>>)null, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, CancellationToken, Task<int>>)((i, j, token) => null), 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None));

            await TaskEx.WithTimeout((i, j, token) => Task.FromResult(0), 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            await TaskEx.WithTimeout((i, j, token) => Task.FromResult(0), 0, 0, TimeSpan.FromSeconds(60), CancellationToken.None);

            // and "to default" too
            await TaskEx.WithTimeout((i, j, token) => Task.FromResult(0), 0, 0, TimeSpan.Zero, 60_000, CancellationToken.None);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => DelayResult(token), 0, 0, TimeSpan.FromMilliseconds(1), CancellationToken.None);
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => ThrowResult(), 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelTaskResult(), 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => CancelOperationResult(), 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            var cancellation = new CancellationTokenSource(1);
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, token) => DelayResult(token), 0, 0, Timeout.InfiniteTimeSpan, cancellation.Token);
            });
        }

        [Test]
        public async Task WithTimeoutResult3()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, CancellationToken, Task<int>>) null, 0, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, CancellationToken, Task<int>>) ((i, j, k, token) => null), 0, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, k, token) => Task.FromResult(0), 0, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, k, token) => Task.FromResult(0), 0, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => DelayResult(token), 0, 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => ThrowResult(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelTaskResult(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelOperationResult(), 0, 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResultCancel3()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, CancellationToken, Task<int>>) null, 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, CancellationToken, Task<int>>) ((i, j, k, token) => null), 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None));

            await TaskEx.WithTimeout((i, j, k, token) => Task.FromResult(0), 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            await TaskEx.WithTimeout((i, j, k, token) => Task.FromResult(0), 0, 0, 0, TimeSpan.FromSeconds(60), CancellationToken.None);

            // and "to default" too
            await TaskEx.WithTimeout((i, j, k, token) => Task.FromResult(0), 0, 0, 0, TimeSpan.Zero, 60_000, CancellationToken.None);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => DelayResult(token), 0, 0, 0, TimeSpan.FromMilliseconds(1), CancellationToken.None);
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => ThrowResult(), 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelTaskResult(), 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => CancelOperationResult(), 0, 0, 0, Timeout.InfiniteTimeSpan, CancellationToken.None);
            });

            var cancellation = new CancellationTokenSource(1);
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, token) => DelayResult(token), 0, 0, 0, Timeout.InfiniteTimeSpan, cancellation.Token);
            });
        }

        [Test]
        public async Task WithTimeoutResult4()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func < int, int, int, int, CancellationToken, Task<int>>) null, 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, int, CancellationToken, Task<int>>) ((i, j, k, l, token) => null), 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, k, l, token) => Task.FromResult(0), 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, k, l, token) => Task.FromResult(0), 0, 0, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, token) => DelayResult(token), 0, 0, 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, token) => ThrowResult(), 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, token) => CancelTaskResult(), 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, token) => CancelOperationResult(), 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult5()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, int, int, CancellationToken, Task<int>>) null, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, int, int, CancellationToken, Task<int>>)((i, j, k, l, m, token) => null), 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, k, l, m, token) => Task.FromResult(0), 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, k, l, m, token) => Task.FromResult(0), 0, 0, 0, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, token) => DelayResult(token), 0, 0, 0, 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, token) => ThrowResult(), 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, token) => CancelTaskResult(), 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, token) => CancelOperationResult(), 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutResult6()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, int, int, int, CancellationToken, Task<int>>) null, 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await TaskEx.WithTimeout((Func<int, int, int, int, int, int, CancellationToken, Task<int>>) ((i, j, k, l, m, n, token) => null), 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan));

            await TaskEx.WithTimeout((i, j, k, l, m, n, token) => Task.FromResult(0), 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            await TaskEx.WithTimeout((i, j, k, l, m, n, token) => Task.FromResult(0), 0, 0, 0, 0, 0, 0, TimeSpan.FromSeconds(60));

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, n, token) => DelayResult(token), 0, 0, 0, 0, 0, 0, TimeSpan.FromMilliseconds(1));
            });

            Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, n, token) => ThrowResult(), 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, n, token) => CancelTaskResult(), 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.WithTimeout((i, j, k, l, m, n, token) => CancelOperationResult(), 0, 0, 0, 0, 0, 0, Timeout.InfiniteTimeSpan);
            });
        }

        private static Task Delay(CancellationToken cancellationToken)
        {
            return Task.Delay(4_000, cancellationToken);
        }

        private static ValueTask DelayValue(CancellationToken cancellationToken)
        {
            return new ValueTask(Task.Delay(4_000, cancellationToken));
        }

        private static async Task<int> DelayResult(CancellationToken cancellationToken)
        {
            await Task.Delay(4_000, cancellationToken);
            return 0;
        }

        private static async ValueTask<int> DelayValueResult(CancellationToken cancellationToken)
        {
            await Task.Delay(4_000, cancellationToken);
            return 0;
        }

        private static Task Throw()
        {
            return Task.FromException(new NotSupportedException("bang"));
        }

        private static ValueTask ThrowValue()
        {
            return new ValueTask(Task.FromException(new NotSupportedException("bang")));
        }

        private static Task<int> ThrowResult()
        {
            return Task.FromException<int>(new NotSupportedException("bang"));
        }

        private static ValueTask<int> ThrowValueResult()
        {
            return new ValueTask<int>(Task.FromException<int>(new NotSupportedException("bang")));Task.FromException<int>(new NotSupportedException("bang"));
        }

        private static Task CancelTask()
        {
            return Task.FromCanceled(new CancellationToken(true));
        }

        private static ValueTask CancelValueTask()
        {
            return new ValueTask(Task.FromCanceled(new CancellationToken(true)));
        }

        private static Task<int> CancelTaskResult()
        {
            return Task.FromCanceled<int>(new CancellationToken(true));
        }

        private static ValueTask<int> CancelValueTaskResult()
        {
            return new ValueTask<int>(Task.FromCanceled<int>(new CancellationToken(true)));
        }

        private static Task CancelOperation()
        {
            return Task.FromException(new OperationCanceledException("cancel"));
        }

        private static ValueTask CancelValueOperation()
        {
            return new ValueTask(Task.FromException(new OperationCanceledException("cancel")));
        }

        private static Task<int> CancelOperationResult()
        {
            return Task.FromException<int>(new OperationCanceledException("cancel"));
        }

        private static ValueTask<int> CancelValueOperationResult()
        {
            return new ValueTask<int>(Task.FromException<int>(new OperationCanceledException("cancel")));
        }
    }
}
