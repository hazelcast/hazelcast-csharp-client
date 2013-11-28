using System;
using Hazelcast.Util;


namespace Hazelcast.Util
{
    public interface IObjectPool<E> where E : class
	{
		E Take();

		void Release(E e);

		int Size();

		void Destroy();
	}
}
