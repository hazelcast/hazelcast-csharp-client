import com.hazelcast.internal.metrics.MetricConsumer;
import com.hazelcast.internal.metrics.MetricDescriptor;

public class TestConsumer implements MetricConsumer {

    public void consumeLong(MetricDescriptor descriptor, long value) {
        System.out.println(String.format("%s = %d", descriptor.metric(), value));
    }

    public void consumeDouble(MetricDescriptor descriptor, double value) {
        System.out.println(String.format("%s = %f", descriptor.metric(), value));
    }
}