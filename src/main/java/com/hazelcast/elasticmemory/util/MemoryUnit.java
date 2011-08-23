package com.hazelcast.elasticmemory.util;


public enum MemoryUnit {
	Bytes {
		public int convert(int value, MemoryUnit m) {return m.toBytes(value);}
		public int toBytes(int value) {return value;}
		public int toKiloBytes(int value) {return value/K;}
		public int toMegaBytes(int value) {return value/M;}
		public int toGigaBytes(int value) {return value/G;}
	},
	KiloBytes {
		public int convert(int value, MemoryUnit m) {return m.toKiloBytes(value);}
		public int toBytes(int value) {return value*K;}
		public int toKiloBytes(int value) {return value;}
		public int toMegaBytes(int value) {return value/K;}
		public int toGigaBytes(int value) {return value/M;}
	},
	MegaBytes {
		public int convert(int value, MemoryUnit m) {return m.toMegaBytes(value);}
		public int toBytes(int value) {return value*M;}
		public int toKiloBytes(int value) {return value*K;}
		public int toMegaBytes(int value) {return value;}
		public int toGigaBytes(int value) {return value/K;}
	},
	GigaBytes {
		public int convert(int value, MemoryUnit m) {return m.toGigaBytes(value);}
		public int toBytes(int value) {return value*G;}
		public int toKiloBytes(int value) {return value*M;}
		public int toMegaBytes(int value) {return value*K;}
		public int toGigaBytes(int value) {return value;}
	};

	private static final int K = 1024;
	private static final int M = K*K;
	private static final int G = K*M;
	
	public abstract int convert(int value, MemoryUnit m);
	public abstract int toBytes(int value);
	public abstract int toKiloBytes(int value);
	public abstract int toMegaBytes(int value);
	public abstract int toGigaBytes(int value);
}
