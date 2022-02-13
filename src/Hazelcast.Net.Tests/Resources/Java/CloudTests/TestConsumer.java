import com.hazelcast.internal.metrics.MetricConsumer;
import com.hazelcast.internal.metrics.MetricDescriptor;

public class TestConsumer implements MetricConsumer {

    public void consumeLong(MetricDescriptor descriptor, long value) {
        System.out.println(String.format("prefix   = %s", descriptor.prefix()));
        System.out.println(String.format("disc.key = %s", descriptor.discriminator()));
        System.out.println(String.format("disc.val = %s", descriptor.discriminatorValue()));
        System.out.println(String.format("string   = %s", descriptor.metricString()));
        System.out.println(String.format("%s = %d", descriptor.metric(), value));
    }

    public void consumeDouble(MetricDescriptor descriptor, double value) {
        System.out.println(String.format("%s = %f", descriptor.metric(), value));
    }
}