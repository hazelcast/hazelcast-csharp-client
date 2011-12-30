package com.hazelcast.elasticmemory.util;

public final class MemorySize {
	private final long value;
	private final MemoryUnit unit;

	public MemorySize(long value, MemoryUnit unit) {
		super();
		this.value = value;
		this.unit = unit;
	}

	public long bytes() {
		return unit.toBytes(value);
	}

	public long kiloBytes() {
		return unit.toKiloBytes(value);
	}

	public long megaBytes() {
		return unit.toMegaBytes(value);
	}

	public long gigaBytes() {
		return unit.toGigaBytes(value);
	}
	
	/**
	 * Parses string representation of a memory size value.
	 * Value may end with one of suffixes;
 	 * <ul> 
	 * <li>'k' or 'K' for 'kilo',</li> 
	 * <li>'m' or 'M' for 'mega',</li>
	 * <li>'g' or 'G' for 'giga'.</li>
	 * </ul>
	 * <p>
	 * Default unit is bytes.
	 * <p>
	 * Examples:
	 * 12345, 12345m, 12345K, 123456G 
	 */
	public static MemorySize parse(String value) {
		return parse(value, MemoryUnit.BYTES);
	}
	
	/**
	 * Parses string representation of a memory size value.
	 * Value may end with one of suffixes;
	 * <ul> 
	 * <li>'k' or 'K' for 'kilo',</li> 
	 * <li>'m' or 'M' for 'mega',</li>
	 * <li>'g' or 'G' for 'giga'.</li>
	 * </ul>
	 * <p>
	 * Uses default unit if value does not contain unit information.
	 * <p>
	 * Examples:
	 * 12345, 12345m, 12345K, 123456G 
	 */
	public static MemorySize parse(String value, MemoryUnit defaultUnit) {
		if(value == null || value.length() == 0) {
			return new MemorySize(0, MemoryUnit.BYTES);
		}
		
		MemoryUnit unit = defaultUnit;
		final char last = value.charAt(value.length() - 1);
		if(!Character.isDigit(last)) {
			value = value.substring(0, value.length() - 1);
			switch (last) {
			case 'g':
			case 'G':
				unit = MemoryUnit.GIGABYTES;
				break;
			
			case 'm':
			case 'M':
				unit = MemoryUnit.MEGABYTES;
				break;
				
			case 'k':
			case 'K':
				unit = MemoryUnit.KILOBYTES;
				break;
				
			default:
				throw new IllegalArgumentException("Could not determine memory unit of " + value + last);
			}
		}
		
		return new MemorySize(Long.parseLong(value), unit);
	}
	
	@Override
	public String toString() {
		return value + " " + unit.toString(); 
	}
}