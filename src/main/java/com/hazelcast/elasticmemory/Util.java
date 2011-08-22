package com.hazelcast.elasticmemory;

final class Util {

	static int divideAndCeil(double d, int k) {
		return (int) Math.ceil(d / k);
	}
	
	private static int divideAndFloor(double d, int k) {
		return (int) Math.floor(d / k);
	}
	
	static int normalize(int value, int factor) {
		return divideAndCeil(value, factor) * factor;
	}
	
	private Util() {}
}
