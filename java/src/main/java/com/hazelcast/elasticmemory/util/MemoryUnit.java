package com.hazelcast.elasticmemory.util;

import static com.hazelcast.elasticmemory.util.MathUtil.*;

public enum MemoryUnit {
	BYTES {
		public long convert(long value, MemoryUnit m) {return m.toBytes(value);}
		public long toBytes(long value) {return value;}
		public long toKiloBytes(long value) {return divideByAndRound(value, K);}
		public long toMegaBytes(long value) {return divideByAndRound(value, M);}
		public long toGigaBytes(long value) {return divideByAndRound(value, G);}
	},
	KILOBYTES {
		public long convert(long value, MemoryUnit m) {return m.toKiloBytes(value);}
		public long toBytes(long value) {return value*K;}
		public long toKiloBytes(long value) {return value;}
		public long toMegaBytes(long value) {return divideByAndRound(value, K);}
		public long toGigaBytes(long value) {return divideByAndRound(value, M);}
	},
	MEGABYTES {
		public long convert(long value, MemoryUnit m) {return m.toMegaBytes(value);}
		public long toBytes(long value) {return value*M;}
		public long toKiloBytes(long value) {return value*K;}
		public long toMegaBytes(long value) {return value;}
		public long toGigaBytes(long value) {return divideByAndRound(value, K);}
	},
	GIGABYTES {
		public long convert(long value, MemoryUnit m) {return m.toGigaBytes(value);}
		public long toBytes(long value) {return value*G;}
		public long toKiloBytes(long value) {return value*M;}
		public long toMegaBytes(long value) {return value*K;}
		public long toGigaBytes(long value) {return value;}
	};

	private static final int K = 1024;
	private static final int M = K*K;
	private static final int G = K*M;
	
	public abstract long convert(long value, MemoryUnit m);
	public abstract long toBytes(long value);
	public abstract long toKiloBytes(long value);
	public abstract long toMegaBytes(long value);
	public abstract long toGigaBytes(long value);
}
