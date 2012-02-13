package com.hazelcast.security;

import com.hazelcast.impl.CallContext;
import com.hazelcast.impl.ThreadContext;
import com.hazelcast.logging.ILogger;

import javax.security.auth.Subject;
import java.security.AccessControlException;
import java.security.Permission;
import java.security.PermissionCollection;
import java.security.PrivilegedExceptionAction;
import java.util.logging.Level;

public class AccessControllerImpl implements IAccessController {

    private final ILogger logger;

    private final IPermissionPolicy policy;

    public AccessControllerImpl(SecurityContextImpl context, IPermissionPolicy policy) {
        super();
        logger = context.getLogger(IAccessController.class.getName());
        this.policy = policy;
    }

    public void checkPermission(Permission permission) throws AccessControlException {
        final CallContext ctx = ThreadContext.get().getCallContext();
        final Subject subject = ctx.getSubject();
        if (subject == null) {
            throw new AccessControlException("Unauthorized access!", permission);
        }
        if (!checkPermission(subject, permission)) {
            throw new AccessControlException("Permission " + permission + " denied!", permission);
        }
    }

    public boolean checkPermission(Subject subject, Permission permission) {
        PermissionCollection coll = policy.getPermissions(subject, permission.getClass());
        return coll != null ? coll.implies(permission) : false;
    }

    public <T> T doAsPrivileged(Subject subject, PrivilegedExceptionAction<T> action) throws Exception, AccessControlException {
        final CallContext ctx = ThreadContext.get().getCallContext();
        Subject s = ctx.getSubject();
        if (s != null && !s.equals(subject)) {
            logger.log(Level.WARNING, "Here is another subject bound into context before!");
        }
        ctx.setSubject(subject);
        try {
            return action.run();
        } finally {
            ctx.setSubject(null);
        }
    }
}
