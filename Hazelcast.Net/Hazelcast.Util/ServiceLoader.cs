namespace Hazelcast.Util
{
    public class ServiceLoader
    {
        //    private static final ILogger logger = Logger.getLogger(ServiceLoader.class);
        //
        //    public static <T> T load(Class<T> clazz, String factoryId, ClassLoader classLoader) throws Exception {
        //        final Iterator<T> iter = iterator(clazz, factoryId, classLoader);
        //        if (iter.hasNext()) {
        //            return iter.next();
        //        }
        //        return null;
        //    }
        //
        //    public static <T> Iterator<T> iterator(final Class<T> clazz, final String factoryId, final ClassLoader classLoader) throws Exception {
        //        final Set<ServiceDefinition> serviceDefinitions = parse(factoryId, classLoader);
        //        // If we are in a multi class-loader environment like JEE we need to ask the Hazelcast class-loader for default services
        //        final ClassLoader systemClassLoader = ServiceLoader.class.getClassLoader();
        //        if (classLoader != null && systemClassLoader != classLoader) {
        //            final Set<ServiceDefinition> systemDefinitions = parse(factoryId, systemClassLoader);
        //            serviceDefinitions.addAll(systemDefinitions);
        //        }
        //        if (serviceDefinitions.isEmpty()) {
        //            Logger.getLogger(ServiceLoader.class).warning("Service loader could not load 'META-INF/services/" + factoryId + "' It may be empty or does not exist.");
        //        }
        //
        //        return new Iterator<T>() {
        //            final Iterator<ServiceDefinition> iterator = serviceDefinitions.iterator();
        //
        //            public boolean hasNext() {
        //                return iterator.hasNext();
        //            }
        //
        //            public T next() {
        //                final ServiceDefinition definition = iterator.next();
        //                try {
        //                    String className = definition.className;
        //                    ClassLoader classLoader = definition.classLoader;
        //                    return clazz.cast(ClassLoaderUtil.newInstance(classLoader, className));
        //                } catch (Exception e) {
        //                    throw new HazelcastException(e);
        //                }
        //            }
        //
        //            public void remove() {
        //                throw new UnsupportedOperationException();
        //            }
        //        };
        //    }
        //
        //    private static Set<ServiceDefinition> parse(String factoryId, ClassLoader classLoader) {
        //        final ClassLoader cl = (classLoader == null) ? Thread.currentThread().getContextClassLoader() : classLoader;
        //        final String resourceName = "META-INF/services/" + factoryId;
        //        try {
        //            final Enumeration<URL> configs;
        //            if (cl != null) {
        //                configs = cl.getResources(resourceName);
        //            } else {
        //                configs = ClassLoader.getSystemResources(resourceName);
        //            }
        //            final Set<ServiceDefinition> names = new HashSet<ServiceDefinition>();
        //            while (configs.hasMoreElements()) {
        //                URL url = configs.nextElement();
        //                BufferedReader r = null;
        //                try {
        //                    r = new BufferedReader(new InputStreamReader(url.openStream(), "UTF-8"));
        //                    while (true) {
        //                        String line = r.readLine();
        //                        if (line == null) {
        //                            break;
        //                        }
        //                        int comment = line.indexOf('#');
        //                        if (comment >= 0) {
        //                            line = line.substring(0, comment);
        //                        }
        //                        String name = line.trim();
        //                        if (name.length() == 0) {
        //                            continue;
        //                        }
        //                        names.add(new ServiceDefinition(name, classLoader));
        //                    }
        //                } finally {
        //                    IOUtil.closeResource(r);
        //                }
        //            }
        //            return names;
        //        } catch (Exception e) {
        //            logger.severe(e);
        //        }
        //        return Collections.emptySet();
        //    }
        //
        //    private static class ServiceDefinition {
        //        private final String className;
        //        private final ClassLoader classLoader;
        //
        //        private ServiceDefinition(String className, ClassLoader classLoader) {
        //            this.className = className;
        //            this.classLoader = classLoader;
        //        }
        //
        //        @Override
        //        public boolean equals(Object o) {
        //            if (this == o) return true;
        //            if (o == null || getClass() != o.getClass()) return false;
        //
        //            ServiceDefinition that = (ServiceDefinition) o;
        //
        //            if (className != null ? !className.equals(that.className) : that.className != null) return false;
        //
        //            return true;
        //        }
        //
        //        @Override
        //        public int hashCode() {
        //            return className != null ? className.hashCode() : 0;
        //        }
        //    }
    }
}