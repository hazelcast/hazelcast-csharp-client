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
using Hazelcast.Core;

namespace Hazelcast.Tests.Sandbox
{
    // consider SyncEvent<TSender, TEventArgs> ?

    /// <summary>
    /// Represents a synchronous event handlers.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public sealed class SyncEvent<TEventArgs>
    {
        // implementation notes
        //
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

        private readonly List<Action<TEventArgs>> _methods = new List<Action<TEventArgs>>();

        /// <summary>
        /// Adds an event handling method.
        /// </summary>
        /// <param name="method">The event handling method.</param>
        public void Add(Action<TEventArgs> method)
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
        public static SyncEventHandler<TEventArgs> operator +(SyncEventHandler<TEventArgs> handler, Action<TEventArgs> method)
        {
            handler = handler ?? new SyncEventHandler<TEventArgs>();
            handler.Add(method);
            return handler;
        }
        */

        /// <summary>
        /// Removes a method.
        /// </summary>
        /// <param name="method">The method.</param>
        public void Remove(Action<TEventArgs> method)
        {
            lock (_methods) _methods.Remove(method ?? throw new ArgumentNullException(nameof(method)));
        }

        /*
        /// <summary>
        /// Removes an event handling method.
        /// </summary>
        /// <param name="handler">The original event handler.</param>
        /// <param name="method">The event handling method.</param>
        /// <returns>The original event handler, with the specified event handling method removed.</returns>
        public static SyncEventHandler<TEventArgs> operator -(SyncEventHandler<TEventArgs> handler, Action<TEventArgs> method)
        {
            handler?.Remove(method);
            return handler;
        }
        */

        /// <summary>
        /// Clears all method.
        /// </summary>
        public void Clear()
        {
            lock (_methods) _methods.Clear();
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public void Invoke(TEventArgs args)
        {
            List<Action<TEventArgs>> snapshot;
            lock (_methods) snapshot = new List<Action<TEventArgs>>(_methods);

            foreach (var method in snapshot)
            {
                try
                {
                    method(args);
                }
                catch (Exception e)
                {
                    // we cannot let one handler kill everything,
                    // so are we going to swallow the exception?
                    HzConsole.WriteLine(this, e);
                }
            }
        }
    }
}
