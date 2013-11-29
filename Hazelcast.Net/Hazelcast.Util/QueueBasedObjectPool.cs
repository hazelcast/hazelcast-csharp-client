using System;
using System.Collections.Concurrent;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    public class QueueBasedObjectPool<E> : IObjectPool<E> where E : class
    {
        private readonly DestructorMethod<E> _destructor;
        private readonly FactoryMethod<E> _factory;
        private readonly BlockingCollection<E> _queue;

        private volatile bool _active = true;

        private QueueBasedObjectPool()
        {
            //
        }

        public QueueBasedObjectPool(int capacity, FactoryMethod<E> factory, DestructorMethod<E> destructor)
        {
            _queue = new BlockingCollection<E>(capacity);
            _factory = factory;
            _destructor = destructor;
        }

        public virtual E Take()
        {
            if (!_active)
            {
                return null;
            }
            E e;
            _queue.TryTake(out e);
            if (e == null)
            {
                try
                {
                    e = _factory();
                }
                catch (Exception ex)
                {
                    throw new HazelcastException(ex);
                }
            }
            return e;
        }

        public virtual void Release(E e)
        {
            if (!_active || !_queue.TryAdd(e))
            {
                _destructor(e);
            }
        }

        public virtual int Size()
        {
            return _queue.Count;
        }

        public virtual void Destroy()
        {
            _active = false;
            _queue.CompleteAdding();
            foreach (E e in _queue.ToArray())
            {
                _destructor(e);
            }
            _queue.Dispose();
        }
    }
}