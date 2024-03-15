﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System.Reflection;

namespace Hazelcast.Linq
{
    internal class HMemberInfo
    {
        public HMemberInfo(MemberInfo memberInfo, bool isPrimitive, bool isKey)
        {
            MemberInfo = memberInfo;
            IsPrimitive = isPrimitive;
            IsKey = isKey;
        }

        public MemberInfo MemberInfo { get; }
        public bool IsPrimitive { get;  }
        public bool IsKey { get;  }
    }
}
