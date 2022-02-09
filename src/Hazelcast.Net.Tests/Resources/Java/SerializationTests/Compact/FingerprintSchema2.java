import java.io.IOException;
//import com.hazelcast.internal.serialization.impl.compact.RabinFingerprint;
import java.util.TreeMap;
import com.hazelcast.internal.serialization.impl.compact.FieldDescriptor;
import com.hazelcast.internal.serialization.impl.compact.Schema;
import com.hazelcast.nio.serialization.FieldKind;

public class FingerprintSchema2 {

    public static void main(String[] args) /*throws IOException*/ {

        TreeMap<String, FieldDescriptor> fields = new TreeMap<String, FieldDescriptor>();
        fields.put("value", new FieldDescriptor("value", FieldKind.INT32));
        Schema schema = new Schema("foo", fields);
        System.out.println(schema.getSchemaId());
    }
}

