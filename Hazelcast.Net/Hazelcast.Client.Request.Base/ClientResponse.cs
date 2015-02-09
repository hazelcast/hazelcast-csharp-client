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
        public IData _errData;

        private GenericError _error;

        private Boolean _isError;

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
            _isError = input.ReadBoolean();
            _response = input.ReadData();
        }

        public Boolean IsError
        {
            get { return _isError; }
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