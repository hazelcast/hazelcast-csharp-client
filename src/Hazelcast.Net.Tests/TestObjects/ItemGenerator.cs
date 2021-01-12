// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Core;

namespace Hazelcast.Tests.TestObjects
{
    public class ItemGenerator
    {
        private const int MaxGroups = 20;
        private const int GroupNumber = 200;

        public static Item GenerateItem(long id)
        {
            var header = new Header(id, new Handle(true));

            var allowedGroups = new int[RandomProvider.Random.Next(MaxGroups + 1)];
            for (var i = 0; i < allowedGroups.Length; i++)
                allowedGroups[i] = RandomProvider.Random.Next(GroupNumber);

            var deniedUsers = new int[RandomProvider.Random.Next(MaxGroups + 1)];
            for (var i = 0; i < deniedUsers.Length; i++)
                deniedUsers[i] = RandomProvider.Random.Next(GroupNumber);
            return new Item(header, allowedGroups, deniedUsers);
        }
    }
}
