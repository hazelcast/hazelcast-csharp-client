using System;
using Hazelcast.IO;


namespace Hazelcast.IO
{
	
	public interface IBufferObjectDataInput : IObjectDataInput, IDisposable
	{
		/// <exception cref="System.IO.IOException"></exception>
		int Read(int position);

		/// <exception cref="System.IO.IOException"></exception>
		int ReadInt(int position);

		/// <exception cref="System.IO.IOException"></exception>
		long ReadLong(int position);

		/// <exception cref="System.IO.IOException"></exception>
		bool ReadBoolean(int position);

		/// <exception cref="System.IO.IOException"></exception>
		byte ReadByte(int position);

		/// <exception cref="System.IO.IOException"></exception>
		char ReadChar(int position);

		/// <exception cref="System.IO.IOException"></exception>
		double ReadDouble(int position);

		/// <exception cref="System.IO.IOException"></exception>
		float ReadFloat(int position);

		/// <exception cref="System.IO.IOException"></exception>
		short ReadShort(int position);

		int Position();

		void Position(int newPos);

		void Reset();
	}
}
