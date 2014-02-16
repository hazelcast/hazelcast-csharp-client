using System;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    [Serializable]
    public sealed class RunnableAdapter<V> : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        private Runnable task;

        public RunnableAdapter()
        {
        }

        public RunnableAdapter(Runnable task)
        {
            this.task = task;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(task);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            task = input.ReadObject<Runnable>();
        }

        public int GetFactoryId()
        {
            return ExecutorDataSerializerHook.FId;
        }

        public int GetId()
        {
            return ExecutorDataSerializerHook.RunnableAdapter;
        }

        public Runnable GetRunnable()
        {
            return task;
        }

        public void SetRunnable(Runnable runnable)
        {
            task = runnable;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("RunnableAdapter");
            sb.Append("{task=").Append(task);
            sb.Append('}');
            return sb.ToString();
        }
    }
}