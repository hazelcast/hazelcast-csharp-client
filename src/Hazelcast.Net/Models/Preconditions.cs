// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System;

namespace Hazelcast.Models;

internal static class Preconditions
{
    public static int ValidateNewBackupCount(int newBackupCount, int currentAsyncBackupCount)
    {
        if (newBackupCount < 0)
        {
            throw new ArgumentException("backup-count can't be smaller than 0");
        }

        if (currentAsyncBackupCount < 0)
        {
            throw new ArgumentException("async-backup-count can't be smaller than 0");
        }

        if (newBackupCount > Constants.PartitionMaxBackupCount)
        {
            throw new ArgumentException("backup-count can't be larger than than " + Constants.PartitionMaxBackupCount);
        }

        if (newBackupCount + currentAsyncBackupCount > Constants.PartitionMaxBackupCount)
        {
            throw new ArgumentException("the sum of backup-count and async-backup-count can't be larger than than "
                                        + Constants.PartitionMaxBackupCount);
        }

        return newBackupCount;
    }

    /**
     * Tests if the newAsyncBackupCount count is valid.
     *
     * @param currentBackupCount  the current number of backups
     * @param newAsyncBackupCount the new number of async backups
     * @return the newAsyncBackupCount
     * @throws java.lang.IllegalArgumentException if asyncBackupCount is smaller than 0, or larger than the maximum
     *                                            number of backups.
     */
    public static int ValidateNewAsyncBackupCount(int currentBackupCount, int newAsyncBackupCount)
    {
        if (currentBackupCount < 0)
        {
            throw new ArgumentException("backup-count can't be smaller than 0");
        }

        if (newAsyncBackupCount < 0)
        {
            throw new ArgumentException("async-backup-count can't be smaller than 0");
        }

        if (newAsyncBackupCount > Constants.PartitionMaxBackupCount)
        {
            throw new ArgumentException("async-backup-count can't be larger than than " + Constants.PartitionMaxBackupCount);
        }

        if (currentBackupCount + newAsyncBackupCount > Constants.PartitionMaxBackupCount)
        {
            throw new ArgumentException("the sum of backup-count and async-backup-count can't be larger than than "
                                        + Constants.PartitionMaxBackupCount);
        }

        return newAsyncBackupCount;
    }
}
