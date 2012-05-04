using System;
using System.Collections.Concurrent;

namespace Hazelcast.Client
{
	public class TypeRegistry
	{
		
		private static readonly ConcurrentDictionary<String, Type> mapperFromString = new ConcurrentDictionary<String, Type>();
		private static readonly ConcurrentDictionary<Type, String> mapperFromType = new ConcurrentDictionary<Type, String>();
		
		private static Hazelcast.IO.ITypeConverter customTypeConverter;
		
		public TypeRegistry ()
		{
		}
		
		public static void register(String javaClassName, Type type){
			mapperFromString.TryAdd(javaClassName, type);
			mapperFromType.TryAdd(type, javaClassName);
		}
		
		public static string getJavaName(Type type){
			String javaName = customTypeConverter==null?null: customTypeConverter.getJavaName(type);
			if(javaName == null)
				mapperFromType.TryGetValue(type, out javaName);
			
			return javaName;
		}
		
		public static Type getType(String javaName){
			Type type = customTypeConverter==null?null:customTypeConverter.getType(javaName);
			if(type == null)
				mapperFromString.TryGetValue(javaName, out type);
			return type;
		}
		
		public static void setTypeConverter(Hazelcast.IO.ITypeConverter typeConverter){
			customTypeConverter = typeConverter;
		}
	}
}

