// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// TCP Socket options
    /// </summary>
    public class SocketOptions
    {
        private int _bufferSize = 32;
        private bool _keepAlive = true;

        private int _lingerSeconds = 3;
        private bool _reuseAddress = true;

        private bool _tcpNoDelay;
        private int _timeout = -1;

        public virtual int GetBufferSize()
        {
            return _bufferSize;
        }

        public virtual int GetLingerSeconds()
        {
            return _lingerSeconds;
        }

        public virtual int GetTimeout()
        {
            return _timeout;
        }

        public virtual bool IsKeepAlive()
        {
            return _keepAlive;
        }

        public virtual bool IsReuseAddress()
        {
            return _reuseAddress;
        }

        // socket options
        // in kb
        public virtual bool IsTcpNoDelay()
        {
            return _tcpNoDelay;
        }

        public virtual SocketOptions SetBufferSize(int bufferSize)
        {
            _bufferSize = bufferSize;
            return this;
        }

        public virtual SocketOptions SetKeepAlive(bool keepAlive)
        {
            _keepAlive = keepAlive;
            return this;
        }

        public virtual SocketOptions SetLingerSeconds(int lingerSeconds)
        {
            _lingerSeconds = lingerSeconds;
            return this;
        }

        public virtual SocketOptions SetReuseAddress(bool reuseAddress)
        {
            _reuseAddress = reuseAddress;
            return this;
        }

        public virtual SocketOptions SetTcpNoDelay(bool tcpNoDelay)
        {
            _tcpNoDelay = tcpNoDelay;
            return this;
        }

        public virtual SocketOptions SetTimeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }
    }
}