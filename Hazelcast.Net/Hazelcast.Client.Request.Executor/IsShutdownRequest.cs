using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Executor
{
    //internal class IsShutdownRequest : ClientRequest, IRetryableRequest
    //{
    //    internal string name;

    //    public IsShutdownRequest()
    //    {
    //    }

    //    public IsShutdownRequest(string name)
    //    {
    //        this.name = name;
    //    }

    //    public override int GetFactoryId()
    //    {
    //        return ExecutorDataSerializerHook.FId;
    //    }

    //    public override int GetClassId()
    //    {
    //        return ExecutorDataSerializerHook.IsShutdownRequest;
    //    }

    //    public override void WritePortable(IPortableWriter writer)
    //    {
    //        writer.WriteUTF("n", name);
    //    }


    //}
}