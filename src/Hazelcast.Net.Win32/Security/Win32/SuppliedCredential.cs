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
//
// This code file is heavily inspired from the Kerberos.NET library,
// which is Copyright (c) 2017 Steve Syfuhs and released under the MIT
// license at
//
// https://github.com/SteveSyfuhs/Kerberos.NET
//

using System;
using System.Runtime.InteropServices;

namespace Hazelcast.Security.Win32
{
    public class SuppliedCredential : Credential
    {
        private readonly AuthIdentity _auth;

        public SuppliedCredential(string username, string password, string domain)
        {
            _auth = new AuthIdentity
            {
                User = username,
                UserLength = username.Length,
                Domain = domain,
                DomainLength = domain.Length,
                Password = password,
                PasswordLength = password.Length,
                Flags = 0x2 // UNICODE
            };
        }

        internal override CredentialHandle Structify()
            => SuppliedCredentialHandle.Create(_auth);

        private class SuppliedCredentialHandle : CredentialHandle
        {
            private unsafe void* _cred;

            private unsafe SuppliedCredentialHandle(void* cred)
                : base(cred)
            {
                _cred = cred;
            }

            public static unsafe SuppliedCredentialHandle Create(AuthIdentity auth)
            {
                // allocating here, must free in ReleaseHandle!
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(auth));
                Marshal.StructureToPtr(auth, ptr, false);
                return new SuppliedCredentialHandle((void*)ptr);
            }

            protected override unsafe bool ReleaseHandle()
            {
                Marshal.FreeHGlobal((IntPtr) _cred);
                _cred = null;
                return base.ReleaseHandle();
            }
        }
    }
}
