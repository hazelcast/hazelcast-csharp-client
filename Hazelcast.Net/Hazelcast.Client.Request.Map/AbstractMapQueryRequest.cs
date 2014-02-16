using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Client.Request.Map
{
    public abstract class AbstractMapQueryRequest : ClientRequest, IRetryableRequest
    {
        private IterationType iterationType;
        private string name;

        protected AbstractMapQueryRequest()
        {
        }

        protected AbstractMapQueryRequest(string name, IterationType iterationType)
        {
            this.name = name;
            this.iterationType = iterationType;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("t", iterationType.ToString().ToUpper());
            WritePortableInner(writer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WritePortableInner(IPortableWriter writer);

    }
}