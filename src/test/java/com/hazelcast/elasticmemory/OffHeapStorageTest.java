package com.hazelcast.elasticmemory;

import java.util.Random;

import org.junit.Test;

import static org.junit.Assert.*;

import com.hazelcast.elasticmemory.storage.*;
import com.hazelcast.elasticmemory.util.*;

public class OffHeapStorageTest {
	
	@Test
	public void testPutGetRemove() {
		final int chunkSize = 2;
		final Storage s = new OffHeapStorage(32, chunkSize);
		final Random rand = new Random();
		final int k = 3072;
		
		byte[] data = new byte[k];
		rand.nextBytes(data);
		final int hash = rand.nextInt();
		
		final EntryRef ref = s.put(hash, data);
		assertEquals(k, ref.length);
		assertEquals((int) Math.ceil((double) k / (chunkSize * 1024)), ref.getChunkCount());
		
		byte[] result = s.get(hash, ref);
		assertArrayEquals(data, result);
		
		s.remove(hash, ref);
		assertNull(s.get(hash, ref));
	}

	final MemorySize total = new MemorySize(32, MemoryUnit.MEGABYTES);
	final MemorySize chunk = new MemorySize(1, MemoryUnit.KILOBYTES);
	
	@Test
	public void testFillUpBuffer() {
		final int count = (int) (total.kiloBytes() / chunk.kiloBytes());
		fillUpBuffer(count);
	}
	
	@Test(expected = OffHeapOutOfMemoryError.class)
	public void testBufferOverFlow() {
		final int count = (int) (total.kiloBytes() / chunk.kiloBytes());
		fillUpBuffer(count + 1);
	}
	
	private void fillUpBuffer(int count) {
		final Storage s = new OffHeapStorage((int) total.megaBytes(), 2, (int) chunk.kiloBytes());
		byte[] data = new byte[(int) chunk.bytes()];
		for (int i = 0; i < count; i++) {
			s.put(i, data);
		}
	}
}
