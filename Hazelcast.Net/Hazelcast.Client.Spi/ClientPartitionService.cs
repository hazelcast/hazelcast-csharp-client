using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Hazelcast.Client.Request.Partition;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Spi
{
    public sealed class ClientPartitionService : IClientPartitionService
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (IClientPartitionService));

        private readonly HazelcastClient client;

        private readonly ConcurrentDictionary<int, Address> partitions = new ConcurrentDictionary<int, Address>();
        //271, 0.75f, 1);

        private readonly AtomicBoolean updating = new AtomicBoolean(false);

        private volatile int partitionCount;

        public ClientPartitionService(HazelcastClient client)
        {
            this.client = client;
        }

        public Address GetPartitionOwner(int partitionId)
        {
            Address rtn;
            partitions.TryGetValue(partitionId, out rtn);
            return rtn;
        }

        public int GetPartitionId(Data key)
        {
            int pc = partitionCount;
            if (pc <= 0)
            {
                return 0;
            }
            int hash = key.GetPartitionHash();
            return (hash == int.MinValue) ? 0 : Math.Abs(hash)%pc;
        }

        public int GetPartitionId(object key)
        {
            Data data = client.GetSerializationService().ToData(key);
            return GetPartitionId(data);
        }

        public int GetPartitionCount()
        {
            return partitionCount;
        }

        public void Start()
        {
            GetInitialPartitions();

            var refreshThread = new Thread(RefreshPartitionsWithFixedDelay) {IsBackground = true};
            refreshThread.Start();
        }

        public void RefreshPartitions()
        {
            try
            {
                client.GetClientExecutionService().Submit(__RefreshPartitions);
            }
            catch (Exception e)
            {
            }
        }

        private void RefreshPartitionsWithFixedDelay()
        {
            try
            {
                while (Thread.CurrentThread.IsAlive)
                {
                    __RefreshPartitions();
                    Thread.Sleep(10000);
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void __RefreshPartitions()
        {
            Debug.WriteLine("Refresh Partitions at " + DateTime.Now.ToLocalTime());

            if (updating.CompareAndSet(false, true))
            {
                try
                {
                    IClientClusterService clusterService = client.GetClientClusterService();
                    Address master = clusterService.GetMasterAddress();
                    PartitionsResponse response = GetPartitionsFrom((ClientClusterService) clusterService, master);
                    if (response != null)
                    {
                        ProcessPartitionResponse(response);
                    }
                }
                catch (HazelcastInstanceNotActiveException)
                {
                }
                finally
                {
                    updating.Set(false);
                }
            }
        }

        private void GetInitialPartitions()
        {
            IClientClusterService clusterService = client.GetClientClusterService();
            ICollection<IMember> memberList = clusterService.GetMemberList();
            foreach (IMember member in memberList)
            {
                Address target = member.GetAddress();
                PartitionsResponse response = GetPartitionsFrom((ClientClusterService) clusterService, target);
                if (response != null)
                {
                    ProcessPartitionResponse(response);
                    return;
                }
            }
            throw new InvalidOperationException("Cannot get initial partitions!");
        }

        private PartitionsResponse GetPartitionsFrom(ClientClusterService clusterService, Address address)
        {
            try
            {
                return clusterService.SendAndReceive<object>(address, new GetPartitionsRequest()) as PartitionsResponse;
            }
            catch (IOException e)
            {
                logger.Severe("Error while fetching cluster partition table!", e);
            }
            return null;
        }

        private void ProcessPartitionResponse(PartitionsResponse response)
        {
            Address[] members = response.GetMembers();
            int[] ownerIndexes = response.GetOwnerIndexes();
            if (partitionCount == 0)
            {
                partitionCount = ownerIndexes.Length;
            }
            for (int partitionId = 0; partitionId < partitionCount; partitionId++)
            {
                int ownerIndex = ownerIndexes[partitionId];
                if (ownerIndex > -1)
                {
                    partitions.TryAdd(partitionId, members[ownerIndex]);
                }
            }
        }

        public void Stop()
        {
            partitions.Clear();
        }
    }
}