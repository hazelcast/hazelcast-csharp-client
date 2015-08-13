using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Client.Connection
{
    internal interface IConnectionHeartbeatListener
    {
        void HeartBeatStarted(ClientConnection connection);
        void HeartBeatStopped(ClientConnection connection);
    }
}
