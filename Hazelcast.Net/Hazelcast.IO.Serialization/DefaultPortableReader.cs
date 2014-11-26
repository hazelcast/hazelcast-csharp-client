using System;
using System.Collections.Generic;
using System.IO;

namespace Hazelcast.IO.Serialization
{
	internal class DefaultPortableReader : IPortableReader
	{
		protected internal readonly IClassDefinition cd;
		private readonly PortableSerializer serializer;
		private readonly IBufferObjectDataInput input;
		private readonly int finalPosition;
		private readonly int offset;
		private bool raw;

		public DefaultPortableReader(PortableSerializer serializer, IBufferObjectDataInput input, IClassDefinition cd)
		{
			this.input = input;
			this.serializer = serializer;
			this.cd = cd;
			try
			{
				// final position after portable is read
				finalPosition = input.ReadInt();
			}
			catch (IOException e)
			{
				throw new HazelcastSerializationException(e);
			}
			this.offset = input.Position();
		}

		public virtual int GetVersion()
		{
			return cd.GetVersion();
		}

		public virtual bool HasField(string fieldName)
		{
			return cd.HasField(fieldName);
		}

		public virtual ICollection<string> GetFieldNames()
		{
			return cd.GetFieldNames();
		}

		public virtual FieldType GetFieldType(string fieldName)
		{
			return cd.GetFieldType(fieldName);
		}

		public virtual int GetFieldClassId(string fieldName)
		{
			return cd.GetFieldClassId(fieldName);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual int ReadInt(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Int);
			return input.ReadInt(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual long ReadLong(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Long);
			return input.ReadLong(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual string ReadUTF(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.Utf);
				input.Position(pos);
				return input.ReadUTF();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual bool ReadBoolean(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Boolean);
			return input.ReadBoolean(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual byte ReadByte(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Byte);
			return input.ReadByte(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual char ReadChar(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Char);
			return input.ReadChar(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual double ReadDouble(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Double);
			return input.ReadDouble(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual float ReadFloat(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Float);
			return input.ReadFloat(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual short ReadShort(string fieldName)
		{
			int pos = ReadPosition(fieldName, FieldType.Short);
			return input.ReadShort(pos);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual byte[] ReadByteArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.ByteArray);
				input.Position(pos);
				return input.ReadByteArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual char[] ReadCharArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.CharArray);
				input.Position(pos);
				return input.ReadCharArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual int[] ReadIntArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.IntArray);
				input.Position(pos);
				return input.ReadIntArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual long[] ReadLongArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.LongArray);
				input.Position(pos);
				return input.ReadLongArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual double[] ReadDoubleArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.DoubleArray);
				input.Position(pos);
				return input.ReadDoubleArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual float[] ReadFloatArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.FloatArray);
				input.Position(pos);
				return input.ReadFloatArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual short[] ReadShortArray(string fieldName)
		{
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fieldName, FieldType.ShortArray);
				input.Position(pos);
				return input.ReadShortArray();
			}
			finally
			{
				input.Position(currentPos);
			}
		}

        /// <exception cref="System.IO.IOException"></exception>
        public virtual P ReadPortable<P>(string fieldName) where P : IPortable
		{
			IFieldDefinition fd = cd.GetField(fieldName);
			if (fd == null)
			{
				throw ThrowUnknownFieldException(fieldName);
			}
			if (fd.GetFieldType() != FieldType.Portable)
			{
				throw new HazelcastSerializationException("Not a Portable field: " + fieldName);
			}
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fd);
				input.Position(pos);
				bool isNull = input.ReadBoolean();
				if (!isNull)
				{
					return (P) serializer.ReadAndInitialize(input);
				}
                return default(P);
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		private HazelcastSerializationException ThrowUnknownFieldException(string fieldName)
		{
			return new HazelcastSerializationException("Unknown field name: '" + fieldName + 
				"' for ClassDefinition {id: " + cd.GetClassId() + ", version: " + cd.GetVersion() + "}");
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual IPortable[] ReadPortableArray(string fieldName)
		{
			IFieldDefinition fd = cd.GetField(fieldName);
			if (fd == null)
			{
				throw ThrowUnknownFieldException(fieldName);
			}
			if (fd.GetFieldType() != FieldType.PortableArray)
			{
				throw new HazelcastSerializationException("Not a Portable array field: " + fieldName);
			}
			int currentPos = input.Position();
			try
			{
				int pos = ReadPosition(fd);
				input.Position(pos);
				int len = input.ReadInt();
				IPortable[] portables = new IPortable[len];
				if (len > 0)
				{
					int offset = input.Position();
					for (int i = 0; i < len; i++)
					{
						int start = input.ReadInt(offset + i * 4);
						input.Position(start);
						portables[i] = serializer.ReadAndInitialize(input);
					}
				}
				return portables;
			}
			finally
			{
				input.Position(currentPos);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		private int ReadPosition(string fieldName, FieldType type)
		{
			if (raw)
			{
				throw new HazelcastSerializationException("Cannot read Portable fields after getRawDataInput() is called!");
			}
			IFieldDefinition fd = cd.GetField(fieldName);
			if (fd == null)
			{
				return ReadNestedPosition(fieldName, type);
			}
			if (fd.GetFieldType() != type)
			{
				throw new HazelcastSerializationException("Not a '" + type + "' field: " + fieldName);
			}
			return ReadPosition(fd);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private int ReadNestedPosition(string fieldName, FieldType type)
		{
			string[] fieldNames = fieldName.Split('.');
			if (fieldNames.Length > 1)
			{
				IFieldDefinition fd = null;
				Hazelcast.IO.Serialization.DefaultPortableReader reader = this;
				for (int i = 0; i < fieldNames.Length; i++)
				{
					fd = reader.cd.GetField(fieldNames[i]);
					if (fd == null)
					{
						break;
					}
					if (i == fieldNames.Length - 1)
					{
						break;
					}
					int pos = reader.ReadPosition(fd);
					input.Position(pos);
					bool isNull = input.ReadBoolean();
					if (isNull)
					{
						throw new ArgumentNullException("Parent field is null: " + fieldNames[i]);
					}
					reader = serializer.CreateReader(input);
				}
				if (fd == null)
				{
					throw ThrowUnknownFieldException(fieldName);
				}
				if (fd.GetFieldType() != type)
				{
					throw new HazelcastSerializationException("Not a '" + type + "' field: " + fieldName);
				}
				return reader.ReadPosition(fd);
			}
			throw ThrowUnknownFieldException(fieldName);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private int ReadPosition(IFieldDefinition fd)
		{
			return input.ReadInt(offset + fd.GetIndex() * 4);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual IObjectDataInput GetRawDataInput()
		{
			if (!raw)
			{
				int pos = input.ReadInt(offset + cd.GetFieldCount() * 4);
				input.Position(pos);
			}
			raw = true;
			return input;
		}

		/// <exception cref="System.IO.IOException"></exception>
		internal virtual void End()
		{
			input.Position(finalPosition);
		}
	}
}
