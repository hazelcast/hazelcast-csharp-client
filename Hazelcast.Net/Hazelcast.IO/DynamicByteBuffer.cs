using System;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
	internal sealed class DynamicByteBuffer
	{
		private ByteBuffer buffer;

		public DynamicByteBuffer(byte[] array)
		{
			buffer = array != null ? ByteBuffer.Wrap(array) : ByteBuffer.Allocate(0);
		}

		public DynamicByteBuffer(byte[] array, int offset, int length)
		{
			buffer = ByteBuffer.Wrap(array, offset, length);
		}

		public DynamicByteBuffer Compact()
		{
			buffer.Compact();
			return this;
		}

		public DynamicByteBuffer Get(byte[] dst)
		{
			buffer.Get(dst);
			return this;
		}
		public DynamicByteBuffer Get(byte[] dst, int offset, int length)
		{
			buffer.Get(dst, offset, length);
			return this;
		}

		public DynamicByteBuffer Put(byte[] src)
		{
			EnsureSize(src.Length);
			buffer.Put(src);
			return this;
		}

		public DynamicByteBuffer PutInt(int value)
		{
			EnsureSize(4);
			buffer.PutInt(value);
			return this;
		}

		private void EnsureSize(int i)
		{
			Check();
			if (buffer.Remaining() < i)
			{
				int newCap = Math.Max(buffer.Limit << 1, buffer.Limit + i);
				ByteBuffer newBuffer =  ByteBuffer.Allocate(newCap);
				newBuffer.Order=buffer.Order;
				buffer.Flip();
				newBuffer.Put(buffer);
				buffer = newBuffer;
			}
		}

		public DynamicByteBuffer Clear()
		{
			Check();
			buffer.Clear();
			return this;
		}

		public DynamicByteBuffer Flip()
		{
			Check();
			buffer.Flip();
			return this;
		}

		public int Limit
		{
		    get
		    {
		        Check();
		        return buffer.Limit;
		    }
		    set
		    {
		        Check();
		        buffer.Limit = value;
		    }
		}

		public DynamicByteBuffer Mark()
		{
			Check();
			buffer.Mark();
			return this;
		}

		public int Position()
		{
			Check();
			return buffer.Position;
		}

		public DynamicByteBuffer Position(int newPosition)
		{
			Check();
			buffer.Position=newPosition;
			return this;
		}

		public int Remaining()
		{
			Check();
			return buffer.Remaining();
		}

		public DynamicByteBuffer Reset()
		{
			Check();
			buffer.Reset();
			return this;
		}

		public int Capacity()
		{
			Check();
			return buffer.Capacity();
		}

		public bool HasRemaining()
		{
			Check();
			return buffer.HasRemaining();
		}

		public byte[] Array()
		{
			Check();
            return buffer.Array();
		}

		public ByteOrder Order()
		{
			Check();
			return buffer.Order;
		}

		public DynamicByteBuffer Order(ByteOrder order)
		{
			Check();
			buffer.Order=order;
			return this;
		}

		public void Close()
		{
			buffer = null;
		}

		private void Check()
		{
			if (buffer == null)
			{
				throw new InvalidOperationException("Buffer is closed!");
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder("DynamicByteBuffer{");
			if (buffer != null)
			{
				sb.Append("position=").Append(buffer.Position);
				sb.Append(", limit=").Append(buffer.Limit);
				sb.Append(", capacity=").Append(buffer.Capacity());
				sb.Append(", order=").Append(buffer.Order);
			}
			else
			{
				sb.Append("<CLOSED>");
			}
			sb.Append('}');
			return sb.ToString();
		}
	}
}
