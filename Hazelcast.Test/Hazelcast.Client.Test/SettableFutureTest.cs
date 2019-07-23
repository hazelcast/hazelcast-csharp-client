﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class SettableFutureTest
    {
        [Test]
        public void TestSetResult()
        {
            var future = new SettableFuture<string>();

            Task.Factory.StartNew(() => future.Result = "done");
            Assert.AreEqual("done", future.Result);
        }

        [Test]
        public void TestGetResult()
        {
            var future = new SettableFuture<string>();

            Task.Factory.StartNew(() => future.Result = "done");
            Assert.AreEqual("done", future.GetResult(100));
        }

        [Test]
        public void TestGetResult_WhenException()
        {
            Assert.Throws<Exception>(() =>
            {
                var future = new SettableFuture<string>();

                Task.Factory.StartNew(() => future.Exception = new Exception("Failed"));
                Assert.AreEqual("done", future.GetResult(100));
            }, "Failed");
        }

        [Test]
        public void TestGetResult_WhenResultNotSet()
        {
            Assert.Throws<TimeoutException>(() =>
            {
                var future = new SettableFuture<string>();

                Assert.AreEqual("done", future.GetResult(100));
            });
        }


        [Test]
        public void TestSetException()
        {
            var future = new SettableFuture<string>();
            var exception = new Exception();
            Task.Factory.StartNew(() => future.Exception = exception);

            Assert.AreEqual(exception, future.Exception);
        }

        [Test]
        public void TestResult_WhenException()
        {
            Assert.Throws<Exception>(() =>
            {
                var future = new SettableFuture<string>();
                Task.Factory.StartNew(() => future.Exception = new Exception("Failed"));

                var result = future.Result;
            }, "Failed");
        }

        [Test]
        public void TestResult_WhenException_withContinue()
        {
            var future = new SettableFuture<string>();
            future.ToTask().ContinueWith(t => { Assert.NotNull(t.Exception); });
            Task.Factory.StartNew(() => future.Exception = new Exception("Failed"));
            try
            {
                var result = future.Result;
            }
            catch (Exception e)
            {
                Assert.NotNull(e);
                Assert.NotNull(future.Exception);
            }
        }

        [Test]
        public void TestWait()
        {
            var future = new SettableFuture<string>();

            Task.Factory.StartNew(() => future.Result = "done");
            Assert.IsTrue(future.Wait());
        }

        [Test]
        public void TestWait_WhenResultNotSet()
        {
            var future = new SettableFuture<string>();

            Assert.IsFalse(future.Wait(100));
        }

        [Test]
        public void TestSetResult_WhenResultSet()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var future = new SettableFuture<string>();
                future.Result = "done";
                future.Result = "done";
            });
        }

        [Test]
        public void TestSetException_WhenResultSet()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var future = new SettableFuture<string>();
                future.Result = "done";
                future.Exception = new Exception();
            });
        }

        [Test]
        public void TestSetResult_WhenExceptionSet()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var future = new SettableFuture<string>();
                future.Exception = new Exception();
                future.Result = "done";
            });
        }

        [Test]
        public void TestSetException_WhenExceptionSet()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var future = new SettableFuture<string>();
                future.Exception = new Exception();
                future.Exception = new Exception();
            });
        }

        [Test]
        public void TestIsComplete()
        {
            var future = new SettableFuture<String>();
            Assert.IsFalse(future.IsComplete);

            future.Result = "done";

            Assert.IsTrue(future.IsComplete);
        }

        [Test]
        public void TestToTask_WhenResultIsSet()
        {
            var future = new SettableFuture<String>();
            var task = future.ToTask();

            Task.Factory.StartNew(() => future.Result = "done");

            Assert.AreEqual("done", task.Result);
        }

        [Test]
        public void TestToTask_WhenExceptionIsSet()
        {
            Assert.Throws<Exception>(() =>
            {
                var future = new SettableFuture<String>();
                var task = future.ToTask();

                Task.Factory.StartNew(() => future.Exception = new Exception("Failed"));

                try
                {
                    var result = task.Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerExceptions.First();
                }
            }, "Failed");
        }

        [Test]
        public void Test_StressTestFuture_WhenGetResult()
        {
            var futures = Enumerable.Range(0, 100 * 1000).Select(i => new SettableFuture<string>()).ToList();

            var tasks = new List<Task>();
            foreach (var future in futures)
            {
                var future1 = future;
                Task.Factory.StartNew(() => future1.Result = "done");
                tasks.Add(Task.Factory.StartNew(() => Assert.AreEqual("done", future1.GetResult(1000))));
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Test]
        public void Test_StressTestFuture_WhenResult()
        {
            var futures = Enumerable.Range(0, 100 * 1000).Select(i => new SettableFuture<string>()).ToList();

            var tasks = new List<Task>();
            foreach (var future in futures)
            {
                var future1 = future;
                Task.Factory.StartNew(() => future1.Result = "done");
                tasks.Add(Task.Factory.StartNew(() => Assert.AreEqual("done", future1.Result)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        [Test]
        public void TestCompleted_checkResult()
        {
            var future = new SettableFuture<string>();

            Task.Factory.StartNew(() => future.Exception = new Exception("Failed"));
            try
            {
                future.GetResult(100);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Failed", e.Message);
            }

            Assert.NotNull(future.Exception);
            Assert.True(future.IsComplete);
        }

        [Test]
        public void TestCompleted_checkExceptiont()
        {
            var future = new SettableFuture<string>();

            Task.Factory.StartNew(() => future.Result = "done");
            Assert.AreEqual("done", future.Result);
            Assert.True(future.IsComplete);
            Assert.Null(future.Exception);
        }
    }
}