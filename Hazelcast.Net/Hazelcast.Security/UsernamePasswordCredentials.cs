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

namespace Hazelcast.Security
{
    /// <summary>
    ///     Simple implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    ///     using
    ///     username and password as security attributes.
    /// </summary>
    [Serializable]
    public class UsernamePasswordCredentials : AbstractCredentials
    {
        public UsernamePasswordCredentials() : this("", "") {}
        
        public UsernamePasswordCredentials(string username, string password) : base(username)
        {
            Password = password;
        }

        /// <summary>
        /// Username property, act as the principal value of the ICredential interface
        /// </summary>
        public string Username
        {
            get
            {
                return GetPrincipal(); 
            }
            set
            {
                SetPrincipal(value);
            }
        }

        /// <summary>
        /// Password principal
        /// </summary>
        public string Password { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "UsernamePasswordCredentials [username=" + Username + "]";
        }
    }
}