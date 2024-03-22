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

public static class MemoryUnitExtensions
{
    public static string Abbrev(this MemoryUnit memoryUnit)
        => memoryUnit switch
        {
            MemoryUnit.Bytes => "B",
            MemoryUnit.KiloBytes => "KB",
            MemoryUnit.MegaBytes => "MB",
            MemoryUnit.GigaBytes => "GB",
            _ => throw new ArgumentOutOfRangeException(nameof(memoryUnit))
        };

    public static long Convert(MemoryUnit toUnit, MemoryUnit fromUnit, long value)
    {
        if (toUnit == fromUnit) return value;

        var bytes = fromUnit switch
        {
            MemoryUnit.Bytes => value,
            MemoryUnit.KiloBytes => value * 1_000,
            MemoryUnit.MegaBytes => value * 1_000_000,
            MemoryUnit.GigaBytes => value * 1_000_000_000,
            _ => throw new ArgumentOutOfRangeException(nameof(fromUnit))
        };

        return toUnit switch
        {
            MemoryUnit.Bytes => bytes,
            MemoryUnit.KiloBytes => (long)Math.Round((double)bytes / 1_000),
            MemoryUnit.MegaBytes => (long)Math.Round((double)bytes / 1_000_000),
            MemoryUnit.GigaBytes => (long)Math.Round((double)bytes / 1_000_000_000),
            _ => throw new ArgumentOutOfRangeException(nameof(toUnit))
        };
    }
}
