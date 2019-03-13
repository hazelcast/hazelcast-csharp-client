// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using Hazelcast.IO;

namespace Hazelcast.Net.Ext
{
    internal class ByteBuffer
    {
        private bool _bigEndian = true;
        private volatile byte[] _buffer;
        private volatile int _capacity;
        private volatile int _index;
        private volatile int _limit;
        private volatile int _mark;

        public ByteBuffer()
        {
            _bigEndian = true;
        }

        private ByteBuffer(byte[] buf, int start, int len)
        {
            _buffer = buf;
            _limit = start + len;
            _index = start;
            _mark = start;
            _capacity = buf.Length;
            _bigEndian = true;
        }

        private ByteBuffer(byte[] buf, int len)
        {
            _buffer = buf;
            _limit = len;
            _index = 0;
            _mark = 0;
            _capacity = buf.Length;
            _bigEndian = true;
        }

        internal ByteBuffer(byte[] buffer, int capacity, int index, int limit, int mark, bool bigEndian)
        {
            _buffer = buffer;
            _capacity = capacity;
            _index = index;
            _limit = limit;
            _mark = mark;
            _bigEndian = bigEndian;
        }

        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        public int Position
        {
            get { return _index; }
            set
            {
                if ((value < 0) || (value > _limit))
                {
                    throw new IndexOutOfRangeException("Byte Buffer under flow");
                }
                _index = value;
            }
        }

        public ByteOrder Order
        {
            get { return _bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian; }

            set { _bigEndian = (value == ByteOrder.BigEndian); }
        }

        public static ByteBuffer Allocate(int size)
        {
            return new ByteBuffer(new byte[size], size);
        }

        public byte[] Array()
        {
            return _buffer;
        }

        public int Capacity()
        {
            return _capacity;
        }

        public void Clear()
        {
            _index = 0;
            _limit = _capacity;
        }

        public virtual void Compact()
        {
            System.Array.Copy(_buffer, Position, _buffer, 0, Remaining());
            Position = Remaining();
            Limit = Capacity();
            _mark = -1;
        }

        public void Flip()
        {
            _limit = _index;
            _index = 0;
        }

        public byte Get()
        {
            CheckGetLimit(1);
            return _buffer[_index++];
        }

        public void Get(byte[] data)
        {
            Get(data, 0, data.Length);
        }

        public void Get(byte[] data, int start, int len)
        {
            CheckGetLimit(len);
            for (var i = 0; i < len; i++)
            {
                data[i + start] = _buffer[_index++];
            }
        }

        public int GetInt()
        {
            return ReadInt();
        }

        public short GetShort()
        {
            return ReadShort();
        }

        public bool HasArray()
        {
            return true;
        }

        public bool HasRemaining()
        {
            return _index < _limit;
        }


        public void Mark()
        {
            _mark = _index;
        }

        public virtual void Put(byte[] data)
        {
            Put(data, 0, data.Length);
        }

        public virtual void Put(byte data)
        {
            CheckPutLimit(1);
            _buffer[_index++] = data;
        }

        public virtual void Put(byte[] data, int start, int len)
        {
            CheckPutLimit(len);
            for (var i = 0; i < len; i++)
            {
                _buffer[_index++] = data[i + start];
            }
        }

        public ByteBuffer Put(ByteBuffer src)
        {
            if (src == this)
                throw new ArgumentException("Source cannot be destination");
            var n = src.Remaining();
            if (n > Remaining())
                throw new InternalBufferOverflowException();
            for (var i = 0; i < n; i++)
                Put(src.Get());
            return this;
        }

        public virtual void PutInt(int i)
        {
            WriteInt(i);
        }

        public virtual void PutShort(short i)
        {
            WriteShort(i);
        }

        public int Remaining()
        {
            return (_limit - _index);
        }

        public void Reset()
        {
            _index = _mark;
        }

        public static ByteBuffer Wrap(byte[] buf)
        {
            return new ByteBuffer(buf, buf.Length);
        }

        public static ByteBuffer Wrap(byte[] buf, int start, int len)
        {
            return new ByteBuffer(buf, start, len);
        }

        protected virtual int ReadInt()
        {
            CheckGetLimit(Bits.IntSizeInBytes);
            var i = Bits.ReadInt(_buffer, _index, _bigEndian);
            _index += Bits.IntSizeInBytes;
            return i;
        }

        protected virtual short ReadShort()
        {
            CheckGetLimit(Bits.ShortSizeInBytes);
            var i = Bits.ReadShort(_buffer, _index, _bigEndian);
            _index += Bits.ShortSizeInBytes;
            return i;
        }

        protected virtual void WriteInt(int v)
        {
            CheckPutLimit(Bits.IntSizeInBytes);
            Bits.WriteInt(_buffer, _index, v, _bigEndian);
            _index += Bits.IntSizeInBytes;
        }

        protected virtual void WriteShort(short v)
        {
            CheckPutLimit(Bits.ShortSizeInBytes);
            Bits.WriteShort(_buffer, _index, v, _bigEndian);
            _index += Bits.ShortSizeInBytes;
        }

        private void CheckGetLimit(int inc)
        {
            if ((_index + inc) > _limit)
            {
                throw new IndexOutOfRangeException("Byte Buffer under flow");
            }
        }

        private void CheckPutLimit(int inc)
        {
            if ((_index + inc) > _limit)
            {
                throw new IndexOutOfRangeException("Byte Buffer under flow");
            }
        }
    }
}