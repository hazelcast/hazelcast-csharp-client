using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Transaction
{

    internal abstract class BaseTransactionRequest : ClientRequest
    {

        protected String txnId;
        protected long clientThreadId;

        public override bool Sticky
        {
            get { return true; }
        }

        protected BaseTransactionRequest() {
        }

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

        protected override void BaseWritePortable(IPortableWriter writer)
        {
            base.BaseWritePortable(writer);
            writer.WriteUTF("tId", txnId);
            writer.WriteLong("cti", clientThreadId);
        }

    }

}
