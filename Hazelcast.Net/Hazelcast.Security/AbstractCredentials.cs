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

using System;

namespace Hazelcast.Security
{
    /// <summary>
    ///     Abstract implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    /// </summary>
    [Serializable]
    internal abstract class AbstractCredentials : ICredentials
    {
        private string _endpoint;
        private string _principal;

        public AbstractCredentials()
        {
        }

        public AbstractCredentials(string principal)
        {
            _principal = principal;
        }

        public string GetEndpoint()
        {
            return _endpoint;
        }

        public void SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;
        }

        public virtual string GetPrincipal()
        {
            return _principal;
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
            var other = (AbstractCredentials) obj;
            if (_principal == null)
            {
                if (other._principal != null)
                {
                    return false;
                }
            }
            else
            {
                if (!_principal.Equals(other._principal))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var prime = 31;
            var result = 1;
            result = prime*result + ((_principal == null) ? 0 : _principal.GetHashCode());
            return result;
        }

        public virtual void SetPrincipal(string principal)
        {
            _principal = principal;
        }
    }
}