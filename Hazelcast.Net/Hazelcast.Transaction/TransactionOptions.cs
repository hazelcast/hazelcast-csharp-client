using System;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;


namespace Hazelcast.Transaction
{
	/// <summary>Contains the configuration for a </summary>
	/// <remarks>Contains the configuration for a </remarks>
	[System.Serializable]
	public sealed class TransactionOptions : IDataSerializable
	{
		private long timeoutMillis;

		private int durability;

		private TransactionType transactionType;

		/// <summary>Creates a new default configured TransactionsOptions.</summary>
		/// <remarks>
		/// Creates a new default configured TransactionsOptions.
		/// It will be configured with a timeout of 2 minutes, durability of 1 and a TransactionType.TWO_PHASE.
		/// </remarks>
		public TransactionOptions()
		{
			SetTimeout(2, TimeUnit.MINUTES).SetDurability(1).SetTransactionType(TransactionOptions.TransactionType.TwoPhase);
		}

		/// <summary>
		/// Gets the
		/// <see cref="TransactionType">TransactionType</see>
		/// .
		/// </summary>
		/// <returns>the TransactionType.</returns>
		public TransactionOptions.TransactionType GetTransactionType()
		{
			return transactionType;
		}

		/// <summary>
		/// Sets the
		/// <see cref="TransactionType">TransactionType</see>
		/// .
		/// A local transaction is less safe than a two phase transaction; when a member fails during the commit
		/// of a local transaction, it could be that some of the changes are committed, while others are not and this
		/// can leave your system in an inconsistent state.
		/// </summary>
		/// <param name="transactionType">the new TransactionType.</param>
		/// <returns>the updated TransactionOptions.</returns>
		/// <seealso cref="GetTransactionType()">GetTransactionType()</seealso>
		/// <seealso cref="SetDurability(int)">SetDurability(int)</seealso>
		public TransactionOptions SetTransactionType(TransactionOptions.TransactionType transactionType)
		{
			if (transactionType == null)
			{
				throw new ArgumentException("transactionType can't be null");
			}
			this.transactionType = transactionType;
			return this;
		}

		/// <summary>Gets the timeout in milliseconds.</summary>
		/// <remarks>Gets the timeout in milliseconds.</remarks>
		/// <returns>the timeout in milliseconds.</returns>
		/// <seealso cref="SetTimeout(long, TimeUnit)">SetTimeout(long, Hazelcast.Net.Ext.TimeUnit)</seealso>
		public long GetTimeoutMillis()
		{
			return timeoutMillis;
		}

		/// <summary>Sets the timeout.</summary>
		/// <remarks>
		/// Sets the timeout.
		/// The timeout determines the maximum lifespan of a transaction. So if a transaction is configured with a
		/// timeout of 2 minutes, then it will automatically rollback if it hasn't committed yet.
		/// </remarks>
		/// <param name="timeout">the timeout.</param>
		/// <param name="timeUnit">the TimeUnit of the timeout.</param>
		/// <returns>the updated TransactionOptions</returns>
		/// <exception cref="System.ArgumentException">if timeout smaller or equal than 0, or timeUnit is null.</exception>
		/// <seealso cref="GetTimeoutMillis()">GetTimeoutMillis()</seealso>
		public TransactionOptions SetTimeout(long timeout, TimeUnit timeUnit)
		{
			if (timeout <= 0)
			{
				throw new ArgumentException("Timeout must be positive!");
			}
			if (timeUnit == null)
			{
				throw new ArgumentException("timeunit can't be null");
			}
			this.timeoutMillis = timeUnit.ToMillis(timeout);
			return this;
		}

		/// <summary>Gets the transaction durability.</summary>
		/// <remarks>Gets the transaction durability.</remarks>
		/// <returns>the transaction durability.</returns>
		/// <seealso cref="SetDurability(int)">SetDurability(int)</seealso>
		public int GetDurability()
		{
			return durability;
		}

		/// <summary>Sets the transaction durability.</summary>
		/// <remarks>
		/// Sets the transaction durability.
		/// The durability is the number of machines that can take over if a member fails during a transaction
		/// commit or rollback. This value only has meaning when
		/// <see cref="TransactionType.TwoPhase">TransactionType.TwoPhase</see>
		/// is selected.
		/// </remarks>
		/// <param name="durability">the durability</param>
		/// <returns>the updated TransactionOptions.</returns>
		/// <exception cref="System.ArgumentException">if durability smaller than 0.</exception>
		public TransactionOptions SetDurability(int durability)
		{
			if (durability < 0)
			{
				throw new ArgumentException("Durability cannot be negative!");
			}
			this.durability = durability;
			return this;
		}

		/// <summary>Creates a new TransactionOptions configured with default settings.</summary>
		/// <remarks>Creates a new TransactionOptions configured with default settings.</remarks>
		/// <returns>the created default TransactionOptions.</returns>
		/// <seealso cref="TransactionOptions()">TransactionOptions()</seealso>
		public static TransactionOptions GetDefault()
		{
			return new TransactionOptions();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WriteData(IObjectDataOutput output)
		{
			output.WriteLong(timeoutMillis);
			output.WriteInt(durability);
			output.WriteInt((int)transactionType);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadData(IObjectDataInput input)
		{
			timeoutMillis = input.ReadLong();
			durability = input.ReadInt();
			transactionType = (TransactionType) input.ReadInt();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("TransactionOptions");
			sb.Append("{timeoutMillis=").Append(timeoutMillis);
			sb.Append(", durability=").Append(durability);
			sb.Append(", txType=").Append((int)transactionType);
			sb.Append('}');
			return sb.ToString();
		}

		public enum TransactionType
		{
			TwoPhase,
			Local
		}
	}
}
