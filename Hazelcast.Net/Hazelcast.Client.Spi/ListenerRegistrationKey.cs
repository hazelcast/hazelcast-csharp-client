// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ListenerRegistrationKey
    {
        public string UserRegistrationId { get; private set; }
        public IClientMessage RegistrationRequest { get; private set; }
        public DecodeRegistrationResponse ResponseDecoder { get; private set; }
        public DistributedEventHandler EventHandler { get; private set; }

        public ListenerRegistrationKey(string userRegistrationId, IClientMessage registrationRequest = null,
            DecodeRegistrationResponse responseDecoder = null, DistributedEventHandler eventHandler = null)
        {
            UserRegistrationId = userRegistrationId;
            RegistrationRequest = registrationRequest;
            ResponseDecoder = responseDecoder;
            EventHandler = eventHandler;
        }

        protected bool Equals(ListenerRegistrationKey other)
        {
            return string.Equals(UserRegistrationId, other.UserRegistrationId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListenerRegistrationKey) obj);
        }

        public override int GetHashCode()
        {
            return UserRegistrationId != null ? UserRegistrationId.GetHashCode() : 0;
        }
    }
}