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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for tests that need to observe exceptions.
    /// </summary>
    public abstract class ObservingTestBase
    {
        private readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions =
            new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();

        [SetUp]
        public void ObservingTestBaseSetUp()
        {
            // make sure the queue is empty
            while (_unobservedExceptions.TryDequeue(out _))
            { }

            // handle unobserved exceptions
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        [TearDown]
        public void ObservingTestBaseTearDown()
        {
            // GC should finalize everything, thus trigger unobserved exceptions
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // check for unobserved exceptions and report
            var failed = false;
            while (_unobservedExceptions.TryDequeue(out var args))
            {
                //var innerException = args.Exception.Flatten().InnerException;
                // log?
                failed = true;
            }

            // remove handler
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;

            // fail if necessary
            if (failed) Assert.Fail("Unobserved task exceptions.");
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            ReportUnobservedException(sender, args);
            _unobservedExceptions.Enqueue(args);
        }

        protected virtual void ReportUnobservedException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Console.WriteLine($"UnobservedTaskException from {sender}.\n{args.Exception}");
        }
    }
}