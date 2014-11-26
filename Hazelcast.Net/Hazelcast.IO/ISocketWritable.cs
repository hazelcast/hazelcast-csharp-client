using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
	/// <summary>
	/// Represents something that can be written to a
	/// <see cref="com.hazelcast.nio.Connection">com.hazelcast.nio.Connection</see>
	/// .
	/// todo:
	/// Perhaps this class should be renamed to ConnectionWritable since it is written to a
	/// <see cref="com.hazelcast.nio.Connection#write(SocketWritable)">com.hazelcast.nio.Connection#write(SocketWritable)
	/// 	</see>
	/// . This aligns the names.
	/// </summary>
	internal interface ISocketWritable
	{
		/// <summary>Asks the SocketWritable to write its content to the destination ByteBuffer.
		/// 	</summary>
		/// <remarks>Asks the SocketWritable to write its content to the destination ByteBuffer.
		/// 	</remarks>
		/// <param name="destination">the ByteBuffer to write to.</param>
		/// <returns>todo: unclear what return value means.</returns>
		bool WriteTo(ByteBuffer destination);

		/// <summary>Checks if this SocketWritable is urgent.</summary>
		/// <remarks>
		/// Checks if this SocketWritable is urgent.
		/// SocketWritable that are urgent, have priority above regular SocketWritable. This is useful to implement
		/// System Operations so that they can be send faster than regular operations; especially when the system is
		/// under load you want these operations have precedence.
		/// </remarks>
		/// <returns>true if urgent, false otherwise.</returns>
		bool IsUrgent();
	}
}
