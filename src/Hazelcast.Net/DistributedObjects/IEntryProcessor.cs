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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines a processor that can process the entries of an <see cref="IHMap{TKey,TValue}"/> on the server.
    /// </summary>
    /// <typeparam name="TResult">The type of the results produced by the processor.</typeparam>
    /// <remarks>
    /// <para>Client-side <see cref="IEntryProcessor{TResult}"/> implementations do not have any processing logic,
    /// they must be backed by a corresponding processor registered on the server and containing the
    /// actual implementation.</para>
    /// </remarks>
    // ReSharper disable once UnusedTypeParameter - yes we want it
    // so that ExecuteAsync<TResult>(IEntryProcessor<TResult> processor) can guess TResult
    public interface IEntryProcessor<TResult>
    {}
}
