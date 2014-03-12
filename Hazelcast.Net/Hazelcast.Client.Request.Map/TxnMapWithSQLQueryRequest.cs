using Hazelcast.IO;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    //internal class TxnMapWithSQLQueryRequest : AbstractTxnMapRequest
    //{
    //    internal string predicate;


    //    public TxnMapWithSQLQueryRequest(string name, TxnMapRequestType requestType, string predicate)
    //        : base(name, requestType, null, null, null)
    //    {
    //        this.predicate = predicate;
    //    }

    //    public override int GetClassId()
    //    {
    //        return MapPortableHook.TxnRequestWithSqlQuery;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    protected internal override void WriteDataInner(IObjectDataOutput output)
    //    {
    //        if (predicate != null)
    //        {
    //            output.WriteBoolean(true);
    //            output.WriteUTF(predicate);
    //        }
    //        else
    //        {
    //            output.WriteBoolean(false);
    //        }
    //    }

    //}
}