/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System.Collections.Generic;
using System.Linq;
using Hazelcast.Logging;

namespace Hazelcast.Client.Test
{
    public class HazelcastCluster
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (HazelcastCluster));
        private readonly Dictionary<int, HazelcastNode> _nodes;
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

        public int AddNode()
        {
            var id = _nextNodeId++;
            var node = new HazelcastNode(id);
            _nodes[id] = node;
            node.Start();
            return id;
        }

        public void RemoveNode()
        {
            // remove node with largest id
            var key = _nodes.Keys.Max();
            RemoveNode(key);
        }

        public void RemoveNode(int id)
        {
            _nodes[id].Stop();
            _nodes.Remove(id);
        }

        public void ResumeNode(int id)
        {
            _nodes[id].Resume();
        }

        public void Shutdown()
        {
            foreach (var node in _nodes.Values)
            {
                node.Stop();
            }
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

        public void SuspendNode(int id)
        {
            _nodes[id].Suspend();
        }

        public ICollection<int> NodeIds
        {
            get { return _nodes.Keys; }
        }
    }
}
