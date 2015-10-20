/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Proxy
{
    internal class ClientIdGeneratorProxy : ClientProxy, IIdGenerator
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
                    residue.Set((int) (id%BlockSize) + 1);
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