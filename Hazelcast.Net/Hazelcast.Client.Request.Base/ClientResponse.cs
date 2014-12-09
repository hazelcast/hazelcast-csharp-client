using System;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    /// <summary>
    ///     Server response wrapper
    /// </summary>
    internal class ClientResponse : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        private int _callId;
        private IData _response;

        private GenericError _error;

        public ClientResponse()
        {
        }

        public int GetFactoryId()
        {
            return ClientDataSerializerHook.Id;
        }

        public int GetId()
        {
            return ClientDataSerializerHook.ClientResponse;
        }

        public void WriteData(IObjectDataOutput output)
        {
            throw new NotSupportedException();
        }

        public void ReadData(IObjectDataInput input)
        {
            _callId = input.ReadInt();
            var isError = input.ReadBoolean();
            if (isError)
            {
                _error = input.ReadObject<GenericError>();
                _response = null;
            }
            else
            {
                _response = input.ReadData();
                _error = null;
            }
        }

        public GenericError Error
        {
            get { return _error; }
        }

        public IData Response
        {
            get { return _response; }
        }

        public int CallId
        {
            get { return _callId; }
        }
    }
}