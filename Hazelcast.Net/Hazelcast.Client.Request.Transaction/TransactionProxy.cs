using System;
using System.IO;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Net.Ext;
using Hazelcast.Transaction;
using Hazelcast.Util;


namespace Hazelcast.Client.Request.Transaction
{
	
	internal sealed class TransactionProxy
	{
		private static readonly ThreadLocal<bool> threadFlag = new ThreadLocal<bool>();

		private readonly TransactionOptions options;

		private readonly ClientClusterService clusterService;

		private readonly long threadId = Thread.CurrentThread.ManagedThreadId;

		private readonly Connection.IConnection connection;

		private string txnId;

		private TransactionState state = TransactionState.NoTxn;

		private long startTime = 0L;

		internal TransactionProxy(HazelcastClient client, TransactionOptions options,Connection.IConnection connection)
		{
			this.options = options;
			this.clusterService = (ClientClusterService)client.GetClientClusterService();
			this.connection = connection;
		}

		public string GetTxnId()
		{
			return txnId;
		}

		public TransactionState GetState()
		{
			return state;
		}

		public long GetTimeoutMillis()
		{
			return options.GetTimeoutMillis();
		}

		internal void Begin()
		{
			try
			{
				if (state == TransactionState.Active)
				{
					throw new InvalidOperationException("Transaction is already active");
				}
				CheckThread();
                if (threadFlag.IsValueCreated)
                {
                    throw new InvalidOperationException("Nested transactions are not allowed!");
                }
				threadFlag.Value = true;
				startTime = Clock.CurrentTimeMillis();
                txnId = SendAndReceive<string>(new CreateTransactionRequest(options));
				state = TransactionState.Active;
			}
			catch (Exception e)
			{
				CloseConnection();
				throw ExceptionUtil.Rethrow(e);
			}
		}

		internal void Commit()
		{
			try
			{
				if (state != TransactionState.Active)
				{
					throw new TransactionNotActiveException("Transaction is not active");
				}
				CheckThread();
				CheckTimeout();
				SendAndReceive<object>(new CommitTransactionRequest());
				state = TransactionState.Committed;
			}
			catch (Exception e)
			{
				state = TransactionState.RollingBack;
				throw ExceptionUtil.Rethrow(e);
			}
			finally
			{
				CloseConnection();
			}
		}

		internal void Rollback()
		{
			try
			{
				if (state == TransactionState.NoTxn || state == TransactionState.RolledBack)
				{
					throw new InvalidOperationException("Transaction is not active");
				}
				if (state == TransactionState.RollingBack)
				{
					state = TransactionState.RolledBack;
					return;
				}
				CheckThread();
				try
				{
                    SendAndReceive<object>(new RollbackTransactionRequest());
				}
				catch (Exception)
				{
				}
				state = TransactionState.RolledBack;
			}
			finally
			{
				CloseConnection();
			}
		}

		private void CloseConnection()
		{
			threadFlag.Dispose();
			try
			{
				connection.Release();
			}
			catch (IOException)
			{
				IOUtil.CloseResource(connection);
			}
		}

		private void CheckThread()
		{
			if (threadId != Thread.CurrentThread.ManagedThreadId)
			{
				throw new InvalidOperationException("Transaction cannot span multiple threads!");
			}
		}

		private void CheckTimeout()
		{
			if (startTime + options.GetTimeoutMillis() < Clock.CurrentTimeMillis())
			{
				throw new TransactionException("Transaction is timed-out!");
			}
		}

		private T SendAndReceive<T>(object request)
		{
			try
			{
				return clusterService.SendAndReceiveFixedConnection<T>(connection, request);
			}
			catch (IOException e)
			{
				throw ExceptionUtil.Rethrow(e);
			}
		}
	}
}
