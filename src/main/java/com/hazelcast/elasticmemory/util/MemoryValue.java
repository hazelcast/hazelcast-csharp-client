package com.hazelcast.elasticmemory.util;

public final class MemoryValue {
	private final int value;
	private final MemoryUnit unit;

	public MemoryValue(int value, MemoryUnit unit) {
		super();
		this.value = value;
		this.unit = unit;
	}

	public int bytes() {
		return unit.toBytes(value);
	}

	public int kiloBytes() {
		return unit.toKiloBytes(value);
	}

	public int megaBytes() {
		return unit.toMegaBytes(value);
	}

	public int gigaBytes() {
		return unit.toGigaBytes(value);
	}
}