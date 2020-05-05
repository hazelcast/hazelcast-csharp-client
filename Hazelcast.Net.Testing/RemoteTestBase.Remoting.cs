using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Eventing;
using Hazelcast.Networking;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    public abstract partial class RemoteTestBase
    {
        protected IRemoteController CreateRemoteController()
        {
            try
            {
#if NETFRAMEWORK
                var transport = new Thrift.Transport.TFramedTransport(new Thrift.Transport.TSocket("localhost", 9701));
                transport.Open();
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return new ThreadSafeRemoteController(protocol);
#else
                var rcHostAddress = NetworkAddress.GetIPAddressByName("localhost");
                var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, 9701);
                var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
                if (!transport.IsOpen)
                {
                    transport.OpenAsync().Wait();
                }
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return new ThreadSafeRemoteController(protocol);
#endif
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "Cannot start Remote Controller");
                throw new AssertionException("Cannot start Remote Controller", e);
            }
        }

        protected void StopRemoteController(IRemoteController client)
        {
            client?.exit();
            ((RemoteController.Client)client)?.InputProtocol?.Transport?.Close();
        }

        protected virtual Remote.Cluster CreateCluster(IRemoteController remoteController)
        {
            Logger.LogInformation("Creating cluster");
            var cluster = remoteController.createCluster(null, Remote.Resources.hazelcast);
            Logger.LogInformation("Created cluster");
            return cluster;
        }

        protected virtual Remote.Cluster CreateCluster(IRemoteController remoteController, string xmlconfig)
        {
            Logger.LogInformation("Creating cluster using custom config...");
            var cluster = remoteController.createCluster(null, xmlconfig);
            Logger.LogInformation("Created cluster");
            return cluster;
        }

        protected virtual void ResumeMember(IRemoteController remoteController, Remote.Cluster cluster, Member member)
        {
            remoteController.resumeMember(cluster.Id, member.Uuid);
        }

        protected virtual Member StartMember(IRemoteController remoteController, Remote.Cluster cluster)
        {
            Logger.LogInformation("Starting new member");
            return remoteController.startMember(cluster.Id);
        }

        protected async Task<Member> StartMemberAndWait(IHazelcastClient client, IRemoteController remoteController, Remote.Cluster cluster,
            int expectedSize)
        {
            var clientInternal = (HazelcastClient) client;
            var added = false;

            void HandleEvent(MembershipEventArgs args)
            {
                if (args.EventType == MembershipEventType.Added)
                    added = true;
            }

            clientInternal.Cluster.MemberAddedOrRemoved.Add(HandleEvent);

            var member = StartMember(remoteController, cluster);
            Assert.Eventually(() =>
            {
                if (!added) throw new Exception("Member was not added.");
            }, 120);

            clientInternal.Cluster.MemberAddedOrRemoved.Remove(HandleEvent);

            // make sure partitions are updated
            await Assert.Eventually(async () =>
            {
                var count = await GetUniquePartitionOwnerCountAsync(client);
                Assert.AreEqual(expectedSize, count);
            }, 60);

            return member;
        }

        protected virtual bool StopCluster(IRemoteController remoteController, Remote.Cluster cluster)
        {
            return remoteController.shutdownCluster(cluster.Id);
        }

        protected void ShutdownCluster(IRemoteController remoteController, Remote.Cluster cluster)
        {
            while (!StopCluster(remoteController, cluster))
            {
                Thread.Sleep(1000);
            }
        }

        protected virtual void StopMember(IRemoteController remoteController, Remote.Cluster cluster, Member member)
        {
            Logger.LogInformation("Shutting down  member " + member.Uuid);
            remoteController.shutdownMember(cluster.Id, member.Uuid);
        }

        protected async Task StopMemberAndWait(IHazelcastClient client, IRemoteController remoteController, Remote.Cluster cluster,
            Member member)
        {
            var clientInternal = (HazelcastClient) client;

            var removed = false;

            void HandleEvent(MembershipEventArgs args)
            {
                if (args.EventType == MembershipEventType.Removed)
                    removed = true;
            }

            clientInternal.Cluster.MemberAddedOrRemoved.Add(HandleEvent);

            StopMember(remoteController, cluster, member);
            Assert.Eventually(() =>
            {
                if (!removed) throw new Exception("Member was not removed.");
            }, 120);

            clientInternal.Cluster.MemberAddedOrRemoved.Remove(HandleEvent);
        }

        protected virtual void SuspendMember(IRemoteController remoteController, Remote.Cluster cluster, Member member)
        {
            remoteController.suspendMember(cluster.Id, member.Uuid);
        }
    }
}
