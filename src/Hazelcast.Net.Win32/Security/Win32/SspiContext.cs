// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hazelcast.Security.Win32
{
    internal class SspiContext : IDisposable
    {
        private readonly string spn;

        private readonly SspiSecurityContext context;

        public SspiContext(string spn, string package = "Kerberos")
            : this(spn, Credential.Current(), package)
        { }

        public SspiContext(string spn, Credential credential, string package = "Kerberos")
        {
            this.spn = spn;

            context = new SspiSecurityContext(credential, package);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public byte[] RequestToken()
        {
            var status = context.InitializeSecurityContext(spn, null, out byte[] clientRequest);

            if (status == ContextStatus.Error)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return clientRequest;
        }

        public void AcceptToken(byte[] token, out byte[] serverResponse)
        {
            var status = context.AcceptSecurityContext(token, out serverResponse);

            if (status == ContextStatus.Error)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
