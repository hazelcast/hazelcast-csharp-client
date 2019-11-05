// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using Hazelcast.Core;

namespace Hazelcast.Client
{
    internal class Client : IClient
    {
        private readonly IPEndPoint _socketAddress;
        private readonly string _uuid;

        public Client(string uuid, IPEndPoint socketAddress)
        {
            _uuid = uuid;
            _socketAddress = socketAddress;
        }

        public virtual Guid Uuid
        {
            get { return _uuid; }
        }

        public virtual IPEndPoint GetSocketAddress()
        {
            return _socketAddress;
        }

        public virtual ClientType GetClientType()
        {
            return ClientType.Csharp;
        }
    }
}