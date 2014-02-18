using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hazelcast.Net.Ext
{
    internal class ByteBuffer
    {
        private byte[] buffer;
        private int capacity;
        private int index;
        private int limit;
        private int mark;
        //private bool _bigEndian=true;

        public ByteBuffer()
        {
            //this._bigEndian=true;
        }

        private ByteBuffer(byte[] buf, int start, int len)
        {
            this.buffer = buf;
            this.limit = start + len;
            this.index = start;
            this.mark = start;
            this.capacity = buf.Length;
            //this._bigEndian = true;
        }
        private ByteBuffer(byte[] buf, int len)
        {
            this.buffer = buf;
            this.limit = len;
            this.index = 0;
            this.mark = 0;
            this.capacity = buf.Length;
            //this._bigEndian = true;
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

        public void Compact()
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

        public void Put(byte[] data)
        {
            Put(data, 0, data.Length);
        }

        public void Put(byte data)
        {
            CheckPutLimit(1);
            buffer[index++] = data;
        }

        public void Put(byte[] data, int start, int len)
        {
            CheckPutLimit(len);
            for (int i = 0; i < len; i++)
            {
                buffer[index++] = data[i + start];
            }
        }

        public void PutInt(int i)
        {
            WriteInt(i);
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

        protected virtual int ReadInt()
        {
            CheckGetLimit(4);
            int ch1 = buffer[index++];
            int ch2 = buffer[index++];
            int ch3 = buffer[index++];
            int ch4 = buffer[index++];
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
        }

        protected virtual void WriteInt(int v)
        {
            CheckPutLimit(4);
            buffer[index++] = unchecked((byte)(((int)(((uint)v) >> 24)) & unchecked(0xFF)));
            buffer[index++] = unchecked((byte)(((int)(((uint)v) >> 16)) & unchecked(0xFF)));
            buffer[index++] = unchecked((byte)(((int)(((uint)v) >> 8)) & unchecked(0xFF)));
            buffer[index++] = unchecked((byte)((v) & unchecked(0xFF)));
        }
    }
}
