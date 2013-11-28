using Hazelcast.Transaction;


namespace Hazelcast.Transaction
{
	
	public enum TransactionState
	{
		NoTxn,
		Active,
		Preparing,
		Prepared,
		Committing,
		Committed,
		CommitFailed,
		RollingBack,
		RolledBack
	}
}
