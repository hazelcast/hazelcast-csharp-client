using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client
{
    internal class QueueIterator<E> : IEnumerator<E>
    {
        private readonly IEnumerator<Data> iter;
        private readonly ISerializationService serializationService;
        private E _currentE;

        public QueueIterator(IEnumerator<Data> iter, ISerializationService serializationService)
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