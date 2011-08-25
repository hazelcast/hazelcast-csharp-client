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
	
	public static MemoryValue parseMemoryValue(String value) {
		return parseMemoryValue(value, MemoryUnit.BYTES);
	}
	
	public static MemoryValue parseMemoryValue(String value, MemoryUnit defaultUnit) {
		if(value == null || value.length() == 0) {
			return new MemoryValue(0, MemoryUnit.BYTES);
		} else if(value.endsWith("g") || value.endsWith("G")) {
			return new MemoryValue(Integer.parseInt(value.substring(0, value.length()-1)), MemoryUnit.GIGABYTES);
		} else if(value.endsWith("m") || value.endsWith("M")) {
			return new MemoryValue(Integer.parseInt(value.substring(0, value.length()-1)), MemoryUnit.MEGABYTES);
		} else if(value.endsWith("k") || value.endsWith("K")) {
			return new MemoryValue(Integer.parseInt(value.substring(0, value.length()-1)), MemoryUnit.KILOBYTES);
		} else {
			return new MemoryValue(Integer.parseInt(value), defaultUnit);
		}
	}
}