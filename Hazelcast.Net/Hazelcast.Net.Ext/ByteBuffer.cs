using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hazelcast.IO;

namespace Hazelcast.Net.Ext
{
    internal class ByteBuffer
    {
        private volatile byte[] buffer;
        private volatile int capacity;
        private volatile int index;
        private volatile int limit;
        private volatile int mark;
        private bool _bigEndian=true;

        public ByteBuffer()
        {
            this._bigEndian=true;
        }

        private ByteBuffer(byte[] buf, int start, int len)
        {
            this.buffer = buf;
            this.limit = start + len;
            this.index = start;
            this.mark = start;
            this.capacity = buf.Length;
            this._bigEndian = true;
        }
        private ByteBuffer(byte[] buf, int len)
        {
            this.buffer = buf;
            this.limit = len;
            this.index = 0;
            this.mark = 0;
            this.capacity = buf.Length;
            this._bigEndian = true;
        }

        internal ByteBuffer(byte[] buffer, int capacity, int index, int limit, int mark, bool bigEndian)
        {
            this.buffer = buffer;
            this.capacity = capacity;
            this.index = index;
            this.limit = limit;
            this.mark = mark;
            _bigEndian = bigEndian;
        }

        public static ByteBuffer Allocate(int size)
        {
            return new ByteBuffer(new byte[size], size);
        }

        public byte[] Array()
        {
            return buffer;
        }

        public int Capacity()
        {
            return capacity;
        }

        private void CheckGetLimit(int inc)
        {
            if ((index + inc) > limit)
            {
                throw new IndexOutOfRangeException("Byte Buffer under flow");
            }
        }

        private void CheckPutLimit(int inc)
        {
            if ((index + inc) > limit)
            {
                throw new IndexOutOfRangeException("Byte Buffer under flow");
            }
        }

        public virtual void Compact()
        {
            System.Array.Copy(buffer, Position, buffer, 0, Remaining());
            Position = Remaining();
            Limit = Capacity();
            mark = -1;
        }

        public void Clear()
        {
            index = 0;
            limit = capacity;
        }

        public void Flip()
        {
            limit = index;
            index = 0;
        }

        public byte Get()
        {
            CheckGetLimit(1);
            return buffer[index++];
        }

        public void Get(byte[] data)
        {
            Get(data, 0, data.Length);
        }

        public void Get(byte[] data, int start, int len)
        {
            CheckGetLimit(len);
            for (int i = 0; i < len; i++)
            {
                data[i + start] = buffer[index++];
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

        public int Limit
        {
            get
            {
                return limit;
            }
            set
            {
                limit = value; 
            }
        }


        public void Mark()
        {
            mark = index;
        }

        public int Position
        {
            get
            {
                return index;
            }
            set
            {
                if ((value < 0) || (value > limit))
                {
                    throw new IndexOutOfRangeException("Byte Buffer under flow");
                }
                index = value; 
            }
        }

        public ByteOrder Order
        {
            get
            {
                return _bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
            }

            set
            {
                _bigEndian = (value == ByteOrder.BigEndian);
            }
        }

        public virtual void Put(byte[] data)
        {
            Put(data, 0, data.Length);
        }

        public virtual void Put(byte data)
        {
            CheckPutLimit(1);
            buffer[index++] = data;
        }

        public virtual void Put(byte[] data, int start, int len)
        {
            CheckPutLimit(len);
            for (int i = 0; i < len; i++)
            {
                buffer[index++] = data[i + start];
            }
        }

        public virtual void PutInt(int i)
        {
            WriteInt(i);
        }
        public virtual void PutShort(short i)
        {
            WriteShort(i);
        }

        public ByteBuffer Put(ByteBuffer src)
        {
            if (src == this)
                throw new ArgumentException("Source cannot be destination");
            int n = src.Remaining();
            if (n > Remaining())
                throw new InternalBufferOverflowException();
            for (int i = 0; i < n; i++)
                Put(src.Get());
            return this;
        }

        public int Remaining()
        {
            return (limit - index);
        }

        public bool HasRemaining() 
        {
            return index < limit;
        }

        public void Reset()
        {
            index = mark;
        }

        public static ByteBuffer Wrap(byte[] buf)
        {
            return new ByteBuffer(buf, buf.Length);
        }

        public static ByteBuffer Wrap(byte[] buf, int start, int len)
        {
            return new ByteBuffer(buf, start, len);
        }

        public ByteBuffer AsReadOnlyBuffer()
        {
            return new ByteBufferReadOnly(this.buffer, this.capacity, this.index, this.limit, this.mark, this._bigEndian);
        }

        protected virtual int ReadInt()
        {
            CheckGetLimit(Bits.IntSizeInBytes);
            int i = Bits.ReadInt(buffer, index, _bigEndian);
            index += Bits.IntSizeInBytes;
            return i;
        }

        protected virtual void WriteInt(int v)
        {
            CheckPutLimit(Bits.IntSizeInBytes);
            Bits.WriteInt(buffer, index, v, _bigEndian);
            index += Bits.IntSizeInBytes;
        }
        protected virtual short ReadShort()
        {
            CheckGetLimit(Bits.ShortSizeInBytes);
            short i = Bits.ReadShort(buffer, index, _bigEndian);
            index += Bits.ShortSizeInBytes;
            return i;
        }

        protected virtual void WriteShort(short v)
        {
            CheckPutLimit(Bits.ShortSizeInBytes);
            Bits.WriteShort(buffer, index, v, _bigEndian);
            index += Bits.ShortSizeInBytes;
        }
    }

    internal class ByteBufferReadOnly : ByteBuffer
    {
        internal ByteBufferReadOnly(byte[] buffer, int capacity, int index, int limit, int mark, bool bigEndian) : base(buffer, capacity, index, limit, mark, bigEndian)
        {
        }

        public override void Compact()
        {
            ReadonlyException();
        }

        public override void Put(byte[] data)
        {
            ReadonlyException();
        }

        public override void Put(byte data)
        {
            ReadonlyException();
        }

        public override void Put(byte[] data, int start, int len)
        {
            ReadonlyException();
        }

        public override void PutInt(int i)
        {
            ReadonlyException();
        }

        public override void PutShort(short i)
        {
            ReadonlyException();
        }

        protected override void WriteInt(int v)
        {
            ReadonlyException();
        }

        protected override void WriteShort(short v)
        {
            ReadonlyException();
        }

        private static void ReadonlyException()
        {
            throw new NotSupportedException("ByteBuffer is read only");
        }
    }
}
