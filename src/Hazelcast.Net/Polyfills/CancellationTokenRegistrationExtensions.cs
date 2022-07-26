﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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


#if !NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

// ReSharper disable once CheckNamespace
namespace System.Threading
{
#if !NET5_0_OR_GREATER
    internal static class CancellationTokenRegistrationExtensions
    {
        public static ValueTask DisposeAsync(this CancellationTokenRegistration registration)
        {
            registration.Dispose();
            return default;
        }
    }
#endif
}
