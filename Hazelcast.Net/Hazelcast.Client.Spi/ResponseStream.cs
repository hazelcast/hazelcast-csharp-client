using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;


namespace Hazelcast.Client.Spi
{
	
	internal sealed class ResponseStream : IResponseStream
	{
		private readonly ISerializationService serializationService;

		private readonly Connection.IConnection connection;

		private bool ended = false;

		internal ResponseStream(ISerializationService serializationService, Connection.IConnection connection)
		{
			this.serializationService = serializationService;
			this.connection = connection;
		}

		/// <exception cref="System.Exception"></exception>
		public object Read()
		{
			Data data = connection.Read();
			object result = serializationService.ToObject(data);
			return ErrorHandler.ReturnResultOrThrowException<object>(result);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void End()
		{
			lock (this)
			{
				if (!ended)
				{
					connection.Close();
					ended = true;
				}
			}
		}
	}
}
