/* 
 * Copyright (c) 2008-2010, Hazel Ltd. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

package com.hazelcast.security;

import java.security.AccessControlException;
import java.security.Permission;
import java.security.PrivilegedExceptionAction;

import javax.security.auth.Subject;

/**
 * IAccessController is responsible for controlling access to protected resources.
 */
public interface IAccessController {
	
	/**
	 * Checks whether current {@link Subject} has been granted specified permission or not.
	 * @param permission 
	 * @throws AccessControlException
	 */
	void checkPermission(Permission permission) throws AccessControlException;
	
	boolean checkPermission(Subject subject, Permission permission);
	
	/**
	 * Perform privileged work as a particular <code>Subject</code>.
	 * @param subject
	 * @param action
	 * @return result returned by the PrivilegedExceptionAction run method.
	 * @throws SecurityException
	 */
	<T> T doAsPrivileged(Subject subject, PrivilegedExceptionAction<T> action) throws Exception, AccessControlException;
}
