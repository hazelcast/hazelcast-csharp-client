import java.io.IOException;
import com.hazelcast.internal.metrics.impl.MetricsCompressor;

public class Program {

    public static void main(String[] args) throws IOException {

        int bufSize = 4096; // 4k buffer.
        byte[] buffer = new byte[bufSize];
        int bytesAvailable;

        System.out.println("JAVA:BEGIN");

        while ((bytesAvailable = System.in.read(buffer, 0, bufSize)) != -1) {
            System.out.println(String.format("Received %d bytes from CSharp", bytesAvailable));
            // TODO we should accumulate bytes in a buffer!
            //myOutputStream.write(buffer, 0, bytesAvailable);
            //myOutputStream.flush();
        }

        TestConsumer consumer = new TestConsumer();
        MetricsCompressor.extractMetrics(buffer, consumer);

        System.out.println("JAVA:END");

       // ok
    }
}