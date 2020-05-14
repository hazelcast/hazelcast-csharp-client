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
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    internal class BufferPoolThreadLocal : IDisposable
    {
        /// <summary>
        /// Thread local has a finalizer and is properly disposable. Once the thread local is disposed,
        /// it removes itself from the buckets of the ThreadStatic field.
        /// </summary>
        private readonly ThreadLocal<BufferPool> _threadLocal;

        public BufferPoolThreadLocal(ISerializationService serializationService)
        {
            _threadLocal = new ThreadLocal<BufferPool>(() => new BufferPool(serializationService), false);
        }

        public BufferPool Get()
        {
            try
            {
                return _threadLocal.Value;
            }
            catch (ObjectDisposedException)
            {
                throw new HazelcastClientNotActiveException();
            }
        }

        public void Dispose()
        {
            _threadLocal.Dispose();
        }
    }
}