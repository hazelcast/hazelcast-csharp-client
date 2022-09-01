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

using System.Runtime.InteropServices;

namespace Hazelcast.Security.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AuthIdentity
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string User;
        public int UserLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Domain;
        public int DomainLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Password;
        public int PasswordLength;
        public int Flags;
    }
}
