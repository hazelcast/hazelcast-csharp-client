using System.Collections.Generic;
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
        long GetLong();
        KeyValuePair<IData, IData> GetMapEntry
            ();
    }
}