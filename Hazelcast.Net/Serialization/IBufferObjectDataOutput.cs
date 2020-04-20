using System;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    // TODO: document and format (see input)
    public interface IBufferObjectDataOutput : IObjectDataOutput, IDisposable
    {
        void Clear();

        int Position();

        void Position(int newPos);
        void Write(int position, int b);

        void WriteBoolean(int position, bool v);

        void WriteByte(int position, int v);

        void WriteChar(int position, int v);

        void WriteDouble(double v, Endianness endianness = Endianness.Unspecified);

        void WriteDouble(int position, double v, Endianness endianness = Endianness.Unspecified);

        void WriteFloat(float v, Endianness endianness = Endianness.Unspecified);

        void WriteFloat(int position, float v, Endianness endianness = Endianness.Unspecified);

        void WriteInt(int v, Endianness endianness = Endianness.Unspecified);

        void WriteInt(int position, int v, Endianness endianness = Endianness.Unspecified);

        void WriteLong(long v, Endianness endianness = Endianness.Unspecified);

        void WriteLong(int position, long v, Endianness endianness = Endianness.Unspecified);

        void WriteShort(int v, Endianness endianness = Endianness.Unspecified);

        void WriteShort(int position, int v, Endianness endianness = Endianness.Unspecified);

        void WriteZeroBytes(int count);
    }
}
