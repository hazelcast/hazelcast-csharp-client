using System;

namespace Hazelcast.Core
{
	public interface Transaction
	{
		/*public static int TXN_STATUS_NO_TXN = 0;
	    public static int TXN_STATUS_ACTIVE = 1;
	    public static int TXN_STATUS_PREPARED = 2;
	    public static int TXN_STATUS_COMMITTED = 3;
	    public static int TXN_STATUS_ROLLED_BACK = 4;
	    public static int TXN_STATUS_PREPARING = 5;
	    public static int TXN_STATUS_COMMITTING = 6;
	    public static int TXN_STATUS_ROLLING_BACK = 7;
	    public static int TXN_STATUS_UNKNOWN = 8;
	*/
	    /**
	     * Creates a new transaction and associate it with the current thread.
	     *
	     * @throws IllegalStateException if transaction is already began
	     */
	    void begin();
	
	    /**
	     * Commits the transaction associated with the current thread.
	     *
	     * @throws IllegalStateException if transaction didn't begin.
	     */
	    void commit();
	
	    /**
	     * Rolls back the transaction associated with the current thread.
	     *
	     * @throws IllegalStateException if transaction didn't begin.
	     */
	    void rollback();
	
	    /**
	     * Returns the status of the transaction associated with the current thread.
	     *
	     * @return the status
	     */
	    int getStatus();
	}
}

