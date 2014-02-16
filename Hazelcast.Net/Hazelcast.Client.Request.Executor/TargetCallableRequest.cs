namespace Hazelcast.Client.Request.Executor
{
    //[System.Serializable]
    //public sealed class TargetCallableRequest : IdentifiedDataSerializable,IIdentifiedDataSerializable
    //{
    //    private string name;

    //    private Callable callable;

    //    private Address target;

    //    public TargetCallableRequest()
    //    {
    //    }

    //    public TargetCallableRequest(string name, Callable callable, Address target)
    //    {
    //        this.name = name;
    //        this.callable = callable;
    //        this.target = target;
    //    }

    //    public Address GetTarget()
    //    {
    //        return target;
    //    }

    //    public int GetFactoryId()
    //    {
    //        return ExecutorDataSerializerHook.FId;
    //    }

    //    public int GetId()
    //    {
    //        return ExecutorDataSerializerHook.TargetCallableRequest;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public void WriteData(IObjectDataOutput output)
    //    {
    //        output.WriteUTF(name);
    //        output.WriteObject(callable);
    //        target.WriteData(output);
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public void ReadData(IObjectDataInput input)
    //    {
    //        name = input.ReadUTF();
    //        callable = input.ReadObject();
    //        target = new Address();
    //        target.ReadData(input);
    //    }
    //}
}