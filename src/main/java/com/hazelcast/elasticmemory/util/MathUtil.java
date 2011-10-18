package com.hazelcast.elasticmemory.util;

public final class MathUtil {

	public static int divideByAndCeil(double d, int k) {
		return (int) Math.ceil(d / k);
	}
	
	public static long divideByAndCeil(double d, long k) {
		return (long) Math.ceil(d / k);
	}
	
	public static int divideByAndRound(double d, int k) {
		return (int) Math.rint(d / k);
	}
	
	public static long divideByAndRound(double d, long k) {
		return (long) Math.rint(d / k);
	}
	
	public static long divideByAndFloor(double d, long k) {
		return (long) Math.floor(d / k);
	}
	
	public static int divideByAndFloor(double d, int k) {
		return (int) Math.floor(d / k);
	}
	
	public static int normalize(int value, int factor) {
		return divideByAndRound(value, factor) * factor;
	}
	
	public static boolean isPowerOf2(long x) {
		if(x <= 0) return false;
		return (x & (x - 1)) == 0;
	}
	
	private MathUtil() {}
}
