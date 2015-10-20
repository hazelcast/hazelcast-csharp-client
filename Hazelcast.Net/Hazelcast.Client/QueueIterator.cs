/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client
{
    internal class QueueIterator<E> : IEnumerator<E>
    {
        private readonly IEnumerator<IData> iter;
        private readonly ISerializationService serializationService;
        private E _currentE;

        public QueueIterator(IEnumerator<IData> iter, ISerializationService serializationService)
        {
            this.iter = iter;
            this.serializationService = serializationService;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (iter.MoveNext())
            {
                try
                {
                    _currentE = serializationService.ToObject<E>(iter.Current);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public void Reset()
        {
            iter.Reset();
        }

        public E Current
        {
            get { return _currentE; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}