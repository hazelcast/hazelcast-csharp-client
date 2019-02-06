using System.Linq;
using System.Threading;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture("AddAndGet")]
    [TestFixture("Get")]
    [TestFixture("DriverCanContinueSessionByCallingReset")]
    public class ClientPNCounterConsistencyLossTest : MultiMemberBaseTest
    {
        private readonly string _type;

        public ClientPNCounterConsistencyLossTest(string type)
        {
            _type = type;
        }

        private ClientPNCounterProxy _pnCounter;

        [SetUp]
        public void Setup()
        {
            _pnCounter = Client.GetPNCounter(TestSupport.RandomString()) as ClientPNCounterProxy;
        }

        protected override void InitMembers()
        {
            //Init 2 members
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetConnectionAttemptLimit(1);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast_quick_node_switching;
        }

        [Test]
        public void ConsistencyLostExceptionIsThrownWhenTargetReplicaDisappears()
        {
            _pnCounter.AddAndGet(5);
            Assert.AreEqual(5, _pnCounter.Get());

            TerminateTargetReplicaMember();
            Thread.Sleep(1000);

            Mutation();
        }

        private void Mutation()
        {
            switch (_type)
            {
                case "AddAndGet":
                    Assert.Throws<ConsistencyLostException>(() => _pnCounter.AddAndGet(5));
                    break;

                case "Get":
                    Assert.Throws<ConsistencyLostException>(() => _pnCounter.Get());
                    break;

                case "DriverCanContinueSessionByCallingReset":
                    _pnCounter.Reset();
                    _pnCounter.AddAndGet(5);
                    break;
            }
        }

        private void TerminateTargetReplicaMember()
        {
            // Shutdown "primary" member
            var allMembers = Client.GetCluster().GetMembers();
            var currentTarget = _pnCounter._currentTargetReplicaAddress;
            var primaryMember = allMembers.First(x => x.GetAddress().Equals(currentTarget));

            RemoteController.terminateMember(HzCluster.Id, primaryMember.GetUuid());
        }
    }
}