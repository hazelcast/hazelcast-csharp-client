using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncTests1.Networking
{
    public class Cluster
    {

        // connection manager   <- would be 'ClusterConnections'
        //   has a set of member id (guid) -> connections
        //   can return connections
        //   handles connection added/removed events
        //   maintains these connections, connect to whole cluster?

        // partition service
        //   has a set of partition id (int) -> member id (guid)
        //   TODO understand the relation between partitions and members

        // invocation service   <- more or less our 'Client'
        //  assigns correlation id
        //  invokes on a connection, obtained from the connection manager
        //   seems to manage some retry strategy?
        //  handles incoming messages
        //   route event message to listener service
        //   completes the invocation with result / exception etc   <- what our 'Client' does

    }
}
