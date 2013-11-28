using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Queue;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Serialization.Hook
{
	
	public class QueuePortableHook : IPortableHook
	{
		public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.QueuePortableFactory, -11);
		public const int Offer = 1;
		public const int Size = 2;
		public const int Remove = 3;
		public const int Poll = 4;
		public const int Peek = 5;
		public const int Iterator = 6;
		public const int Drain = 7;
		public const int Contains = 8;
		public const int CompareAndRemove = 9;
		public const int Clear = 10;
		public const int AddAll = 11;
		public const int AddListener = 12;
		public const int RemainingCapacity = 13;
		public const int TxnOffer = 14;
		public const int TxnPoll = 15;
		public const int TxnSize = 16;
		public const int TxnPeek = 17;

		public virtual int GetFactoryId()
		{
			return FId;
		}

		public virtual IPortableFactory CreateFactory()
		{
            var constructors = new Func<int, IPortable>[TxnPeek + 1];
			constructors[Offer] = arg => new OfferRequest(); 
			constructors[Size] = arg => new SizeRequest(); 
			constructors[Remove] = arg => new RemoveRequest(); 
			constructors[Poll] = arg => new PollRequest(); 
			constructors[Peek] = arg => new PeekRequest(); 
			constructors[Iterator] = arg => new IteratorRequest(); 
			constructors[Drain] = arg => new DrainRequest(); 
			constructors[Contains] = arg => new ContainsRequest(); 
			constructors[CompareAndRemove] = arg => new CompareAndRemoveRequest(); 
			constructors[Clear] = arg => new ClearRequest(); 
			constructors[AddAll] = arg => new AddAllRequest(); 
			constructors[AddListener] = arg => new AddListenerRequest(); 
			constructors[RemainingCapacity] = arg => new RemainingCapacityRequest(); 
			constructors[TxnOffer] = arg => new TxnOfferRequest(); 
			constructors[TxnPoll] = arg => new TxnPollRequest(); 
			constructors[TxnSize] = arg => new TxnSizeRequest(); 
			constructors[TxnPeek] = arg => new TxnPeekRequest();
            return new ArrayPortableFactory(constructors);
		}

		public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
		{
			return null;
		}
	}
}
