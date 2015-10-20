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

using Hazelcast.Client.Connection;

namespace Hazelcast.Config
{
    public class SocketOptions
    {
        private int bufferSize = 32;
        private bool keepAlive = true;

        private int lingerSeconds = 3;
        private bool reuseAddress = true;

        private ISocketFactory socketFactory;
        private bool tcpNoDelay;
        private int timeout = -1;

        // socket options
        // in kb
        public virtual bool IsTcpNoDelay()
        {
            return tcpNoDelay;
        }

        public virtual SocketOptions SetTcpNoDelay(bool tcpNoDelay)
        {
            this.tcpNoDelay = tcpNoDelay;
            return this;
        }

        public virtual bool IsKeepAlive()
        {
            return keepAlive;
        }

        public virtual SocketOptions SetKeepAlive(bool keepAlive)
        {
            this.keepAlive = keepAlive;
            return this;
        }

        public virtual bool IsReuseAddress()
        {
            return reuseAddress;
        }

        public virtual SocketOptions SetReuseAddress(bool reuseAddress)
        {
            this.reuseAddress = reuseAddress;
            return this;
        }

        public virtual int GetLingerSeconds()
        {
            return lingerSeconds;
        }

        public virtual SocketOptions SetLingerSeconds(int lingerSeconds)
        {
            this.lingerSeconds = lingerSeconds;
            return this;
        }

        public virtual int GetTimeout()
        {
            return timeout;
        }

        public virtual SocketOptions SetTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public virtual int GetBufferSize()
        {
            return bufferSize;
        }

        public virtual SocketOptions SetBufferSize(int bufferSize)
        {
            this.bufferSize = bufferSize;
            return this;
        }

        public virtual ISocketFactory GetSocketFactory()
        {
            return socketFactory;
        }

        public virtual SocketOptions SetSocketFactory(ISocketFactory socketFactory)
        {
            this.socketFactory = socketFactory;
            return this;
        }
    }
}