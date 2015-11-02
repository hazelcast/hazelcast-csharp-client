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

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class 
        ClientPartitionService : IClientPartitionService
    {
        private const int PartitionTimeout = 60000;
        private const int PartitionRefreshPeriod = 10000;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientPartitionService));
        private readonly HazelcastClient _client;
        private readonly ConcurrentDictionary<int, Address> _partitions = new ConcurrentDictionary<int, Address>();
        private readonly AtomicBoolean _updating = new AtomicBoolean(false);
        private volatile int _partitionCount;
        private Thread _partitionThread;
        private volatile bool _isLive;

        public ClientPartitionService(HazelcastClient client)
        {
            _client = client;
        }

        public void Start()
        {
            _isLive = true;
            _partitionThread = new Thread(RefreshPartitionThread) {IsBackground = true, Name = "hz-partition-updater-" + new Random().Next()};
            _partitionThread.Start();
        }

        public void RefreshPartitions()
        {
            _client.GetClientExecutionService().Submit(() => { GetPartitions(); });
        }

        public void Stop()
        {
            try
            {
                _isLive = false;
                _partitionThread.Interrupt();
                _partitionThread.Join();
            }
            catch (Exception e)
            {
                Logger.Finest("Shut down partition refresher thread problem...", e);
            }
            _partitions.Clear();
        }

        private void RefreshPartitionThread()
        {
            while (_isLive)
            {
                try
                {
                    GetPartitions();
                    Thread.Sleep(PartitionRefreshPeriod);
                }
                catch (ThreadInterruptedException)
                {
                }
            }   
        }

        private bool GetPartitions()
        {
            if (_updating.CompareAndSet(false, true))
            {
                try
                {
                    Logger.Finest("Updating partition list.");
                    var clusterService = _client.GetClientClusterService();
                    var ownerAddress = clusterService.GetOwnerConnectionAddress();
                    var connection = _client.GetConnectionManager().GetConnection(ownerAddress);
                    if (connection == null)
                    {
                        throw new InvalidOperationException(
                            "Owner connection is not available, could not get partitions.");
                    }
                    var response = GetPartitionsFrom(connection);
                    var result = ProcessPartitionResponse(response);
                    Logger.Finest("Partition list updated");
                    return result;
                }
                catch (HazelcastInstanceNotActiveException)
                {
                }
                catch (Exception e)
                {
                    Logger.Warning("Error when getting list of partitions", e);
                }
                finally
                {
                    _updating.Set(false);
                }
            }
            return false;
        }
        
        private ClientGetPartitionsCodec.ResponseParameters GetPartitionsFrom(ClientConnection connection)
        {
            var request = ClientGetPartitionsCodec.EncodeRequest();
            var task = ((ClientInvocationService)_client.GetInvocationService()).InvokeOnConnection(request, connection);
            var result = ThreadUtil.GetResult(task, PartitionTimeout);
            return ClientGetPartitionsCodec.DecodeResponse(result);
        }

        private bool ProcessPartitionResponse(ClientGetPartitionsCodec.ResponseParameters response)
        {
            var partitionResponse = response.partitions;
            foreach (var entry in partitionResponse)
            {
                var address = entry.Key;
                foreach (var partition in entry.Value)
                {
                    _partitions.AddOrUpdate(partition, address, (p,a) => address);
                }
            }
            _partitionCount = _partitions.Count;
            return _partitionCount > 0;
        }

//        TODO: will be useful when the ClientPartitionServiceProxy is implemeneted 
//        internal class Partition : IPartition
//        {
//            private readonly HazelcastClient _client;
//            private readonly int _partitionId;
//
//            public Partition(HazelcastClient client, int partitionId)
//            {
//                _client = client;
//                _partitionId = partitionId;
//            }
//
//            public int GetPartitionId()
//            {
//                return _partitionId;
//            }
//
//            public IMember GetOwner()
//            {
//                var owner = _client.GetPartitionService().GetPartitionOwner(_partitionId);
//                if (owner != null)
//                {
//                    return _client.GetClientClusterService().GetMember(owner);
//                }
//                return null;
//            }
//
//            public override string ToString()
//            {
//                var sb = new StringBuilder("PartitionImpl{");
//                sb.Append("partitionId=").Append(_partitionId);
//                sb.Append('}');
//                return sb.ToString();
//            }
//        }

        #region IClientPartitionService

        public Address GetPartitionOwner(int partitionId)
        {
            Address rtn;
            _partitions.TryGetValue(partitionId, out rtn);
            return rtn;
        }

        internal int GetPartitionId(IData key)
        {
            var pc = GetPartitionCount();
            if (pc <= 0)
            {
                return 0;
            }
            var hash = key.GetPartitionHash();
            return (hash == int.MinValue) ? 0 : Math.Abs(hash)%pc;
        }

        public int GetPartitionId(object key)
        {
            var data = _client.GetSerializationService().ToData(key);
            return GetPartitionId(data);
        }

        public int GetPartitionCount()
        {
            if (_partitionCount == 0)
            {
                GetPartitionsBlocking();
            }
            return _partitionCount;
        }

        private void GetPartitionsBlocking()
        {
            while (!GetPartitions() && _isLive)
            {
                Thread.Sleep(PartitionRefreshPeriod);
            }
        }

        #endregion

    }
}