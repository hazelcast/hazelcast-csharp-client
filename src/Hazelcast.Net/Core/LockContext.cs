// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a lock context.
    /// </summary>
    /// <remarks>
    /// <para>In .NET, the <c>lock</c> statement is thread-bound i.e. the underlying <see cref="Monitor"/>
    /// is entered by the thread, and can be entered by only one thread at a time, and must be exited by
    /// that same thread. However, in an async flow, a <see cref="Task"/> can be executed by different
    /// threads (each time <c>await</c> is used, the <see cref="Task"/> can resume execution on any
    /// available thread). For this reason, the <c>lock</c> statement is *not* supported in async flows
    /// (the compiler raises an error) and explicitly implementing the pattern with an underlying
    /// <see cref="Monitor"/> cannot work, as we cannot guarantee that all code between entering and exiting
    /// the monitor executes on the same thread.</para>
    /// <para>The Hazelcast cluster locks (could be Map locks or FencedLock locks) are owned by a "context"
    /// which is represented by a <c>long</c> (64-bits integer) identifier. At codec level, this identifier
    /// is passed by the client to the cluster, for all operations that involve locks. In other words,
    /// locks in the Hazelcast cluster are context-bound and that context is represented by a <c>long</c>
    /// identifier.</para>
    /// <para>The Java client uses the Java thread unique identifier as the context identifier for the
    /// purpose of locks. This has the benefit of being transparent, i.e. all operations naturally "know"
    /// the context identifier in which they are operating. The drawback is that, just as with the
    /// <c>lock</c> statement, this cannot be supported by .NET async flows.</para>
    /// <para>The <see cref="LockContext"/> class provides an explicit representation of such a context,
    /// that can be passed to every context-dependent methods (such as locking a fenced lock) to indicate
    /// that they operate within that given context. Each instance of the class is assigned a unique
    /// <c>long</c> identifier produced by an atomic sequence, which is used as a context (or "thread")
    /// identifier at codec and cluster level, for all locking purposes.
    /// </para>
    /// </remarks>
    public sealed class LockContext
    {
        // the sequence of unique identifiers for contexts
        private static ISequence<long> _idSequence = new Int64Sequence();

        /// <summary>
        /// Initializes a new instance of the <see cref="LockContext"/> class.
        /// </summary>
        public LockContext()
        {
            // assign the unique identifier using the sequence
            Id = _idSequence.GetNext();
        }

        // TODO: would this be a good idea? and then what about identifier collisions?
        //public static LockContext FromAsyncContext() => new LockContext(AsyncContext.Current.Id);

        /// <summary>
        /// Gets the unique identifier for of this context.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// (internal for tests only) Resets the sequence of unique identifiers.
        /// </summary>
        internal static void ResetSequence()
        {
            _idSequence = new Int64Sequence();
        }
    }
}
