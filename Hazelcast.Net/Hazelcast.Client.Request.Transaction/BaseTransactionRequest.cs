using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Transaction
{
    internal abstract class BaseTransactionRequest : ClientRequest
    {
        protected long clientThreadId;
        protected String txnId;

        public string TxnId
        {
            get { return txnId; }
            set { txnId = value; }
        }

        public long ClientThreadId
        {
            get { return clientThreadId; }
            set { clientThreadId = value; }
        }

        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("tId", txnId);
            writer.WriteLong("cti", clientThreadId);
        }
    }
}