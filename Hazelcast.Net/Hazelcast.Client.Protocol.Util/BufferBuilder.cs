/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using Hazelcast.Util;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>Builder for appending buffers that grows capacity as necessary.</summary>
    internal class BufferBuilder
    {
        /// <summary>Buffer's default initial capacity</summary>
        public const int InitialCapacity = 4096;

        private readonly IClientProtocolBuffer protocolBuffer;
        private int capacity;
        private int position;

        /// <summary>
        ///     Construct a buffer builder with a default growth increment of
        ///     <see cref="InitialCapacity" />
        /// </summary>
        public BufferBuilder()
            : this(InitialCapacity)
        {
        }

        /// <summary>Construct a buffer builder with an initial capacity that will be rounded up to the nearest power of 2.</summary>
        /// <param name="initialCapacity">at which the capacity will start.</param>
        private BufferBuilder(int initialCapacity)
        {
            capacity = QuickMath.NextPowerOfTwo(initialCapacity);

            protocolBuffer = new SafeBuffer(new byte[capacity]);
        }

        /// <summary>The current capacity of the buffer.</summary>
        /// <returns>the current capacity of the buffer.</returns>
        public virtual int Capacity()
        {
            return capacity;
        }

        /// <summary>The current position of the buffer that has been used by accumulate operations.</summary>
        /// <returns>the current position of the buffer that has been used by accumulate operations.</returns>
        public virtual int Position()
        {
            return position;
        }

        /// <summary>
        ///     The
        ///     <see cref="IClientProtocolBuffer" />
        ///     that encapsulates the internal buffer.
        /// </summary>
        /// <returns>
        ///     the
        ///     <see cref="IClientProtocolBuffer" />
        ///     that encapsulates the internal buffer.
        /// </returns>
        public virtual IClientProtocolBuffer Buffer()
        {
            return protocolBuffer;
        }

        /// <summary>Append a source buffer to the end of the internal buffer, resizing the internal buffer as required.</summary>
        /// <param name="srcBuffer">from which to copy.</param>
        /// <param name="srcOffset">in the source buffer from which to copy.</param>
        /// <param name="length">in bytes to copy from the source buffer.</param>
        /// <returns>the builder for fluent API usage.</returns>
        public virtual BufferBuilder Append(IClientProtocolBuffer srcBuffer, int srcOffset, int length)
        {
            EnsureCapacity(length);
            srcBuffer.GetBytes(srcOffset, protocolBuffer.ByteArray(), position, length);
            position += length;
            return this;
        }

        private void EnsureCapacity(int additionalCapacity)
        {
            var requiredCapacity = position + additionalCapacity;
            if (requiredCapacity < 0)
            {
                var s = string.Format("Insufficient capacity: position=%d additional=%d", position, additionalCapacity);
                throw new InvalidOperationException(s);
            }
            if (requiredCapacity > capacity)
            {
                var newCapacity = QuickMath.NextPowerOfTwo(requiredCapacity);
                var newBuffer = new byte[newCapacity];
                Array.Copy(protocolBuffer.ByteArray(), 0, newBuffer, 0, capacity);
                capacity = newCapacity;
                protocolBuffer.Wrap(newBuffer);
            }
        }
    }
}