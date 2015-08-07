using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol
{
    public interface IClientMessage
    {
        int GetMessageType();
        bool GetBoolean();
        IData GetData();
        string GetStringUtf8();
        int GetInt();
    }
}