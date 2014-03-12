using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapSizeRequest : TxnMultiMapRequest
    {
        public TxnMultiMapSizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmSize;
        }
    }
}