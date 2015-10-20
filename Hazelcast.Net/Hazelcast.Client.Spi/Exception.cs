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
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    internal class RetryableHazelcastException : HazelcastException
    {
        public RetryableHazelcastException()
        {
        }

        public RetryableHazelcastException(String message) : base(message)
        {
        }
    }

    internal class TargetNotMemberException : RetryableHazelcastException
    {
        public TargetNotMemberException(String message) : base(message)
        {
        }

    }

    internal class TargetDisconnectedException : RetryableHazelcastException
    {
        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }

        public TargetDisconnectedException(String msg) : base(msg)
        {
        }
    }
}