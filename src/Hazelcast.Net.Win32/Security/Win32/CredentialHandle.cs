// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    internal abstract class Credential
    {
        protected const int SEC_WINNT_AUTH_IDENTITY_VERSION_2 = 0x201;

        internal abstract CredentialHandle Structify();

        public static Credential Current()
        {
            return new CurrentCredential();
        }

        private class CurrentCredential : Credential
        {
            internal unsafe override CredentialHandle Structify()
            {
                return new CredentialHandle((void*)0);
            }
        }
    }

    public class CredentialHandle : SafeHandle
    {
        public unsafe CredentialHandle(void* cred)
            : base(new IntPtr(cred), true)
        {
        }

        public override bool IsInvalid => base.handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }
}
