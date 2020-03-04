#!/usr/bin/env bash

cleanup() {
    if [ "x${serverPid}" != "x" ]
    then
        echo "Killing server with pid ${serverPid}"
        kill -9 ${serverPid}
    fi
}

trap cleanup EXIT

usage() {
    printf "Build script for Hazelcast .Net Client. \nYou can build and run unit tests.\n\nUsage:"
    echo "$build.sh [--run-tests] [--server-version=SERVER_VERSION] [--enterprise] [--enterprise-key=HAZELCAST_ENTERPRISE_LICENSE_KEY]"
    echo "--run-tests:      run unit tests after a successful build."
    echo "--server-version: server version to be used for unit tests, latest server will be used if left empty."
    echo "--enterprise:     Enterprise server will be used in unit tests if selected. If this option is selected, you should provide a license key."
    echo "--enterprise-key: Optional enterprise licence key for Hazelcast Enterprise server."
    echo "                  As an alternative, you can set the license to  environment var HAZELCAST_ENTERPRISE_KEY4"
    printf "\nSample usage for running unit tests with OSS server: \n\$build.sh --run-tests --server-version=LATEST\n"
}

HZ_VERSION="LATEST"

for i in "$@"
do
	case $i in
		--server-version=*)
		    HZ_VERSION=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--run-tests)
		    RUN_TESTS=true
		;;
		--enterprise)
		    ENTERPRISE=true
		;;
		--enterprise-key=*)
		    ENTERPRISE=true
		    ENTERPRISE_KEY=`echo $i | sed 's/[-a-zA-Z0-9]*=//'`
		;;
		--help)
		    usage
			exit 1
		;;
		*)
			# unknown option
			echo "Unrecognised option $i"
			usage
			exit 1
		;;
	esac
done

#DOTNET BUILD

dotnet build Hazelcast.Test/Hazelcast.Test.csproj --configuration Release --framework netcoreapp2.1

if [[ -z "${RUN_TESTS}" ]]
then
    exit 1
fi

if [[ "${HZ_VERSION}" == "LATEST" ]]; then
    echo "Running with latest hazelcast server, to choose another version use the --server-version parameter"
fi

if [[ ${ENTERPRISE} ]]; then
    if [[ -z ${ENTERPRISE_KEY} && -n ${HAZELCAST_ENTERPRISE_KEY} ]]; then
        ENTERPRISE_KEY=${HAZELCAST_ENTERPRISE_KEY}
    fi
    if [[ -z ${ENTERPRISE_KEY} ]]; then
        echo "Enterprise server selected but no licence key is provided. Either provide one with --enterprise-key or set to HAZELCAST_ENTERPRISE_KEY4 environment var."
        exit 1
    fi
fi

# clear rogue environment variable
FrameworkPathOverride=""

HAZELCAST_TEST_VERSION=${HZ_VERSION}
HAZELCAST_ENTERPRISE_TEST_VERSION=${HZ_VERSION}
HAZELCAST_VERSION=${HZ_VERSION}
HAZELCAST_ENTERPRISE_VERSION=${HZ_VERSION}
HAZELCAST_RC_VERSION="0.6-SNAPSHOT"
SNAPSHOT_REPO="https://oss.sonatype.org/content/repositories/snapshots"
RELEASE_REPO="http://repo1.maven.apache.org/maven2"
ENTERPRISE_RELEASE_REPO="https://repository.hazelcast.com/release/"
ENTERPRISE_SNAPSHOT_REPO="https://repository.hazelcast.com/snapshot/"

if [[ ${HZ_VERSION} == *-SNAPSHOT ]]
then
	REPO=${SNAPSHOT_REPO}
	ENTERPRISE_REPO=${ENTERPRISE_SNAPSHOT_REPO}
else
	REPO=${RELEASE_REPO}
	ENTERPRISE_REPO=${ENTERPRISE_RELEASE_REPO}
fi

if [ -f "hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar" ]; then
    echo "remote controller already exist, not downloading from maven."
else
    echo "Downloading: remote-controller jar com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION}"
    mvn -q dependency:get -DrepoUrl=${SNAPSHOT_REPO} -Dartifact=com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION} -Ddest=hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar
    if [ $? -ne 0 ]; then
        echo "Failed download remote-controller jar com.hazelcast:hazelcast-remote-controller:${HAZELCAST_RC_VERSION}"
        exit 1
    fi
fi

if [ -f "hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar" ]; then
    echo "hazelcast-test.jar already exists, not downloading from maven."
else
    echo "Downloading: hazelcast test jar com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests"
    mvn -q dependency:get -DrepoUrl=${REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests -Ddest=hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar
    if [ $? -ne 0 ]; then
        echo "Failed download hazelcast test jar com.hazelcast:hazelcast:${HAZELCAST_TEST_VERSION}:jar:tests"
        exit 1
    fi
fi

CLASSPATH="hazelcast-remote-controller-${HAZELCAST_RC_VERSION}.jar:hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar:test/javaclasses"

if [ -n "${ENTERPRISE_KEY}" ]; then
    if [ -f "hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar" ]; then
        echo "hazelcast-enterprise.jar already exists, not downloading from maven."
    else
        echo "Downloading: hazelcast enterprise jar com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION}"
        mvn -q dependency:get -DrepoUrl=${ENTERPRISE_REPO} -Dartifact=com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION} -Ddest=hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar
        if [ $? -ne 0 ]; then
            echo "Failed download hazelcast enterprise jar com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_VERSION}"
            exit 1
        fi
    fi
    if [ -f "hazelcast-enterprise-${HAZELCAST_ENTERPRISE_TEST_VERSION}-tests.jar" ]; then
        echo "hazelcast-enterprise-test.jar already exists, not downloading from maven."
    else
        echo "Downloading: hazelcast enterprise test jar com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_TEST_VERSION}:jar:tests"
        mvn -q dependency:get -DrepoUrl=${ENTERPRISE_REPO} -Dartifact=com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_TEST_VERSION}:jar:tests -Ddest=hazelcast-enterprise-${HAZELCAST_ENTERPRISE_TEST_VERSION}-tests.jar
        if [ $? -ne 0 ]; then
            echo "Failed download hazelcast enterprise test jar com.hazelcast:hazelcast-enterprise:${HAZELCAST_ENTERPRISE_TEST_VERSION}:jar:tests"
            exit 1
        fi
    fi

    CLASSPATH="hazelcast-enterprise-${HAZELCAST_ENTERPRISE_VERSION}.jar:hazelcast-enterprise-${HAZELCAST_ENTERPRISE_TEST_VERSION}-tests.jar:"${CLASSPATH}
    echo "Starting Remote Controller ... enterprise ..."
else
    if [ -f "hazelcast-${HAZELCAST_VERSION}.jar" ]; then
        echo "hazelcast.jar already exists, not downloading from maven."
    else
        echo "Downloading: hazelcast jar com.hazelcast:hazelcast:${HAZELCAST_VERSION}"
        mvn -q dependency:get -DrepoUrl=${REPO} -Dartifact=com.hazelcast:hazelcast:${HAZELCAST_VERSION} -Ddest=hazelcast-${HAZELCAST_VERSION}.jar
        if [ $? -ne 0 ]; then
            echo "Failed download hazelcast jar com.hazelcast:hazelcast:${HAZELCAST_VERSION}"
            exit 1
        fi
    fi
    CLASSPATH="hazelcast-${HAZELCAST_VERSION}.jar:"${CLASSPATH}
    echo "Starting Remote Controller ... oss ..."
fi

java -Dhazelcast.enterprise.license.key=${ENTERPRISE_KEY} -cp ${CLASSPATH} com.hazelcast.remotecontroller.Main>rc_stdout.log 2>rc_stderr.log &
serverPid=$!

sleep 15

dotnet test Hazelcast.Test/Hazelcast.Test.csproj -c Release --no-build --no-restore -f netcoreapp2.1 -v n
