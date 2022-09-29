using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class Sandbox
    {
        [Test]
        public void Test()
        {
            // can we fast-scan a compact object?

            var thingSchema = SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.String)
                .WithField("value", FieldKind.Int32)
                .Build();

            var wrapperSchema = SchemaBuilder
                .For("thing-wrapper")
                .WithField("thing", FieldKind.Compact)
                .Build();

            Console.WriteLine($"Thing   Schema ID: {thingSchema.Id:X}");
            Console.WriteLine($"Wrapper Schema ID: {wrapperSchema.Id:X}");

            var rw = new ReaderWriter(thingSchema, wrapperSchema);

            var t = new Thing { Name = "thing", Value = 42 };
            var w = new ThingWrapper { Thing = t };

            var output = new ObjectDataOutput(1024, rw, Endianness.LittleEndian);
            rw.Write(output, w);

            var bytes = output.ToByteArray();

            Console.WriteLine(bytes.Dump());

            var schemas = new Dictionary<long, Schema>
            {

                { thingSchema.Id, thingSchema },
                { wrapperSchema.Id, wrapperSchema }
            };

            var input = new ObjectDataInput(bytes, rw, Endianness.LittleEndian);
            var result = Scan(input, schemas);
            Console.WriteLine($">>> {result}");

            // but, how would we use it?
            //await SerializationService.CanDeserialize(data)?
        }

        private long Scan(ObjectDataInput input, Dictionary<long, Schema> schemas)
        {
            var schemaId = input.ReadLong();
            Console.WriteLine($"SCHEMA ID: {schemaId}");
            if (!schemas.TryGetValue(schemaId, out var schema))
            {
                Console.WriteLine("MISSING");
                return schemaId;
            }

            Console.WriteLine("FOUND");

            if (!schema.HasReferenceFields)
            {
                Console.WriteLine("NOREF");
                return 0;
            }

            var compactFields = schema.Fields.Where(x => x.Kind == FieldKind.Compact || x.Kind == FieldKind.ArrayOfCompact);

            var start = -1;
            var dataLength = -1;

            foreach (var field in compactFields)
            {
                if (dataLength < 0)
                {
                    dataLength = input.ReadInt();
                    start = input.Position;
                    Console.WriteLine($"START: {start}");
                    Console.WriteLine($"LENGTH: {dataLength}");
                }

                Console.WriteLine($"FIELD: '{field.FieldName}' is {field.Kind} index {field.Index}");

                var offsetReader = CompactReader.GetOffsetReader(dataLength);
                var offset = offsetReader(input, start + dataLength, field.Index);
                Console.WriteLine($"OFFSET: {offset} -> POS: {start + offset}");
                input.MoveTo(start + offset);
                if (field.Kind == FieldKind.Compact)
                {
                    var scan = Scan(input, schemas);
                    if (scan != 0) return scan;
                }
                else
                {
                    // we now need to scan the array oh boy
                    var arrayStart = input.Position;
                    var arrayDataLength = input.ReadInt();
                    var arrayOffsetReader = CompactReader.GetOffsetReader(arrayDataLength);
                    var arrayCount = input.ReadInt();
                    for (var i = 0; i < arrayCount; i++)
                    {
                        var itemOffset = arrayOffsetReader(input, arrayStart + arrayDataLength, i);
                        if (itemOffset < 0) continue;
                        input.MoveTo(arrayStart + itemOffset);
                        var scan = Scan(input, schemas);
                        if (scan != 0) return scan;
                    }
                }
            }

            int OffsetReader(ObjectDataInput input, int start, int index)
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfByte);
                return input.ReadByte();
            }

            return 0;
        }

        internal class ReaderWriter : IReadWriteObjectsFromIObjectDataInputOutput
        {
            private readonly Schema _thingSchema;
            private readonly Schema _wrapperSchema;

            public ReaderWriter(Schema thingSchema, Schema wrapperSchema)
            {
                _thingSchema = thingSchema;
                _wrapperSchema = wrapperSchema;
            }

            public void Write(IObjectDataOutput output, object obj)
            {
                if (obj is ThingWrapper wrapper)
                {
                    output.WriteLong(_wrapperSchema.Id);
                    var serializer = new ThingWrapper.ThingWrapperSerializer();
                    var writer = new CompactWriter(this, (ObjectDataOutput) output, _wrapperSchema);
                    serializer.Write(writer, wrapper);
                    writer.Complete();
                }
                if (obj is Thing thing)
                {
                    output.WriteLong(_thingSchema.Id);
                    var serializer = new ThingCompactSerializer<Thing>();
                    var writer = new CompactWriter(this, (ObjectDataOutput)output, _thingSchema);
                    serializer.Write(writer, thing);
                    writer.Complete();
                }
            }

            public object Read(IObjectDataInput input, Type type)
                => throw new NotImplementedException();

            public T Read<T>(IObjectDataInput input)
                => throw new NotImplementedException();
        }
    }
}
