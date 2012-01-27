using System.Collections.Generic;
using System;

namespace Hazelcast.Client
{
	public static class Extensions
	{
		public static void Shuffle<T>(this System.Collections.Generic.IList<T> list)  
		{  
		    Random rng = new Random();  
		    int n = list.Count;  
		    while (n > 1) {  
		        n--;  
		        int k = rng.Next(n + 1);  
		        T value = list[k];  
		        list[k] = list[n];  
		        list[n] = value;  
		    }  
		}

	}
}

