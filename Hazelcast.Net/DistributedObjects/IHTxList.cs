using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    public interface IHTxList<TItem>
    {
        // FIXME wip
        // - rename this interface
        // - what happens if an operation throws? should we have tmieouts?

        /// <summary>Add new item to transactional list</summary>
        /// <param name="item">item</param>
        /// <returns>true if item is added successfully</returns>
        Task<bool> AddAsync(TItem item, CancellationToken cancellationToken);

        /// <summary>Add item from transactional list</summary>
        /// <param name="item">item</param>
        /// <returns>true if item is remove successfully</returns>
        Task<bool> RemoveAsync(TItem item, CancellationToken cancellationToken);

        /// <summary>Returns the size of the list</summary>
        /// <returns>size</returns>
        Task<int> CountAsync(CancellationToken cancellationToken);
    }
}
