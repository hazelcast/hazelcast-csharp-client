using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Client.Request.Map
{
    //public sealed class MapSQLQueryRequest : AbstractMapQueryRequest
    //{
    //    private string sql;

    //    public MapSQLQueryRequest()
    //    {
    //    }

    //    public MapSQLQueryRequest(string name, string sql, IterationType iterationType) : base(name, iterationType)
    //    {
    //        this.sql = sql;
    //    }

    //    public override int GetClassId()
    //    {
    //        return MapPortableHook.SqlQuery;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    protected internal override void WritePortableInner(IPortableWriter writer)
    //    {
    //        writer.WriteUTF("sql", sql);
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    protected internal override void ReadPortableInner(IPortableReader reader)
    //    {
    //        sql = reader.ReadUTF("sql");
    //    }
    //}
}