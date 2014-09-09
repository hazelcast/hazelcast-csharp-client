call mvn dependency:get -DrepoUrl=https://oss.sonatype.org/content/repositories/snapshots -Dartifact=com.hazelcast:hazelcast:3.3.1-SNAPSHOT -Ddest=hazelcast.jar
start "hazelcast" java -cp hazelcast.jar com.hazelcast.core.server.StartServer
ping -n 10 127.0.0.1 >nul
nunit-console /xml:"console-text.xml" "Hazelcast.Test/Hazelcast.Test.nunit"
ping -n 10 127.0.0.1 >nul
taskkill /F /FI "WINDOWTITLE eq hazelcast"