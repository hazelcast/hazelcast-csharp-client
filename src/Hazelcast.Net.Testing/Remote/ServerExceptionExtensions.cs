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

using System;
using System.Reflection;

namespace Hazelcast.Testing.Remote
{
    /// <summary>
    /// Provides extension methods for the <see cref="ServerException"/> class.
    /// </summary>
    public static class ServerExceptionExtensions
    {
        private static FieldInfo _messageField;
        
        // NOTE
        //
        // the ServerException class is generated by Thrift so we should not modify it,
        // yet its code is ugly C# which hides the actual Message property, which thus
        // remains empty... so we provide a equally ugly method to fix it.
        
        /// <summary>
        /// Fix the <see cref="ServerException"/> message so it appears correctly.
        /// </summary>
        /// <param name="serverException">A <see cref="ServerException"/> instance.</param>
        public static void FixMessage(this ServerException serverException)
        {
            _messageField ??= typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_messageField == null) return; // bah
            _messageField.SetValue(serverException, serverException.Message);
        }
    }
}
