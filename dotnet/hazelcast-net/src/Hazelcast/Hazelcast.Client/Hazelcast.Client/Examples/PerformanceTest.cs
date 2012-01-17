using System;
using System.Threading;
using Hazelcast.Client;

namespace Hazelcast.Client.Tests
{
	//[TestFixture()]
	public class PerformanceTest
	{
		static HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		static IMap<String, byte[]> map = client.getMap<String, byte[]>("perf");
		int ENTRY_COUNT = 10000;
		int GET_PERCENTAGE = 40;
		int PUT_PERCENTAGE = 40;
		int VALUE_SIZE = 1000;
		int puts;
		int gets;
		int removes;
		//[Test()]
		public static void Main2 ()
		{
			PerformanceTest test = new PerformanceTest();
			test.start();
			
		}
		
		public void start(){
			int THREAD_COUNT = 4;
			for (int i = 0; i < THREAD_COUNT; i++) {
            	Thread thread = new Thread(new ThreadStart(this.run));
				thread.Start();
				Console.WriteLine("Thread" + i + " started");
            }
			Thread statThread = new Thread(new ThreadStart(this.stats));
			statThread.Start();
		}
		
		public void run() {
            while (true) {
                int key = (int) (GetRandomNumber(0, ENTRY_COUNT));
                int operation = ((int) (GetRandomNumber(0,100)));
                if (operation < GET_PERCENTAGE) {
                    map.get(""+key);
                    Interlocked.Increment(ref gets);
                } else if (operation < GET_PERCENTAGE + PUT_PERCENTAGE) {
                    map.put(""+key, new byte[VALUE_SIZE]);
					Interlocked.Increment(ref puts);
                } else {
                    map.remove(""+key);
					Interlocked.Increment(ref removes);
                }
            }
		}
		
		public void stats(){
			while (true) {
			    Thread.Sleep(10000);
			    Console.WriteLine("PUTS: " + Interlocked.Exchange(ref puts, 0)/10);
				Console.WriteLine("GETS: " + Interlocked.Exchange(ref gets, 0)/10);
				Console.WriteLine("REMOVES: " + Interlocked.Exchange(ref removes, 0)/10);
			}
			
		}
		
		System.Random objRandom = new System.Random(10);


		public int GetRandomNumber(int Low,int High) {
			return objRandom.Next(Low, (High + 1));
		}

	}
}

