using System.Collections.Generic;
using Hazelcast.IO;
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
        KeyValuePair<IData, IData> GetMapEntry();
        bool IsRetryable();
        int GetCorrelationId();
        bool IsFlagSet(short listenerEventFlag);
        int GetPartitionId();

        /// <summary>Sets the correlation id field.</summary>
        /// <param name="correlationId">The value to set in the correlation id field.</param>
        /// <returns>The ClientMessage with the new correlation id field value.</returns>
        IClientMessage SetCorrelationId(int correlationId);
    }
}