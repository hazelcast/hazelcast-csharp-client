// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Tests.Sandbox
{
    // consider AsyncEvent<TSender, TEventArgs> ?

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public sealed class AsyncEvent<TEventArgs>
    {
        // implementation notes
        // this is using a locked list - simple enough and fast - other concurrent structures
        // are way heavier, and also take shallow snapshots before enumerating - so before
        // replacing this with more a complex solution, benchmark to be sure it's actually
        // faster - also, using a list sort-of ensures a deterministic order of events, and
        // even if this should not be relied upon, it may make things cleaner
        //
        // one may be tempted to uncomment the += and -= operators (left here for reference)
        // to reproduce the native C# events syntax - however, that syntax is by nature not
        // thread safe, even with magic in the event add/remove methods, and so please don't.
        // recommended reading: https://blog.stephencleary.com/2009/06/threadsafe-events.html

        private readonly List<Func<TEventArgs, ValueTask>> _methods = new List<Func<TEventArgs, ValueTask>>();

        /// <summary>
        /// Adds a event handling method.
        /// </summary>
        /// <param name="method">The event handling method.</param>
        public void Add(Func<TEventArgs, ValueTask> method)
        {
            lock (_methods) _methods.Add(method ?? throw new ArgumentNullException(nameof(method)));
        }

        /*
        /// <summary>
        /// Adds an event handling method.
        /// </summary>
        /// <param name="handler">The original event handler.</param>
        /// <param name="method">The event handling method.</param>
        /// <returns>The original event handler, with the specified event handling method added.</returns>
        public static AsyncEventHandler<TEventArgs> operator +(AsyncEventHandler<TEventArgs> handler, Func<TEventArgs, ValueTask> method)
        {
            handler = handler ?? new AsyncEventHandler<TEventArgs>();
            handler.Add(method);
            return handler;
        }
        */

        /// <summary>
        /// Removes an event handling method.
        /// </summary>
        /// <param name="func">The event handling method.</param>
        public void Remove(Func<TEventArgs, ValueTask> func)
        {
            lock (_methods) _methods.Remove(func ?? throw new ArgumentNullException(nameof(func)));
        }

        /*
        /// <summary>
        /// Removes an event handling method.
        /// </summary>
        /// <param name="handler">The original event handler.</param>
        /// <param name="method">The event handling method.</param>
        /// <returns>The original event handler, with the specified event handling method removed.</returns>
        public static AsyncEventHandler<TEventArgs> operator -(AsyncEventHandler<TEventArgs> handler, Func<TEventArgs, ValueTask> method)
        {
            handler?.Remove(method);
            return handler;
        }
        */

        /// <summary>
        /// Invokes the methods.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        /// <returns>A task that will complete when all methods have completed.</returns>
        public async ValueTask InvokeAsync(TEventArgs args)
        {
            List<Func<TEventArgs, ValueTask>> snapshot;
            lock (_methods) snapshot = new List<Func<TEventArgs, ValueTask>>(_methods);

            foreach (var method in snapshot)
            {
                try
                {
                    await method(args).CfAwait();
                }
                catch (Exception e)
                {
                    // we cannot let one handler kill everything,
                    // so are we going to swallow the exception?
                    HConsole.WriteLine(this, e);
                }
            }
        }
    }
}
