using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Util
{
    
	public class QueueBasedObjectPool<E> : IObjectPool<E> where E : class
	{
        private readonly BlockingCollection<E> _queue;

        private readonly FactoryMethod<E> _factory ;

		private readonly DestructorMethod<E> _destructor;

		private volatile bool _active = true;

	    private QueueBasedObjectPool()
	    {
	        //
	    }

        public QueueBasedObjectPool(int capacity, FactoryMethod<E> factory, DestructorMethod<E> destructor  )
		{
            this._queue = new BlockingCollection<E>(capacity);
            this._factory = factory;
			this._destructor = destructor;
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
