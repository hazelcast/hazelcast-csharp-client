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

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hazelcast.Testing
{
    public static class ExceptionExtensions
    {
        public static TException SerializeAndDeSerialize<TException>(this TException e)
            where TException : Exception
        {
            // read https://stackoverflow.com/questions/94488

            var fmt = new BinaryFormatter();
            using var ms = new MemoryStream();

            fmt.Serialize(ms, e);
            ms.Seek(0, 0);
            return (TException)fmt.Deserialize(ms);
        }
    }
}
