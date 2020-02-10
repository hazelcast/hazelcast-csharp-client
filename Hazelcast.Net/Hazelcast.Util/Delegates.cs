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
using System.Collections.Generic;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.IO;

namespace Hazelcast.Util
{
    public delegate void Runnable();

    public delegate T Callable<T>();

    internal delegate void Authenticator(Connection connection);

    internal delegate void DistributedEventHandler(ClientMessage eventMessage);

    internal delegate Guid DecodeRegisterResponse(ClientMessage requestMessage);

    internal delegate ClientMessage EncodeDeregisterRequest(Guid registrationId);
    
    internal delegate IDictionary<Address, Address> GetAddressDictionary();
}