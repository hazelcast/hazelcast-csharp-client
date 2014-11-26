namespace Hazelcast.IO.Serialization
{
	/// <summary>
	/// Base interface of custom serialization interfaces
	/// <p/>
	/// see
	/// <see cref="IByteArraySerializer{T}">IByteArraySerializer&lt;T&gt;</see>
	/// see
	/// <see cref="IStreamSerializer{T}">IStreamSerializer&lt;T&gt;</see>
	/// </summary>
	public interface ISerializer
	{
		/// <returns>typeId of serializer</returns>
		int GetTypeId();

		/// <summary>Called when instance is shutting down.</summary>
		/// <remarks>Called when instance is shutting down. It can be used to clear used resources.
		/// 	</remarks>
		void Destroy();
	}
}
