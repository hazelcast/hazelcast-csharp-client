using System;

namespace Hazelcast.Client
{
	public class MyTypeConverter: Hazelcast.IO.ITypeConverter
	{
		public MyTypeConverter ()
		{
		}
		
		public string getJavaName(Type type){
			if(type.Equals(typeof(Hazelcast.Client.Examples.MyCSharpClass))){
				return "whiteboard.Csharp.MyClass";
			}	
			return null;
		}
		
		public Type getType(String javaName){
			if("whiteboard.Csharp.MyClass".Equals(javaName)){
				return typeof(Hazelcast.Client.Examples.MyCSharpClass);
			}
			return null;
		}
	}
}

