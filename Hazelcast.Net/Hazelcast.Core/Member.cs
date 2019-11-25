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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Core
{
    internal sealed class Member : IMember
    {
        private readonly ConcurrentDictionary<string, string> _attributes = new ConcurrentDictionary<string, string>();
        private readonly ILogger _logger;

        public Member()
        {
        }

        public Member(Address address)
            : this(address, Guid.Empty)
        {
        }

        public Member(Address address, Guid uuid)
            : this(address, uuid, new Dictionary<string, string>(), false)
        {
        }

        public Member(Address address, Guid uuid, IDictionary<string, string> attributes, bool liteMember)
        {
            _logger = Logger.GetLogger(typeof (Member) + ":" + address);
            Address = address;
            Uuid = uuid;
            IsLiteMember = liteMember;
            foreach (var kv in attributes)
            {
                _attributes.TryAdd(kv.Key, kv.Value);
            }
        }

        public bool IsLiteMember { get; }

        public Address Address { get; }

        public IPEndPoint GetSocketAddress()
        {
            try
            {
                return Address.GetInetSocketAddress();
            }
            catch (Exception e)
            {
                if (_logger != null)
                {
                    _logger.Warning(e);
                }
                return null;
            }
        }

        public Guid Uuid { get; }

        public IDictionary<string, string> Attributes => _attributes;

        public string GetAttribute(string key)
        {
            _attributes.TryGetValue(key, out var @out);
            return @out;
        }

        private bool Equals(Member other)
        {
            return Equals(Address, other.Address) && string.Equals(Uuid, other.Uuid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Member && Equals((Member) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Address != null ? Address.GetHashCode() : 0) * 397) ^ (Uuid != null ? Uuid.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Member [");
            sb.Append(Address.Host);
            sb.Append("]");
            sb.Append(":");
            sb.Append(Address.Port);
            sb.Append(" - ").Append(Uuid);
            if (IsLiteMember) {
                sb.Append(" lite");
            }
            return sb.ToString();
        }

        internal void UpdateAttribute(MemberAttributeOperationType operationType, string key, string value)
        {
            switch (operationType)
            {
                case MemberAttributeOperationType.Put:
                    _attributes.TryAdd(key, value);
                    break;
                case MemberAttributeOperationType.Remove:
                    string _out;
                    _attributes.TryRemove(key, out _out);
                    break;
            }
        }
    }
}