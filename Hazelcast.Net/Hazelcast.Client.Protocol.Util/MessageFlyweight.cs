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
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>Parameter Flyweight</summary>
    internal class MessageFlyweight
    {
        /// <summary>Long mask</summary>
        private const long LongMask = unchecked(0x00000000FFFFFFFFL);

        /// <summary>Int mask</summary>
        private const int IntMask = unchecked(0x0000FFFF);

        /// <summary>Short mask</summary>
        private const short ShortMask = unchecked(0x00FF);

        private int _index;
        private int _offset;

        protected IClientProtocolBuffer Buffer;

        public MessageFlyweight()
        {
            //initialized in wrap method by user , does not change.
            //starts from zero, incremented each tome something set to buffer
            _offset = 0;
        }

        //endregion SET Overloads
        //region GET Overloads
        public virtual bool GetBoolean()
        {
            var result = Buffer.GetByte(_index + _offset);
            _index += Bits.ByteSizeInBytes;
            return result != 0;
        }

        public virtual IClientProtocolBuffer GetBuffer()
        {
            return Buffer;
        }

        public virtual byte GetByte()
        {
            var result = Buffer.GetByte(_index + _offset);
            _index += Bits.ByteSizeInBytes;
            return result;
        }

        private ArraySegment<byte> GetByteArraySegment()
        {
            var length = Buffer.GetInt(_index + _offset);
            _index += Bits.IntSizeInBytes;
            var result = Buffer.GetBytesSegment(_index + _offset, length);
            _index += length;
            return result;
        }

        public virtual IData GetData()
        {
            return new HeapData(GetByteArraySegment());
        }

        public virtual IList<IData> GetDataList()
        {
            var length = Buffer.GetInt(_index + _offset);
            _index += Bits.IntSizeInBytes;
            IList<IData> result = new List<IData>();
            for (var i = 0; i < length; i++)
            {
                result.Add(GetData());
            }
            return result;
        }

        public virtual short GetShort()
        {
            var result = Buffer.GetShort(_index + _offset);
            _index += Bits.ShortSizeInBytes;
            return result;
        }

        public virtual int GetInt()
        {
            var result = Buffer.GetInt(_index + _offset);
            _index += Bits.IntSizeInBytes;
            return result;
        }

        public virtual long GetLong()
        {
            var result = Buffer.GetLong(_index + _offset);
            _index += Bits.LongSizeInBytes;
            return result;
        }

        public virtual KeyValuePair<IData, IData> GetMapEntry()
        {
            var key = GetData();
            var value = GetData();
            return new KeyValuePair<IData, IData>(key, value);
        }

        public virtual string GetStringUtf8()
        {
            var length = Buffer.GetInt(_index + _offset);
            var result = Buffer.GetStringUtf8(_index + _offset, length);
            _index += length + Bits.IntSizeInBytes;
            return result;
        }

        public virtual int Index()
        {
            return _index;
        }

        public virtual MessageFlyweight Index(int index)
        {
            _index = index;
            return this;
        }

        //region SET Overloads
        public virtual MessageFlyweight Set(bool value)
        {
            Buffer.PutByte(_index + _offset, unchecked((byte) (value ? 1 : 0)));
            _index += Bits.ByteSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(byte value)
        {
            Buffer.PutByte(_index + _offset, value);
            _index += Bits.ByteSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(int value)
        {
            Buffer.PutInt(_index + _offset, value);
            _index += Bits.IntSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(long value)
        {
            Buffer.PutLong(_index + _offset, value);
            _index += Bits.LongSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(string value)
        {
            _index += Buffer.PutStringUtf8(_index + _offset, value);
            return this;
        }

        public virtual MessageFlyweight Set(IData data)
        {
            var bytes = data.ToByteArraySegment();
            Set(bytes);
            return this;
        }

        public virtual MessageFlyweight Set(byte[] value)
        {
            Set(new ArraySegment<byte>(value));
            return this;
        }

        void Set(ArraySegment<byte> value)
        {
            var length = value.Count;
            Set(length);
            Buffer.PutBytes(_index + _offset, value.Array, value.Offset, value.Count);
            _index += length;
        }

        public virtual MessageFlyweight Set(ICollection<IData> value)
        {
            var length = value.Count;
            Set(length);
            foreach (var v in value)
            {
                Set(v);
            }
            return this;
        }

        public virtual MessageFlyweight Set(KeyValuePair<IData, IData> entry)
        {
            return Set(entry.Key).Set(entry.Value);
        }

        public virtual MessageFlyweight Wrap(IClientProtocolBuffer buffer, int offset)
        {
            Buffer = buffer;
            _offset = offset;
            _index = 0;
            return this;
        }

        protected internal virtual long Int64Get(int index)
        {
            return Buffer.GetLong(index + _offset);
        }

        protected internal virtual void Int64Set(int index, long value)
        {
            Buffer.PutLong(index + _offset, value);
        }

        //endregion GET Overloads
        protected internal virtual int Int32Get(int index)
        {
            return Buffer.GetInt(index + _offset);
        }

        protected internal virtual void Int32Set(int index, int value)
        {
            Buffer.PutInt(index + _offset, value);
        }

        protected internal virtual int Uint16Get(int index)
        {
            return Buffer.GetShort(index + _offset) & IntMask;
        }

        protected internal virtual void Uint16Put(int index, int value)
        {
            Buffer.PutShort(index + _offset, (short) value);
        }

        protected internal virtual long Uint32Get(int index)
        {
            return Buffer.GetInt(index + _offset) & LongMask;
        }

        protected internal virtual void Uint32Put(int index, long value)
        {
            Buffer.PutInt(index + _offset, (int) value);
        }

        protected internal virtual short Uint8Get(int index)
        {
            return (short) (Buffer.GetByte(index + _offset) & ShortMask);
        }

        protected internal virtual void Uint8Put(int index, short value)
        {
            Buffer.PutByte(index + _offset, unchecked((byte) value));
        }
    }
}