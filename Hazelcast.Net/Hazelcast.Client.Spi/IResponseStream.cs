using Hazelcast.Client.Spi;


namespace Hazelcast.Client.Spi
{
	
	public interface IResponseStream
	{
		/// <exception cref="System.Exception"></exception>
		object Read();

		/// <exception cref="System.IO.IOException"></exception>
		void End();
	}
}
