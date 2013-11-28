using System;
using Hazelcast.IO;


namespace Hazelcast.IO
{
	
	public interface IBufferObjectDataOutput : IObjectDataOutput, IDisposable
	{
		void Write(int position, int b);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteInt(int position, int v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteLong(int position, long v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteBoolean(int position, bool v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteByte(int position, int v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteChar(int position, int v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteDouble(int position, double v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteFloat(int position, float v);

		/// <exception cref="System.IO.IOException"></exception>
		void WriteShort(int position, int v);

		int Position();

		void Position(int newPos);

		byte[] GetBuffer();

		void Clear();
	}
}
