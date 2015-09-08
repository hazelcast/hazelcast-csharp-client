using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Logging;

namespace Hazelcast.Client.Test
{
    public class HazelcastCluster
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (HazelcastCluster));
        private readonly Dictionary<int, HazelcastNode> _nodes = new Dictionary<int, HazelcastNode>();
        private int _nextNodeId;

        public HazelcastCluster(int initialNodeCount)
        {
            _nodes = Enumerable.Range(0, initialNodeCount).ToDictionary(
                i => i, i => new HazelcastNode(i));
            _nextNodeId = initialNodeCount;
        }

        public void Start()
        {
            foreach (var node in _nodes.Values)
            {
                node.Start();
            }
            //wait for instances to start up
        }

        public void Shutdown()
        {
            foreach (var node in _nodes.Values)
            {
                node.Stop();
            }
        }
    }
}
