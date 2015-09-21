using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            GetInitialPartitions();
            _partitionThread = new Thread(RefreshPartitionsWithFixedDelay) {IsBackground = true};
            _partitionThread.Start();
        }

        public void Stop()
        {
            try
            {
                _isLive = false;
                _partitionThread.Interrupt();
            }
            catch (Exception e)
            {
                Logger.Finest("Shut down partition refresher thread problem...", e);
            }
            _partitions.Clear();
        }

        public void RefreshPartitions()
        {
            if (_partitionThread != null)
            {
                _partitionThread.Interrupt();
            }
        }

        private void RefreshPartitionsWithFixedDelay()
        {
            while (_isLive)
            {
                try
                {
                    __RefreshPartitions();
                    Thread.Sleep(10000);
                }
                catch (ThreadInterruptedException)
                {
                    Logger.Finest("Partition Refresher thread wakes up");
                }
            }
        }

        private void __RefreshPartitions()
        {
            Logger.Finest("Refresh Partitions at " + DateTime.Now.ToLocalTime());
            if (_updating.CompareAndSet(false, true))
            {
                try
                {
                    var clusterService = _client.GetClientClusterService();
                    var master = clusterService.GetMasterAddress();
                    var response = GetPartitionsFrom(master);
                    if (response != null)
                    {
                        ProcessPartitionResponse(response);
                    }
                }
                catch (HazelcastInstanceNotActiveException)
                {
                }
                catch (Exception e)
                {
                    Logger.Warning(e);
                }
                finally
                {
                    _updating.Set(false);
                }
            }
        }

        private void GetInitialPartitions()
        {
            var clusterService = _client.GetClientClusterService();
            var memberList = clusterService.GetMemberList();
            foreach (var member in memberList)
            {
                var target = member.GetAddress();
                if (target == null)
                {
                    Logger.Severe("Address cannot be null");
                }
                var response = GetPartitionsFrom(target);
                if (response != null)
                {
                    ProcessPartitionResponse(response);

                    return;
                }
            }
            throw new InvalidOperationException("Cannot get initial partitions!");
        }

        private ClientGetPartitionsCodec.ResponseParameters GetPartitionsFrom(Address address)
        {
            try
            {
                var request = ClientGetPartitionsCodec.EncodeRequest();
                var task = _client.GetInvocationService().InvokeOnTarget(request, address);
                var result = ThreadUtil.GetResult(task, 150000);
                return ClientGetPartitionsCodec.DecodeResponse(result);
            }
            catch (TimeoutException e)
            {
                Logger.Finest("Operation timed out while fetching cluster partition table!", e);
            }
            catch (Exception e)
            {
                Logger.Severe("Error while fetching cluster partition table!", e);
            }
            return null;
        }

        private void ProcessPartitionResponse(ClientGetPartitionsCodec.ResponseParameters response)
        {
            var partitionResponse = response.partitions;
            _partitions.Clear();
            foreach (var entry in partitionResponse)
            {
                var address = entry.Key;
                foreach (var partition in entry.Value)
                {
                    _partitions.TryAdd(partition, address);
                }
            }
            _partitionCount = _partitions.Count;
        }

        internal class Partition : IPartition
        {
            private readonly HazelcastClient _client;
            private readonly int _partitionId;

            public Partition(HazelcastClient client, int partitionId)
            {
                _client = client;
                _partitionId = partitionId;
            }

            public int GetPartitionId()
            {
                return _partitionId;
            }

            public IMember GetOwner()
            {
                var owner = _client.GetPartitionService().GetPartitionOwner(_partitionId);
                if (owner != null)
                {
                    return _client.GetClientClusterService().GetMember(owner);
                }
                return null;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("PartitionImpl{");
                sb.Append("partitionId=").Append(_partitionId);
                sb.Append('}');
                return sb.ToString();
            }
        }

        #region IClientPartitionService

        public Address GetPartitionOwner(int partitionId)
        {
            Address rtn;
            _partitions.TryGetValue(partitionId, out rtn);
            return rtn;
        }

        internal int GetPartitionId(IData key)
        {
            var pc = _partitionCount;
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
            return _partitionCount;
        }

        #endregion
    }
}