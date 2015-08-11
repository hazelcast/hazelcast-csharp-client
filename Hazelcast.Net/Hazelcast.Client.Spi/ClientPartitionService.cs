using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientPartitionService : IClientPartitionService
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (IClientPartitionService));
        private readonly HazelcastClient client;
        private readonly ConcurrentDictionary<int, Address> partitions = new ConcurrentDictionary<int, Address>();
        private readonly AtomicBoolean updating = new AtomicBoolean(false);
        private volatile int partitionCount;
        private Thread partitionThread;

        public ClientPartitionService(HazelcastClient client)
        {
            this.client = client;
        }

        public void Start()
        {
            GetInitialPartitions();

            partitionThread = new Thread(RefreshPartitionsWithFixedDelay) {IsBackground = true};
            partitionThread.Start();
        }

        public void Stop()
        {
            try
            {
                partitionThread.Abort();
            }
            catch (Exception e)
            {
                logger.Finest("Shut down partition refresher thread problem...");
            }
            partitions.Clear();
        }

        public void RefreshPartitions()
        {
            partitionThread.Interrupt();
        }

        private void RefreshPartitionsWithFixedDelay()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                try
                {
                    __RefreshPartitions();
                    Thread.Sleep(10000);
                }
                catch (ThreadInterruptedException)
                {
                    logger.Finest("Partition Refresher thread wakes up");
                }
            }
        }

        private void __RefreshPartitions()
        {
            logger.Finest("Refresh Partitions at " + DateTime.Now.ToLocalTime());
            if (updating.CompareAndSet(false, true))
            {
                try
                {
                    var clusterService = client.GetClientClusterService();
                    var master = clusterService.GetMasterAddress();
                    var response = GetPartitionsFrom((ClientClusterService) clusterService, master);
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
                    logger.Warning(e);
                }
                finally
                {
                    updating.Set(false);
                }
            }
        }

        private void GetInitialPartitions()
        {
            var clusterService = client.GetClientClusterService();
            var memberList = clusterService.GetMemberList();
            foreach (var member in memberList)
            {
                var target = member.GetAddress();
                if (target == null)
                {
                    logger.Severe("Address cannot be null");
                }
                var response = GetPartitionsFrom((ClientClusterService) clusterService, target);
                if (response != null)
                {
                    ProcessPartitionResponse(response);

                    return;
                }
            }
            throw new InvalidOperationException("Cannot get initial partitions!");
        }

        private ClientGetPartitionsCodec.ResponseParameters GetPartitionsFrom(ClientClusterService clusterService,
            Address address)
        {
            try
            {
                var request = ClientGetPartitionsCodec.EncodeRequest();
                var task = client.GetInvocationService().InvokeOnTarget(request, address);
                var result = ThreadUtil.GetResult(task, 150000);
                return ClientGetPartitionsCodec.DecodeResponse(result);
            }
            catch (TimeoutException e)
            {
                logger.Finest("Operation timed out while fetching cluster partition table!", e);
            }
            catch (Exception e)
            {
                logger.Severe("Error while fetching cluster partition table!", e);
            }
            return null;
        }

        private void ProcessPartitionResponse(ClientGetPartitionsCodec.ResponseParameters response)
        {
            var partitionResponse = response.partitions;
            partitions.Clear();
            foreach (var entry in partitionResponse)
            {
                var address = entry.Key;
                foreach (var partition in entry.Value)
                {
                    partitions.TryAdd(partition, address);
                }
            }
            partitionCount = partitions.Count;
        }

        internal class Partition : IPartition
        {
            private readonly HazelcastClient client;
            private readonly int partitionId;

            public Partition(HazelcastClient client, int partitionId)
            {
                this.client = client;
                this.partitionId = partitionId;
            }

            public int GetPartitionId()
            {
                return partitionId;
            }

            public IMember GetOwner()
            {
                var owner = client.GetPartitionService().GetPartitionOwner(partitionId);
                if (owner != null)
                {
                    return client.GetClientClusterService().GetMember(owner);
                }
                return null;
            }

            public override string ToString()
            {
                var sb = new StringBuilder("PartitionImpl{");
                sb.Append("partitionId=").Append(partitionId);
                sb.Append('}');
                return sb.ToString();
            }
        }

        #region IClientPartitionService

        public Address GetPartitionOwner(int partitionId)
        {
            Address rtn;
            partitions.TryGetValue(partitionId, out rtn);
            return rtn;
        }

        internal int GetPartitionId(IData key)
        {
            var pc = partitionCount;
            if (pc <= 0)
            {
                return 0;
            }
            var hash = key.GetPartitionHash();
            return (hash == int.MinValue) ? 0 : Math.Abs(hash)%pc;
        }

        public int GetPartitionId(object key)
        {
            var data = client.GetSerializationService().ToData(key);
            return GetPartitionId(data);
        }

        public int GetPartitionCount()
        {
            return partitionCount;
        }

        #endregion
    }
}