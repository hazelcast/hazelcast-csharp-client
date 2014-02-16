using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>Concurrent, distributed, partitioned, listenable collection.</summary>
    /// <remarks>Concurrent, distributed, partitioned, listenable collection.</remarks>
    public interface IHCollection<T> : ICollection<T>,  IDistributedObject
    {

        string AddItemListener(IItemListener<T> listener, bool includeValue);

        bool RemoveItemListener(string registrationId);

        new bool Add(T item);

        int Size();

        bool IsEmpty();

        T[] ToArray();

        T[] ToArray<T>(T[] a);

        bool ContainsAll<T>(ICollection<T> c);

        bool RemoveAll<T>(ICollection<T> c);

        bool RetainAll<T>(ICollection<T> c);

        bool AddAll<T>(ICollection<T> c);
    }

}