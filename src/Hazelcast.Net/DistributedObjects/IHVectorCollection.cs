// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Models;
namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed collection of vector documents.
    /// </summary>
    /// <typeparam name="TKey">The type of the key associated with the collection.</typeparam>
    /// <typeparam name="TValue">The type of the value contained in the vector documents.</typeparam>
    public interface IHVectorCollection<TKey, TValue>:IDistributedObject
    {
        /// <summary>
        /// Gets the vector document associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the vector document to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the vector document associated
        /// with the specified key.</returns>
        Task<VectorDocument<TValue>> GetAsync(TKey key);
        
        /// <summary>
        /// Puts the specified vector document into the collection with the specified key.
        /// </summary>
        /// <param name="key">The key of the vector document to put.</param>
        /// <param name="valueVectorDocument">The vector document to put into the collection.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the previous vector document
        /// associated with the specified key, or null if there was no mapping for the key.</returns>
        Task<VectorDocument<TValue>> PutAsync(TKey key, VectorDocument<TValue> valueVectorDocument);
        
        /// <summary>
        /// Sets the specified vector document into the collection with the specified key.
        /// </summary>
        /// <param name="key">The key of the vector document to set.</param>
        /// <param name="vectorDocument">The vector document to set into the collection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetAsync(TKey key, VectorDocument<TValue> vectorDocument);
        
        /// <summary>
        /// Puts the specified vector document into the collection with the specified key if the key is not already associated
        /// with a vector document.
        /// </summary>
        /// <param name="key">The key of the vector document to put if absent.</param>
        /// <param name="vectorDocument">The vector document to put into the collection if the key is not already associated with
        /// a vector document.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the previous vector document
        /// associated with the specified key, or null if there was no mapping for the key.</returns>
        Task<VectorDocument<TValue>> PutIfAbsentAsync(TKey key, VectorDocument<TValue> vectorDocument);
        
        /// <summary>
        /// Puts all the specified vector documents into the collection.
        /// </summary>
        /// <param name="vectorDocumentMap">A dictionary containing the keys and vector documents to put into the collection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PutAllAsync(IDictionary<TKey, VectorDocument<TValue>> vectorDocumentMap);
        
        /// <summary>
        /// Removes the vector document associated with the specified key from the collection.
        /// </summary>
        /// <param name="key">The key of the vector document to remove.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the removed vector document,
        /// or null if there was no mapping for the key.</returns>
        Task<VectorDocument<TValue>> RemoveAsync(TKey key);
        
        /// <summary>
        /// Optimizes the only index by fully removing nodes marked for deletion,
        /// trimming neighbor sets to the advertised degree, and updating the entry node as necessary.
        /// <para>
        /// Backups of this operation are always executed as async backups.
        /// </para>
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is void if the process finishes successfully;
        /// or completed exceptionally with an <see cref="IndexMutationDisallowedException"/> if the index is currently
        /// undergoing an optimization operation; or completed exceptionally with an <see cref="System.ArgumentException"/>
        /// if the collection has more than one index.</returns>
        Task OptimizeAsync();
        
        /// <summary>
        /// Optimizes the specified index by fully removing nodes marked for deletion,
        /// trimming neighbor sets to the advertised degree, and updating the entry node as necessary.
        /// <para>
        /// Backups of this operation are always executed as async backups.
        /// </para>
        /// </summary>
        /// <param name="indexName">The name of the index to optimize.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is void if the process finishes successfully;
        /// or completed exceptionally with an <see cref="IndexMutationDisallowedException"/> if the index is currently
        /// undergoing an optimization operation; or completed exceptionally with an <see cref="System.ArgumentException"/>
        /// if the collection has more than one index.</returns>
        Task OptimizeAsync(string indexName);
        
        /// <summary>
        /// Asynchronously clears all entries in the collection.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ClearAsync();
        
        /// <summary>
        /// Gets the number of entries in the vector collection.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of entries in the
        /// vector collection.</returns>
        Task<long> GetSizeAsync();
        
        /// <summary>
        /// Performs asynchronously a similarity search according to the options in the given <paramref name="searchOptions"/>.
        /// <para>
        /// If there are many concurrent modifications during the search, it is possible but extremely unlikely
        /// to receive fewer results than requested, even when the collection contains enough items.
        /// </para>
        /// </summary>
        /// <param name="vectorValues">The search vector. Can be unnamed if the collection has only one index,
        /// otherwise it has to be associated with an index name.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="IVectorSearchResult{TKey, TValue}"/> object
        /// that allows iterating over search results in order of descending similarity score.</returns>
        Task<IVectorSearchResult<TKey, TValue>> SearchAsync(VectorValues vectorValues, VectorSearchOptions searchOptions);
    }
}
