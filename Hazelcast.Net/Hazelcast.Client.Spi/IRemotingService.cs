using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IRemotingService
    {

        Task<IClientMessage> Send(IClientMessage request, IMember target, int partitionId = -1, DistributedEventHandler handler = null);
        Task<IClientMessage> Send(IClientMessage request, Address target, int partitionId = -1, DistributedEventHandler handler = null);
        Task<IClientMessage> Send(IClientMessage request, DistributedEventHandler handler = null);

        void RegisterListener(string registrationId, int callId);
        bool UnregisterListener(string registrationId);
        void ReRegisterListener(string uuidregistrationId, string alias, int callId);

    }
}
