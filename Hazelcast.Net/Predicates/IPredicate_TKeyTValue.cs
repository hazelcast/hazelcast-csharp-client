using System;

namespace Hazelcast.Predicates
{
    [Obsolete("Use non generic version, IPredicate instead.")]
    public interface IPredicate<TKey, TValue> : IPredicate
    {
    }
}
