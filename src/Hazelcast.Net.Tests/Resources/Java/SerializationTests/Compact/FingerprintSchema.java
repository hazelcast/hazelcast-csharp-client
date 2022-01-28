import java.io.IOException;
//import com.hazelcast.internal.serialization.impl.compact.RabinFingerprint;
import java.util.TreeMap;
import com.hazelcast.internal.serialization.impl.compact.FieldDescriptor;
import com.hazelcast.internal.serialization.impl.compact.Schema;
import com.hazelcast.nio.serialization.FieldKind;

public class FingerprintSchema {

    public static void main(String[] args) /*throws IOException*/ {

        TreeMap<String, FieldDescriptor> fields = new TreeMap<String, FieldDescriptor>();
        fields.put("fieldname", new FieldDescriptor("fieldname", FieldKind.STRING));
        Schema schema = new Schema("typename", fields);
        System.out.println(schema.getSchemaId());
    }
}

