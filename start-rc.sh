#!/bin/sh

HAZELCAST_TEST_VERSION="3.8"
HAZELCAST_VERSION="3.8"
HAZELCAST_ENTERPRISE_VERSION="3.8"

HAZELCAST_RC_VERSION="0.2-SNAPSHOT"
SNAPSHOT_REPO="https://oss.sonatype.org/content/repositories/snapshots"
RELEASE_REPO="http://repo1.maven.apache.org/maven2"
ENTERPRISE_REPO="https://repository-hazelcast-l337.forge.cloudbees.com/release/"
ENTERPRISE_SNAPSHOT_REPO="https://repository-hazelcast-l337.forge.cloudbees.com/snapshot/"

mvn dependency:get -DrepoUrl=${SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION} -Ddest=hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar
mvn dependency:get -DrepoUrl=${SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_VERSION} -Ddest=hazelcast-${HAZELCAST_VERSION}.jar
mvn dependency:get -DrepoUrl=${SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests -Ddest=hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar

CLASSPATH="hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar:hazelcast-${HAZELCAST_VERSION}.jar:hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar:test/javaclasses"

if [ -n "${HAZELCAST_ENTERPRISE_KEY}" ]; then
    mvn dependency:get -DrepoUrl=${ENTERPRISE_SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION} -Ddest=hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar
    CLASSPATH="hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar:"${CLASSPATH}
    echo "Starting Remote Controller ... enterprise ..."
else
    echo "Starting Remote Controller ... oss ..."
fi

java -Dhazelcast.enterprise.license.key=${HAZELCAST_ENTERPRISE_KEY} -cp ${CLASSPATH} com.hazelcast.remotecontroller.Main
