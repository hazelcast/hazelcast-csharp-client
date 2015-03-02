call mvn dependency:get -DrepoUrl=https://oss.sonatype.org/content/repositories/snapshots -Dartifact=com.hazelcast:hazelcast:3.5-SNAPSHOT -Ddest=hazelcast.jar
start "hazelcast" java -cp hazelcast.jar -Dhazelcast.event.queue.capacity=1100000 -Dhazelcast.config=script\hazelcast.xml com.hazelcast.core.server.StartServer
ping -n 10 127.0.0.1 >nul
nunit-console /xml:"console-text.xml" "Hazelcast.Test/Hazelcast.Test.nunit" /noshadow
ping -n 10 127.0.0.1 >nul
taskkill /F /FI "WINDOWTITLE eq hazelcast"
