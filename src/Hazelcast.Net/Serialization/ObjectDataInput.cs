// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataInput : IObjectDataInput, IDisposable
    {
        private byte[] _buffer;
        private readonly SerializationService _serializationService;
        private int _length;

        public ObjectDataInput(byte[] buffer, SerializationService serializationService, Endianness endianness, int offset = 0)
        {
            _buffer = buffer;
            _serializationService = serializationService;
            Endianness = endianness;
            _length = buffer?.Length ?? 0;
            Debug.Assert(offset >= 0 && offset <= _length, "Wrong offset value for input");
            Position = offset;
        }

        /// <summary>
        /// Initializes the buffer.
        /// </summary>
        /// <param name="data">The buffer data.</param>
        /// <param name="offset">The buffer data offset.</param>
        public void Initialize(byte[] buffer, int offset)
        {
            _buffer = buffer;
            Position = offset;
            _length = buffer?.Length ?? 0;
        }

        internal int Position { get; set; }

        public void MoveTo(int position)
        {
            CheckAvailable(position, 0);
            Position = position;
        }

        //test only
        internal byte[] Buffer => _buffer;

        public void Dispose()
        {
            _buffer = null;
            Position = 0;
            _length = 0;
        }

        internal void CheckAvailable(int position, int count)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_length - position < count)
                throw new InvalidOperationException(ExceptionMessages.NotEnoughBytes);
        }
    }
}
