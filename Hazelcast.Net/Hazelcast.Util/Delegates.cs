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

﻿using System;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    public delegate TE FactoryMethod<out TE>();

    public delegate TE DestructorMethod<TE>(TE e);

    public delegate V ConstructorMethod<K, V>(K arg);

    public delegate void Runnable();

    public delegate T Callable<T>();


    /// <exception cref="System.IO.IOException"></exception>
    internal delegate ClientConnection NewConnection(Address address);

    internal delegate void Authenticator(ClientConnection connection);

    public delegate void DistributedEventHandler(IClientMessage eventMessage);

    public delegate string DecodeStartListenerResponse(IClientMessage requestMessage);
    public delegate bool DecodeStopListenerResponse(IClientMessage requestMessage);
    public delegate IClientMessage EncodeStopListenerRequest(string registrationId);

}