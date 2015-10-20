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

        protected internal IClientProtocolBuffer buffer;
        private int index;
        private int offset;

        public MessageFlyweight()
        {
            //initialized in wrap method by user , does not change.
            //starts from zero, incremented each tome something set to buffer
            offset = 0;
        }

        public virtual MessageFlyweight Wrap(IClientProtocolBuffer buffer, int offset)
        {
            this.buffer = buffer;
            this.offset = offset;
            index = 0;
            return this;
        }

        public virtual int Index()
        {
            return index;
        }

        public virtual MessageFlyweight Index(int index)
        {
            this.index = index;
            return this;
        }

        public virtual IClientProtocolBuffer Buffer()
        {
            return buffer;
        }

        //region SET Overloads
        public virtual MessageFlyweight Set(bool value)
        {
            buffer.PutByte(index + offset, unchecked((byte) (value ? 1 : 0)));
            index += Bits.ByteSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(int value)
        {
            buffer.PutInt(index + offset, value);
            index += Bits.IntSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(long value)
        {
            buffer.PutLong(index + offset, value);
            index += Bits.LongSizeInBytes;
            return this;
        }

        public virtual MessageFlyweight Set(string value)
        {
            index += buffer.PutStringUtf8(index + offset, value);
            return this;
        }

        public virtual MessageFlyweight Set(IData data)
        {
            var bytes = data.ToByteArray();
            Set(bytes);
            return this;
        }

        public virtual MessageFlyweight Set(byte[] value)
        {
            var length = value.Length;
            Set(length);
            buffer.PutBytes(index + offset, value);
            index += length;
            return this;
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

        //endregion SET Overloads
        //region GET Overloads
        public virtual bool GetBoolean()
        {
            var result = buffer.GetByte(index + offset);
            index += Bits.ByteSizeInBytes;
            return result != 0;
        }

        public virtual byte GetByte()
        {
            var result = buffer.GetByte(index + offset);
            index += Bits.ByteSizeInBytes;
            return result;
        }

        public virtual int GetInt()
        {
            var result = buffer.GetInt(index + offset);
            index += Bits.IntSizeInBytes;
            return result;
        }

        public virtual long GetLong()
        {
            var result = buffer.GetLong(index + offset);
            index += Bits.LongSizeInBytes;
            return result;
        }

        public virtual string GetStringUtf8()
        {
            var length = buffer.GetInt(index + offset);
            var result = buffer.GetStringUtf8(index + offset, length);
            index += length + Bits.IntSizeInBytes;
            return result;
        }

        public virtual byte[] GetByteArray()
        {
            var length = buffer.GetInt(index + offset);
            index += Bits.IntSizeInBytes;
            var result = new byte[length];
            buffer.GetBytes(index + offset, result);
            index += length;
            return result;
        }

        public virtual IData GetData()
        {
            return new HeapData(GetByteArray());
        }

        public virtual IList<IData> GetDataList()
        {
            var length = buffer.GetInt(index + offset);
            index += Bits.IntSizeInBytes;
            IList<IData> result = new List<IData>();
            for (var i = 0; i < length; i++)
            {
                result.Add(GetData());
            }
            return result;
        }

        public virtual ICollection<IData> GetDataSet()
        {
            var length = buffer.GetInt(index + offset);
            index += Bits.IntSizeInBytes;
            ICollection<IData> result = new HashSet<IData>();
            for (var i = 0; i < length; i++)
            {
                result.Add(GetData());
            }
            return result;
        }

        public virtual KeyValuePair<IData, IData> GetMapEntry()
        {
            var key = GetData();
            var value = GetData();
            return new KeyValuePair<IData, IData>(key, value);
        }

        //endregion GET Overloads
        protected internal virtual int Int32Get(int index)
        {
            return buffer.GetInt(index + offset);
        }

        protected internal virtual void Int32Set(int index, int length)
        {
            buffer.PutInt(index + offset, length);
        }

        protected internal virtual short Uint8Get(int index)
        {
            return (short) (buffer.GetByte(index + offset) & ShortMask);
        }

        protected internal virtual void Uint8Put(int index, short value)
        {
            buffer.PutByte(index + offset, unchecked((byte) value));
        }

        protected internal virtual int Uint16Get(int index)
        {
            return buffer.GetShort(index + offset) & IntMask;
        }

        protected internal virtual void Uint16Put(int index, int value)
        {
            buffer.PutShort(index + offset, (short) value);
        }

        protected internal virtual long Uint32Get(int index)
        {
            return buffer.GetInt(index + offset) & LongMask;
        }

        protected internal virtual void Uint32Put(int index, long value)
        {
            buffer.PutInt(index + offset, (int) value);
        }
    }
}