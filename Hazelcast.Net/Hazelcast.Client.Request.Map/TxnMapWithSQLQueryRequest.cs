using Hazelcast.Client.Request.Map;
using Hazelcast.IO;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class TxnMapWithSQLQueryRequest : AbstractTxnMapRequest
	{
		internal string predicate;

		public TxnMapWithSQLQueryRequest()
		{
		}

		public TxnMapWithSQLQueryRequest(string name, AbstractTxnMapRequest.TxnMapRequestType requestType, string predicate) : base(name, requestType, null, null, null)
		{
			this.predicate = predicate;
		}

		public override int GetClassId()
		{
			return MapPortableHook.TxnRequestWithSqlQuery;
		}

		/// <exception cref="System.IO.IOException"></exception>
		protected internal override void WriteDataInner(IObjectDataOutput output)
		{
			if (predicate != null)
			{
				output.WriteBoolean(true);
				output.WriteUTF(predicate);
			}
			else
			{
				output.WriteBoolean(false);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		protected internal override void ReadDataInner(IObjectDataInput input)
		{
			bool hasPredicate = input.ReadBoolean();
			if (hasPredicate)
			{
				predicate = input.ReadUTF();
			}
		}
	}
}
