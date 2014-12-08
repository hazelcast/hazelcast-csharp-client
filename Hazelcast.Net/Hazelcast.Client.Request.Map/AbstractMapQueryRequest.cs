using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Client.Request.Map
{
    internal abstract class AbstractMapQueryRequest : ClientRequest, IRetryableRequest
    {
        private IterationType iterationType;
        private string name;

        protected AbstractMapQueryRequest(string name, IterationType iterationType)
        {
            this.name = name;
            this.iterationType = iterationType;
        }

        public sealed override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("t", iterationType.ToString());
            WritePortableInner(writer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WritePortableInner(IPortableWriter writer);

    }
}