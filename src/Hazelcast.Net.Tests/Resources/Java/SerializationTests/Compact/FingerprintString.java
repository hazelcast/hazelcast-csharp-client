import java.io.IOException;
//import com.hazelcast.internal.serialization.impl.compact.RabinFingerprint;

public class FingerprintString {

    public static void main(String[] args) /*throws IOException*/ {

        int bufSize = 4096; // 4k buffer.
        byte[] buffer = new byte[bufSize];
        int bytesAvailable;
        int bytesCount;

        bytesCount = 0;

        try {
            while ((bytesAvailable = System.in.read(buffer, 0, bufSize)) != -1) {
                //System.out.println(String.format("Received %d bytes from CSharp", bytesAvailable));
                bytesCount = bytesAvailable;
                // fixme we should accumulate bytes in a buffer!
                //myOutputStream.write(buffer, 0, bytesAvailable);
                //myOutputStream.flush();
            }
        } catch (IOException e) { }

        if (bytesCount == 0){
            return;
        }

        byte[] fingerprintBuffer = new byte[bytesCount];
        for (int i = 0; i < bytesCount; i++)
        {
            fingerprintBuffer[i] = buffer[i];
        }

        long fingerprint = RabinFingerprint.fingerprint64(fingerprintBuffer);
        System.out.println(fingerprint);
    }
}

