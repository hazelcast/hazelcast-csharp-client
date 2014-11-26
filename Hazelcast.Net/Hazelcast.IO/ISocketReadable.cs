using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
	/// <summary>Represents something where data can be read from.</summary>
	/// <remarks>Represents something where data can be read from.</remarks>
	internal interface ISocketReadable
	{
		bool ReadFrom(ByteBuffer source);
	}
}
