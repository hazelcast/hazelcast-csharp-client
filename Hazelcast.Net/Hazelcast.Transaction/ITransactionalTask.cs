namespace Hazelcast.Transaction
{
    public interface ITransactionalTask<T>
    {
 
        T Execute(ITransactionalTaskContext context);
    }

    delegate T ExecuteTransactionalTask<T>(ITransactionalTaskContext context);
}