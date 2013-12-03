using System;
namespace Hazelcast.Client
{
	public class Prefix
	{
		/*
     * The Constant MAP_BASED: "m:"
     */
		public static readonly String MAP_BASED = "m:";

		/*
     * The Constant MAP: "c:"
     */
		public static readonly String MAP = "c:";

		/*
     * The Constant AS_LIST: "l:"
     */
		public static readonly String AS_LIST = "l:";

		/*
     * The Constant LIST: "m:l:"
     */
		public static readonly String LIST = MAP_BASED + AS_LIST;

		/*
     * The Constant AS_SET: "s:"
     */
		public static readonly String AS_SET = "s:";

		/*
     * The Constant SET: "m:s:"
     */
		public static readonly String SET = MAP_BASED + AS_SET;

		/*
     * The Constant QUEUE: "q:"
     */
		public static readonly String QUEUE = "q:";

		/*
     * The Constant TOPIC: "t:"
     */
		public static readonly String TOPIC = "t:";

		/*
     * The Constant IDGEN: "i:"
     */
		public static readonly String IDGEN = "i:";

		/*
     * The Constant ATOMIC_NUMBER: "a:"
     */
		public static readonly String ATOMIC_NUMBER = "a:";

		/*
     * The Constant AS_MULTIMAP: "u:"
     */
		public static readonly String AS_MULTIMAP = "u:";

		/*
     * The Constant MULTIMAP: "m:u:"
     */
		public static readonly String MULTIMAP = MAP_BASED + AS_MULTIMAP;

		/*
     * The Constant EXECUTOR_SERVICE: "x:"
     */
		public static readonly String EXECUTOR_SERVICE = "x:";

		/*
     * The Constant SEMAPHORE: "smp:"
     */
		public static readonly String SEMAPHORE = "smp:";
		
		public static readonly String COUNT_DOWN_LATCH = "d:";
		

		/*
     * Private constructor to avoid instances of the class.
     */
		private Prefix ()
		{
			
		}
	}
}

