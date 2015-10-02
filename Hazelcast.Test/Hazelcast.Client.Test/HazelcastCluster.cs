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

        public int Size
        {
            get { return _nodes.Count; }
        }

        public void Start()
        {
            foreach (var node in _nodes.Values)
            {
                node.Start();
            }
        }

        public void Stop(int id)
        {
            _nodes[id].Stop();
        }

        public int AddNode()
        {
            var id = _nextNodeId++;
            var node = new HazelcastNode(id);
            _nodes[id] = node;
            node.Start();
            return id;
        }

        public void Shutdown()
        {
            foreach (var node in _nodes.Values)
            {
                node.Stop();
            }
        }

        public void RemoveNode()
        {
            var key = _nodes.Keys.First();
            RemoveNode(key);
        }
        public void RemoveNode(int id)
        {
            _nodes[id].Stop();
            _nodes.Remove(id);
        }
    }
}
