﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace AsyncTests1.Networking
{
    [Flags]
    public enum FrameFlags2 : ushort
    {
        Default = 0,
        Final = 1 << 13,
        Begin = 1 << 12,
        End = 1 << 11,
        Null = 1 << 10
    }

    public static class FrameFlags2Extensions
    {
        public static bool Has(this FrameFlags2 value, FrameFlags2 flag)
            => value.HasFlag(flag);
    }
}