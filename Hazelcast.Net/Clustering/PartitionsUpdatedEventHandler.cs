using System;
using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    internal class PartitionsUpdatedEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, EventArgs> _handler;

        public PartitionsUpdatedEventHandler(Action<Cluster, EventArgs> handler)
        {
            _handler = handler;
        }

        public void Handle(Cluster cluster, MemberInfo member)
            => _handler(cluster, EventArgs.Empty);

        public void Handle(Cluster cluster, EventArgs args)
            => _handler(cluster, args);
    }
}
