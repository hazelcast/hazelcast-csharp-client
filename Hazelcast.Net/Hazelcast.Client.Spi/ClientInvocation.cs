using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientInvocation
    {
        private readonly string _memberUuid;
        private readonly IClientMessage _message;
        private readonly int _partitionId;
        private readonly SettableFuture<IClientMessage> _future;

        private int _retryCount;

        public ClientInvocation(IClientMessage message, int partitionId = -1,
            string memberUuid = null)
        {
            _memberUuid = memberUuid;
            _partitionId = partitionId;
            _message = message;
            _future = new SettableFuture<IClientMessage>();
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

        public int IncrementAndGetRetryCount()
        {
            return Interlocked.Increment(ref _retryCount);
        }

        public SettableFuture<IClientMessage> Future
        {
            get { return _future; }
        }

        /// <summary>
        /// Connection that was used to execute this invocation
        /// </summary>
        public ClientConnection SentConnection { get; set; }
    }
}