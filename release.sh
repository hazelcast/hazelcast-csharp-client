mvn clean package resources:resources assembly:assembly -DskipTests
cd target
jar xvf hazelcast-*.zip
cd ..