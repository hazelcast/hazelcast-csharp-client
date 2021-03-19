import java.io.IOException;
import com.hazelcast.internal.metrics.impl.MetricsCompressor;

public class Program {

    public static void main(String[] args) /*throws IOException*/ {

        int bufSize = 4096; // 4k buffer.
        byte[] buffer = new byte[bufSize]; 
        int bytesAvailable;

        try {
            while ((bytesAvailable = System.in.read(buffer, 0, bufSize)) != -1) {
                System.out.println(String.format("Received %d bytes from CSharp", bytesAvailable));
                // fixme we should accumulate bytes in a buffer!
                //myOutputStream.write(buffer, 0, bytesAvailable);
                //myOutputStream.flush();
            } 
        } catch (IOException e) {}

        TestConsumer consumer = new TestConsumer();
        MetricsCompressor.extractMetrics(buffer, consumer);

        // ok
    }
}