package com.hazelcast.security.impl;

import com.hazelcast.config.PermissionConfig;
import com.hazelcast.config.SecurityConfig;
import com.hazelcast.logging.ILogger;
import com.hazelcast.logging.Logger;
import com.hazelcast.security.ClusterPrincipal;
import com.hazelcast.security.IPermissionPolicy;
import com.hazelcast.security.permission.AllPermissions;
import com.hazelcast.security.permission.AllPermissions.AllPermissionsCollection;
import com.hazelcast.security.permission.ClusterPermission;
import com.hazelcast.security.permission.ClusterPermissionCollection;
import com.hazelcast.security.permission.DenyAllPermissionCollection;

import javax.security.auth.Subject;
import java.security.Permission;
import java.security.PermissionCollection;
import java.security.Principal;
import java.util.Collection;
import java.util.Properties;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.logging.Level;

import static com.hazelcast.security.SecurityUtil.*;


public class DefaultPermissionPolicy implements IPermissionPolicy {

    private static final ILogger logger = Logger.getLogger(DefaultPermissionPolicy.class.getName());
    private static final PermissionCollection DENY_ALL = new DenyAllPermissionCollection();
    private static final PermissionCollection ALLOW_ALL = new AllPermissionsCollection(true);

    // Configured permissions
    final ConcurrentMap<PrincipalKey, PermissionCollection> configPermissions = new ConcurrentHashMap<PrincipalKey, PermissionCollection>();

    // Principal permissions
    final ConcurrentMap<String, PrincipalPermissionsHolder> principalPermissions = new ConcurrentHashMap<String, PrincipalPermissionsHolder>();

    public void configure(SecurityConfig securityConfig, Properties properties) {
        logger.log(Level.FINEST, "Configuring and initializing policy.");
        final Set<PermissionConfig> permissionConfigs = securityConfig.getClientPermissionConfigs();
        for (PermissionConfig permCfg : permissionConfigs) {
            final ClusterPermission permission = createPermission(permCfg);
            final String principal = permCfg.getPrincipal() != null ? permCfg.getPrincipal() : "*"; // allow all principals
            final Set<String> endpoints = permCfg.getEndpoints();
            PermissionCollection coll = null;

            if (endpoints.isEmpty()) {
                endpoints.add("*.*.*.*"); // allow all endpoints
            }
            for (final String endpoint : endpoints) {
                final PrincipalKey key = new PrincipalKey(principal, endpoint);
                coll = configPermissions.get(key);
                if (coll == null) {
                    coll = new ClusterPermissionCollection();
                    configPermissions.put(key, coll);
                }
                coll.add(permission);
            }
        }
    }

    public PermissionCollection getPermissions(Subject subject, Class<? extends Permission> type) {
        final ClusterPrincipal principal = getPrincipal(subject);
        if (principal == null) {
            return DENY_ALL;
        }

        ensurePrincipalPermissions(principal);
        final PrincipalPermissionsHolder permissionsHolder = principalPermissions.get(principal.getName());
        if (!permissionsHolder.prepared) {
            synchronized (permissionsHolder) {
                if (!permissionsHolder.prepared) {
                    try {
                        permissionsHolder.wait();
                    } catch (InterruptedException ignored) {
                    }
                }
            }
        }

        if (permissionsHolder.hasAllPermissions) {
            return ALLOW_ALL;
        }
        PermissionCollection coll = permissionsHolder.permissions.get(type);
        if (coll == null) {
            coll = DENY_ALL;
            permissionsHolder.permissions.putIfAbsent(type, coll);
        }
        return coll;
    }

    private ClusterPrincipal getPrincipal(Subject subject) {
        final Set<Principal> principals = subject.getPrincipals();
        for (Principal p : principals) {
            if (p instanceof ClusterPrincipal) {
                return (ClusterPrincipal) p;
            }
        }
        return null;
    }

    private void ensurePrincipalPermissions(ClusterPrincipal principal) {
        if (principal != null) {
            final String fullName = principal.getName();
            if (!principalPermissions.containsKey(fullName)) {
                final PrincipalPermissionsHolder permissionsHolder = new PrincipalPermissionsHolder();
                if (principalPermissions.putIfAbsent(fullName, permissionsHolder) != null) {
                    return;
                }

                final String endpoint = principal.getEndpoint();
                final String principalName = principal.getPrincipal();
                try {
                    logger.log(Level.FINEST, "Preparing permissions for: " + fullName);
                    final ClusterPermissionCollection allMatchingPermissionsCollection = new ClusterPermissionCollection();
                    final Set<PrincipalKey> keys = configPermissions.keySet();
                    for (PrincipalKey key : keys) {
                        if (nameMatches(principalName, key.principal)
                                && addressMatches(endpoint, key.endpoint)) {
                            allMatchingPermissionsCollection.add(configPermissions.get(key));
                        }
                    }

                    final Set<Permission> allMatchingPermissions = allMatchingPermissionsCollection.getPermissions();
                    for (Permission perm : allMatchingPermissions) {
                        if (perm instanceof AllPermissions) {
                            permissionsHolder.permissions.clear();
                            permissionsHolder.hasAllPermissions = true;
                            logger.log(Level.FINEST, "Granted all-permissions to: " + fullName);
                            return;
                        }
                        Class<? extends Permission> type = perm.getClass();
                        ClusterPermissionCollection coll = (ClusterPermissionCollection) permissionsHolder.permissions.get(type);
                        if (coll == null) {
                            coll = new ClusterPermissionCollection(type);
                            permissionsHolder.permissions.put(type, coll);
                        }
                        coll.add(perm);
                    }

                    logger.log(Level.FINEST, "Compacting permissions for: " + fullName);
                    final Collection<PermissionCollection> principalCollections = permissionsHolder.permissions.values();
                    for (PermissionCollection coll : principalCollections) {
                        ((ClusterPermissionCollection) coll).compact();
                    }

                } finally {
                    synchronized (permissionsHolder) {
                        permissionsHolder.prepared = true;
                        permissionsHolder.notifyAll();
                    }
                }
            }
        }
    }

    private class PrincipalKey {
        final String principal;
        final String endpoint;

        PrincipalKey(String principal, String endpoint) {
            this.principal = principal;
            this.endpoint = endpoint;
        }

        public int hashCode() {
            final int prime = 31;
            int result = 1;
            result = prime * result
                    + ((endpoint == null) ? 0 : endpoint.hashCode());
            result = prime * result
                    + ((principal == null) ? 0 : principal.hashCode());
            return result;
        }

        public boolean equals(Object obj) {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (getClass() != obj.getClass())
                return false;
            PrincipalKey other = (PrincipalKey) obj;
            if (endpoint == null) {
                if (other.endpoint != null)
                    return false;
            } else if (!endpoint.equals(other.endpoint))
                return false;
            if (principal == null) {
                if (other.principal != null)
                    return false;
            } else if (!principal.equals(other.principal))
                return false;
            return true;
        }
    }

    private class PrincipalPermissionsHolder {
        volatile boolean prepared = false;
        boolean hasAllPermissions = false;
        final ConcurrentMap<Class<? extends Permission>, PermissionCollection> permissions =
                new ConcurrentHashMap<Class<? extends Permission>, PermissionCollection>();
    }

    public void destroy() {
        principalPermissions.clear();
        configPermissions.clear();
    }
}
