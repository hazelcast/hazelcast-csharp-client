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
        private readonly Address _address;
        private readonly ConcurrentDictionary<string, string> _attributes = new ConcurrentDictionary<string, string>();
        private readonly bool _liteMember;
        private readonly ILogger _logger;
        private readonly string _uuid;

        public Member()
        {
        }

        public Member(Address address)
            : this(address, null)
        {
        }

        public Member(Address address, string uuid)
            : this(address, uuid, new Dictionary<string, string>(), false)
        {
        }

        public Member(Address address, string uuid, IDictionary<string, string> attributes, bool liteMember)
        {
            _logger = Logger.GetLogger(typeof (Member) + ":" + address);
            _address = address;
            _uuid = uuid;
            _liteMember = liteMember;
            foreach (var kv in attributes)
            {
                _attributes.TryAdd(kv.Key, kv.Value);
            }
        }

        public bool IsLiteMember
        {
            get { return _liteMember; }
        }

        public Address GetAddress()
        {
            return _address;
        }

        public IPEndPoint GetSocketAddress()
        {
            try
            {
                return _address.GetInetSocketAddress();
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

        public string GetUuid()
        {
            return _uuid;
        }

        public IDictionary<string, string> GetAttributes()
        {
            return _attributes;
        }

        public string GetAttribute(string key)
        {
            string _out;
            _attributes.TryGetValue(key, out _out);
            return _out;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var other = (Member) obj;
            if (_address == null)
            {
                if (other._address != null)
                {
                    return false;
                }
            }
            else
            {
                if (!_address.Equals(other._address))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var Prime = 31;
            var result = 1;
            result = Prime*result + ((_address == null) ? 0 : _address.GetHashCode());
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Member [");
            sb.Append(_address.GetHost());
            sb.Append("]");
            sb.Append(":");
            sb.Append(_address.GetPort());
            sb.Append(" - ").Append(_uuid);
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