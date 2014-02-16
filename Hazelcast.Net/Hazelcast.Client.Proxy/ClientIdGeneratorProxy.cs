using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Proxy
{
    public class ClientIdGeneratorProxy : ClientProxy, IIdGenerator
    {
        private const int BlockSize = 10000;

        internal readonly IAtomicLong atomicLong;
        internal readonly string name;

        internal AtomicLong local;
        internal AtomicInteger residue;

        public ClientIdGeneratorProxy(string serviceName, string objectId, IAtomicLong atomicLong)
            : base(serviceName, objectId)
        {
            this.atomicLong = atomicLong;
            name = objectId;
            residue = new AtomicInteger(BlockSize);
            local = new AtomicLong(-1);
        }

        public virtual bool Init(long id)
        {
            if (id <= 0)
            {
                return false;
            }
            long step = (id/BlockSize);
            lock (this)
            {
                bool init = atomicLong.CompareAndSet(0, step + 1);
                if (init)
                {
                    local.Set(step);
                    residue.Set((int) (id % BlockSize) + 1);
                }
                return init;
            }
        }

        public virtual long NewId()
        {
            int value = residue.GetAndIncrement();
            if (value >= BlockSize)
            {
                lock (this)
                {
                    value = residue.Get();
                    if (value >= BlockSize)
                    {
                        local.Set(atomicLong.GetAndIncrement());
                        residue.Set(0);
                    }
                    return NewId();
                }
            }
            return local.Get()*BlockSize + value;
        }

        protected override void OnDestroy()
        {
            atomicLong.Destroy();
            residue = null;
            local = null;
        }
    }
}