using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class ExecutorDataSerializerHook : DataSerializerHook
    {
        public const int CallableTask = 0;
        public const int MemberCallableTask = 1;
        public const int RunnableAdapter = 2;
        public const int TargetCallableRequest = 6;
        public const int LocalTargetCallableRequest = 7;
        public const int IsShutdownRequest = 9;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ExecutorDsFactory, -13);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IDataSerializableFactory CreateFactory()
        {
            return null;
        }
    }
}