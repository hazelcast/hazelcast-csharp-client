using System;
using System.Threading;
using System.Collections.Concurrent;
using Hazelcast.Client;
using Hazelcast.Core;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Client.Tests
{
	//[TestFixture()]
	public class PerformanceTest
	{
		static HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
		static IMap<String, byte[]> map = client.getMap<String, byte[]>("perf");
		static int ENTRY_COUNT = 10000;
		static int GET_PERCENTAGE = 40;
		static int THREAD_COUNT = 40;
		static int PUT_PERCENTAGE = 40;
		static int VALUE_SIZE = 1000;
		static int puts;
		static int gets;
		static int removes;
		static bool debug = false;
		List<Thread> threads = new List<Thread>();
		//[Test()]
		public static void Main (String[] args)
		{
			Console.WriteLine("Command line arguments:");
			
			if (args != null && args.Length > 0) {
            foreach (String s in args) {
					Console.WriteLine(s);
                String arg = s;
				arg = arg.Trim();
                if (arg.StartsWith("t")) {
                    THREAD_COUNT = int.Parse(arg.Substring(1));
                } else if (arg.StartsWith("c")) {
                    ENTRY_COUNT = int.Parse(arg.Substring(1));
                } else if (arg.StartsWith("v")) {
                    VALUE_SIZE = int.Parse(arg.Substring(1));
                } else if (arg.StartsWith("g")) {
                    GET_PERCENTAGE = int.Parse(arg.Substring(1));
                } else if (arg.StartsWith("p")) {
                    PUT_PERCENTAGE = int.Parse(arg.Substring(1));
                } else if (arg.StartsWith("d")) {
                    debug = true;
                }
            }
        } 
			PerformanceTest test = new PerformanceTest();
			test.start();
		}
	
		public void start(){
			for (int i = 0; i < THREAD_COUNT; i++) {
            	Thread thread = new Thread(new ThreadStart(this.run));
				threads.Add(thread);
				thread.Start();
				thread.Name = "HZ_CLIENT_Thread_"+i;
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
				if(debug)
					Console.WriteLine("Doing op " + Thread.CurrentThread.Name);
				//Thread.Sleep(10);
            }
		}
		
		public void stats(){
			while (true) {
			    Thread.Sleep(10000);
			    Console.WriteLine("PUTS: " + Interlocked.Exchange(ref puts, 0)/10);
				Console.WriteLine("GETS: " + Interlocked.Exchange(ref gets, 0)/10);
				Console.WriteLine("REMOVES: " + Interlocked.Exchange(ref removes, 0)/10);
				//Call.printAndReset();
			}
			
		}
		
			private static String GetStackTrace(Thread t)
	    {
			if(t==null){
				return "Thread is null";
			}else
				Console.WriteLine("Dumping " + t.Name);
	        t.Suspend();
	        var trace1 = new StackTrace(t, true);
	        t.Resume();
	
	        String  text1 = System.Environment.NewLine;
	        var builder1 = new StringBuilder(255);
	        for (Int32 num1 = 0; (num1 < trace1.FrameCount); num1++)
	        {
	            StackFrame  frame1 = trace1.GetFrame(num1);
	            builder1.Append("   at ");
	            System.Reflection.MethodBase  base1 = frame1.GetMethod();
	            Type  type1 = base1.DeclaringType;
	            if (type1 != null)
	            {
	                String  text2 = type1.Namespace;
	                if (text2 != null)
	                {
	                    builder1.Append(text2);
	                    builder1.Append(".");                                                
	                }
	                builder1.Append(type1.Name);
	                builder1.Append(".");
	            }
	            builder1.Append(base1.Name);
	            builder1.Append("(");
	            System.Reflection.ParameterInfo [] infoArray1 = base1.GetParameters();
	            for (Int32 num2 = 0; (num2 < infoArray1.Length); num2++)
	            {
	                String text3 = "<UnknownType>";
	                if (infoArray1[num2].ParameterType != null)
	                {
	                                text3 = infoArray1[num2].ParameterType.Name;
	                }
	                builder1.Append(String.Concat(((num2 != 0) ? ", " : ""), text3, " ", infoArray1[num2].Name));
	            }
	            builder1.Append(")");
	            if (frame1.GetILOffset() != -1)
	            {
	                String text4 = null;
	                try
	                {
	                    text4 = frame1.GetFileName();
	                }
	                catch (System.Security.SecurityException)
	                {
	                }
	                if (text4 != null)
	                {
	                    builder1.Append(String.Concat(" in ", text4, ":line ", frame1.GetFileLineNumber().ToString()));
	                }
	            }
	            if (num1 != (trace1.FrameCount - 1))
	            {
	                builder1.Append(text1);
	            }
	        }
	        return builder1.ToString();
	    }
		
	

		
		System.Random objRandom = new System.Random(10);


		public int GetRandomNumber(int Low,int High) {
			return objRandom.Next(Low, (High + 1));
		}

	}
}

