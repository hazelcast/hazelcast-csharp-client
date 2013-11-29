using Hazelcast.Client.Connection;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ResponseStream : IResponseStream
    {
        private readonly IConnection connection;
        private readonly ISerializationService serializationService;

        private bool ended;

        internal ResponseStream(ISerializationService serializationService, IConnection connection)
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