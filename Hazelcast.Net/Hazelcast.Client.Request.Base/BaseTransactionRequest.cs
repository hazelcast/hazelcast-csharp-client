using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Base
{
    internal abstract class BaseTransactionRequest : ClientRequest
	{
		protected internal string txnId;

		protected internal long clientThreadId;

		public virtual void SetTxnId(string txnId)
		{
			this.txnId = txnId;
		}

		public virtual void SetClientThreadId(long clientThreadId)
		{
			this.clientThreadId = clientThreadId;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("tId", txnId);
			writer.WriteLong("cti", clientThreadId);
		}

	}
}
