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
using System.Collections.Concurrent;
using Hazelcast.Messaging;

namespace Hazelcast.Serialization
{
    internal sealed partial class PortableContext // ClassDefinitionContext
    {
        private sealed class ClassDefinitionContext
        {
            private readonly ConcurrentDictionary<int, int> _currentClassVersions = new ConcurrentDictionary<int, int>();
            private readonly int _factoryId;
            private readonly PortableContext _portableContext;

            private readonly ConcurrentDictionary<long, IClassDefinition> _versionedDefinitions =
                new ConcurrentDictionary<long, IClassDefinition>();

            internal ClassDefinitionContext(PortableContext portableContext, int factoryId)
            {
                _portableContext = portableContext;
                _factoryId = factoryId;
            }

            internal int GetClassVersion(int classId)
            {
                int version;
                var hasValue = _currentClassVersions.TryGetValue(classId, out version);
                return hasValue ? version : -1;
            }

            internal IClassDefinition Lookup(int classId, int version)
            {
                var versionedClassId = Portability.CombineToLong(classId, version);
                IClassDefinition cd;
                _versionedDefinitions.TryGetValue(versionedClassId, out cd);
                return cd;
            }

            internal IClassDefinition Register(IClassDefinition cd)
            {
                if (cd == null)
                {
                    return null;
                }
                if (cd.GetFactoryId() != _factoryId)
                {
                    throw new HazelcastSerializationException("Invalid factory-id! " + _factoryId + " -> " + cd);
                }
                if (cd is ClassDefinition)
                {
                    var cdImpl = (ClassDefinition) cd;
                    cdImpl.SetVersionIfNotSet(_portableContext.GetVersion());
                }
                var versionedClassId = Portability.CombineToLong(cd.GetClassId(), cd.GetVersion());
                var currentCd = _versionedDefinitions.GetOrAdd(versionedClassId, cd);
                if (Equals(currentCd, cd))
                {
                    return cd;
                }
                if (currentCd is ClassDefinition)
                {
                    if (!currentCd.Equals(cd))
                    {
                        throw new HazelcastSerializationException(
                            "Incompatible class-definitions with same class-id: " + cd + " VS " + currentCd);
                    }
                    return currentCd;
                }
                _versionedDefinitions.AddOrUpdate(versionedClassId, cd, (key, oldValue) => cd);
                return cd;
            }

            internal void SetClassVersion(int classId, int version)
            {
                var hasAdded = _currentClassVersions.TryAdd(classId, version);
                if (!hasAdded && _currentClassVersions[classId] != version)
                {
                    throw new ArgumentException("Class-id: " + classId + " is already registered!");
                }
            }
        }
    }
}