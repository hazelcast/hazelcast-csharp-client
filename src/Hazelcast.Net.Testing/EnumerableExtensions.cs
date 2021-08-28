// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Testing
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (batchSize <= 0) throw new ArgumentException(@"Batch size must be positive.", nameof(batchSize));

            List<T> batch = null;
            foreach (var elem in source)
            {
                batch ??= new List<T>(batchSize);

                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }

                batch.Add(elem);
            }

            if (batch != null && batch.Any())
                yield return batch;
        }
    }
}
