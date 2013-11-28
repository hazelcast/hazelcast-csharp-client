using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Partition
{
	
	[System.Serializable]
	public sealed class PartitionsResponse : IIdentifiedDataSerializable
	{
		private Address[] members;

		private int[] ownerIndexes;

		public PartitionsResponse()
		{
		}

		public PartitionsResponse(Address[] members, int[] ownerIndexes)
		{
			this.members = members;
			this.ownerIndexes = ownerIndexes;
		}

		public int GetFactoryId()
		{
			return PartitionDataSerializerHook.FId;
		}

		public int GetId()
		{
			return PartitionDataSerializerHook.Partitions;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WriteData(IObjectDataOutput output)
		{
			output.WriteInt(members.Length);
			foreach (Address member in members)
			{
				member.WriteData(output);
			}
			output.WriteInt(ownerIndexes.Length);
			foreach (int index in ownerIndexes)
			{
				output.WriteInt(index);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadData(IObjectDataInput input)
		{
			int len = input.ReadInt();
			members = new Address[len];
			for (int i = 0; i < len; i++)
			{
				Address a = new Address();
				a.ReadData(input);
				members[i] = a;
			}
			len = input.ReadInt();
			ownerIndexes = new int[len];
			for (int i_1 = 0; i_1 < len; i_1++)
			{
				ownerIndexes[i_1] = input.ReadInt();
			}
		}

		public Address[] GetMembers()
		{
			return members;
		}

		public int[] GetOwnerIndexes()
		{
			return ownerIndexes;
		}
	}
}
