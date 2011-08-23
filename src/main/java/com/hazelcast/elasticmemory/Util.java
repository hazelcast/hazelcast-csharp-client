package com.hazelcast.elasticmemory;

public final class Util {

	public static int divideAndCeil(double d, int k) {
		return (int) Math.ceil(d / k);
	}
	
	public static int divideAndFloor(double d, int k) {
		return (int) Math.floor(d / k);
	}
	
	public static int normalize(int value, int factor) {
		return divideAndCeil(value, factor) * factor;
	}
	
	private Util() {}
}
