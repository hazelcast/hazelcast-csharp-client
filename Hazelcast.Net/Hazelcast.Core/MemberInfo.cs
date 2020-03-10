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
using System.Collections.Generic;
using System.Net;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Core
{
    public class MemberInfo : IMember
    {
        private readonly ILogger _logger;

        public MemberInfo(Address address, Guid uuid, IDictionary<string,string> attributes, bool isLiteMember, MemberVersion version)
        {
            _logger = Logger.GetLogger(typeof (IMember) + ":" + address);
            Address = address;
            Uuid = uuid;
            IsLiteMember = isLiteMember;
            Version = version;
            Attributes = attributes;
        }

        public Address Address { get; }
        public Guid Uuid { get; }
        public IDictionary<string, string> Attributes { get; }
        public bool IsLiteMember { get; }
        public MemberVersion Version { get; }


        public IPEndPoint SocketAddress
        {
            get
            {
                try
                {
                    return Address.GetInetSocketAddress();
                }
                catch (Exception e)
                {
                    _logger?.Warning(e);
                    return null;
                }
            }
        }

        private bool Equals(MemberInfo other)
        {
            return Uuid.Equals(other.Uuid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberInfo) obj);
        }

        public override int GetHashCode() => Uuid.GetHashCode();

        public override string ToString()
        {
            return $"Member [{Address.Host}]:{Address.Port} - {Uuid}{(IsLiteMember ? " lite" : "")}";
        }
    }
}