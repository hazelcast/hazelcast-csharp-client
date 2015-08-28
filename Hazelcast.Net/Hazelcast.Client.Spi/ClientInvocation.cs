using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    class ClientInvocation
    {
        private readonly DistributedEventHandler _handler;
        private readonly string _memberUuid;
        private readonly int _partitionId;
        private readonly IClientMessage _message;
        private int _retryCount = 0;
        private readonly TaskCompletionSource<IClientMessage> _response = new TaskCompletionSource<IClientMessage>();
        private readonly object _lock = new object();

        public ClientInvocation(IClientMessage message, int partitionId = -1, 
            string memberUuid = null, DistributedEventHandler handler = null)
        {
            _handler = handler;
            _memberUuid = memberUuid;
            _partitionId = partitionId;
            _message = message;
        }

        public DistributedEventHandler Handler
        {
            get { return _handler; }
        }

        public string MemberUuid
        {
            get { return _memberUuid; }
        }

        public int PartitionId
        {
            get { return _partitionId; }
        }

        public IClientMessage Message
        {
            get { return _message; }
        }

        public TaskCompletionSource<IClientMessage> Response
        {
            get { return _response; }
        }

        public int IncrementAndGetRetryCount()
        {
            return Interlocked.Increment(ref _retryCount);
        }
    }
}
