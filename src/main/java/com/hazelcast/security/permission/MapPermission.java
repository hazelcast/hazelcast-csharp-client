package com.hazelcast.security.permission;

import static com.hazelcast.security.SecurityConstants.*; 

public class MapPermission extends ClusterPermission {
	
	private final static int CREATE 	= 0x1;
	private final static int DESTROY 	= 0x2;
	private final static int PUT 		= 0x4;
	private final static int GET 		= 0x8;
	private final static int REM 		= 0x16;
	
	private final static int ALL 	= PUT | GET | REM | CREATE | DESTROY;

	public MapPermission(String name, String... actions) {
		super(name, actions);
	}

	protected int initMask(String[] actions) {
		int mask = NONE;
		for (int i = 0; i < actions.length; i++) {
			if(ACTION_ALL.equals(actions[i])) {
				return ALL;
			}
			
			if(ACTION_CREATE.equals(actions[i])) {
				mask |= CREATE;
			} else if(ACTION_PUT.equals(actions[i])) {
				mask |= PUT;
			} else if(ACTION_GET.equals(actions[i])) {
				mask |= GET;
			} else if(ACTION_REMOVE.equals(actions[i])) {
				mask |= REM;
			} else if(ACTION_DESTROY.equals(actions[i])) {
				mask |= DESTROY;
			}
		}
		return mask;
	}
}
