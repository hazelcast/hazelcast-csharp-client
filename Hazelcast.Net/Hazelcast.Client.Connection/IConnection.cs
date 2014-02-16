using System;
using System.Net;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Connection
{
    public interface IConnection2 : IDisposable
    {
        /// <exception cref="System.IO.IOException"></exception>
        bool Write(Data data);

        /// <exception cref="System.IO.IOException"></exception>
        Data Read();

        int GetId();

        long GetLastReadTime();

        /// <summary>May close socket or return connection to a pool depending on connection's type.</summary>
        /// <remarks>May close socket or return connection to a pool depending on connection's type.</remarks>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void Release();

        /// <summary>Closes connection's socket, regardless of connection's type.</summary>
        /// <remarks>Closes connection's socket, regardless of connection's type.</remarks>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void Close();

        Address GetRemoteEndpoint();

        void SetRemoteEndpoint(Address address);

        IPEndPoint GetLocalSocketAddress();
    }
}