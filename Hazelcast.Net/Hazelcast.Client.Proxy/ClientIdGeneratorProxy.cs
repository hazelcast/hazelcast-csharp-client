// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Proxy
{
    internal class ClientIdGeneratorProxy : ClientProxy, IIdGenerator
    {
        private const int BlockSize = 10000;

        private readonly IAtomicLong _atomicLong;

        private readonly AtomicLong _local;
        private readonly AtomicInteger _residue;

        public ClientIdGeneratorProxy(string serviceName, string objectId, IAtomicLong atomicLong)
            : base(serviceName, objectId)
        {
            _atomicLong = atomicLong;
            _residue = new AtomicInteger(BlockSize);
            _local = new AtomicLong(-1);
        }

        public virtual bool Init(long id)
        {
            if (id <= 0)
            {
                return false;
            }
            var step = (id/BlockSize);
            lock (this)
            {
                var init = _atomicLong.CompareAndSet(0, step + 1);
                if (init)
                {
                    _local.Set(step);
                    _residue.Set((int) (id%BlockSize) + 1);
                }
                return init;
            }
        }

        public virtual long NewId()
        {
            var value = _residue.GetAndIncrement();
            if (value >= BlockSize)
            {
                lock (this)
                {
                    value = _residue.Get();
                    if (value >= BlockSize)
                    {
                        _local.Set(_atomicLong.GetAndIncrement());
                        _residue.Set(0);
                    }
                    return NewId();
                }
            }
            return _local.Get()*BlockSize + value;
        }

        protected override void OnDestroy()
        {
            _atomicLong.Destroy();
        }
    }
}