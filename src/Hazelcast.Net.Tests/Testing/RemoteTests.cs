using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class RemoteTests : RemoteTestBase
    {
        [Test]
        public async Task CanStartAndShutdownRemoteController()
        {
            var rc = await CreateRemoteControllerAsync();

            // we have a remote controller

            await rc.ExitAsync();
        }

        [Test]
        public async Task CanStartAndShutdownCluster()
        {
            var rc = await CreateRemoteControllerAsync();
            var cluster = await rc.CreateClusterAsync(Resources.Cluster_Default);

            // we have a cluster

            await rc.ShutdownClusterAsync(cluster.Id);
            await rc.ExitAsync();
        }

        [Test]
        public async Task CanStartAndShutdownMember()
        {
            var rc = await CreateRemoteControllerAsync();
            var cluster = await rc.CreateClusterAsync();
            var member = await rc.StartMemberAsync(cluster.Id);

            // we have a member

            await rc.ShutdownMemberAsync(cluster.Id, member.Uuid);
            await rc.ShutdownClusterAsync(cluster.Id);
            await rc.ExitAsync();
        }
    }
}
