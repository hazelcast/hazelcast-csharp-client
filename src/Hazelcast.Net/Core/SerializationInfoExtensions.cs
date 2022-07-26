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

using System;
using System.Runtime.Serialization;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="SerializationInfo"/> class.
    /// </summary>
    public static class SerializationInfoExtensions
    {
        /// <summary>
        /// Retrieves a <see cref="Guid"/> value from the <see cref="SerializationInfo"/> store.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> store.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A <see cref="Guid"/> value.</returns>
        public static Guid GetGuid(this SerializationInfo info, string name)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            // GetValue either returns a Guid or throws, so the cast is safe
            return (Guid) info.GetValue(name, typeof (Guid));
        }
    }
}
