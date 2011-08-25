package com.hazelcast.elasticmemory.util;

public final class MathUtil {

	public static int divideByAndCeil(double d, int k) {
		return (int) Math.ceil(d / k);
	}
	
	public static int divideByAndRound(double d, int k) {
		return (int) Math.rint(d / k);
	}
	
	public static int divideByAndFloor(double d, int k) {
		return (int) Math.floor(d / k);
	}
	
	public static int normalize(int value, int factor) {
		return divideByAndRound(value, factor) * factor;
	}
	
	private MathUtil() {}
}
