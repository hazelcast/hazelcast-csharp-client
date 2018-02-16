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
using System.Text;

namespace Hazelcast.Security
{
    /// <summary>
    ///     Simple implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    ///     using
    ///     username and password as security attributes.
    /// </summary>
    [Serializable]
    internal class UsernamePasswordCredentials : AbstractCredentials
    {
        private byte[] _password;

        public UsernamePasswordCredentials()
        {
        }

        public UsernamePasswordCredentials(string username, string password) : base(username)
        {
            _password = Encoding.UTF8.GetBytes(password);
        }

        public virtual string GetPassword()
        {
            return _password == null ? string.Empty : Encoding.UTF8.GetString(_password);
        }

        public virtual string GetUsername()
        {
            return GetPrincipal();
        }

        public virtual void SetPassword(string password)
        {
            _password = Encoding.UTF8.GetBytes(password);
        }

        public virtual void SetUsername(string username)
        {
            SetPrincipal(username);
        }

        public override string ToString()
        {
            return "UsernamePasswordCredentials [username=" + GetUsername() + "]";
        }
    }
}