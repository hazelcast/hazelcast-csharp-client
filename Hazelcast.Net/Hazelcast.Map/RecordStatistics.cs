/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Map
{
    //[System.Serializable]
    //internal class RecordStatistics : IDataSerializable
    //{
    //    protected internal volatile int hits = 0;

    //    protected internal volatile long lastStoredTime = 0;

    //    protected internal volatile long lastUpdateTime = 0;

    //    protected internal volatile long lastAccessTime = 0;

    //    protected internal volatile long creationTime = 0;

    //    protected internal volatile long expirationTime = 0;

    //    protected internal volatile long cost = 0;

    //    public RecordStatistics()
    //    {
    //        // todo is volatile needed? if yes then hits should be atomicnumber
    //        long now = Clock.CurrentTimeMillis();
    //        lastAccessTime = now;
    //        lastUpdateTime = now;
    //        creationTime = now;
    //    }

    //    public virtual int GetHits()
    //    {
    //        return hits;
    //    }

    //    public virtual void SetHits(int hits)
    //    {
    //        this.hits = hits;
    //    }

    //    public virtual long GetCreationTime()
    //    {
    //        return creationTime;
    //    }

    //    public virtual void SetCreationTime(long creationTime)
    //    {
    //        this.creationTime = creationTime;
    //    }

    //    public virtual long GetExpirationTime()
    //    {
    //        return expirationTime;
    //    }

    //    public virtual void SetExpirationTime(long expirationTime)
    //    {
    //        this.expirationTime = expirationTime;
    //    }

    //    public virtual long GetCost()
    //    {
    //        return cost;
    //    }

    //    public virtual void SetCost(long cost)
    //    {
    //        this.cost = cost;
    //    }

    //    public virtual void Access()
    //    {
    //        lastAccessTime = Clock.CurrentTimeMillis();
    //        hits++;
    //    }

    //    public virtual void Update()
    //    {
    //        lastUpdateTime = Clock.CurrentTimeMillis();
    //    }

    //    public virtual void Store()
    //    {
    //        lastStoredTime = Clock.CurrentTimeMillis();
    //    }

    //    public virtual long GetLastAccessTime()
    //    {
    //        return lastAccessTime;
    //    }

    //    public virtual long GetLastStoredTime()
    //    {
    //        return lastStoredTime;
    //    }

    //    public virtual void SetLastStoredTime(long lastStoredTime)
    //    {
    //        this.lastStoredTime = lastStoredTime;
    //    }

    //    public virtual long GetLastUpdateTime()
    //    {
    //        return lastUpdateTime;
    //    }

    //    public virtual void SetLastUpdateTime(long lastUpdateTime)
    //    {
    //        this.lastUpdateTime = lastUpdateTime;
    //    }

    //    public virtual long Size()
    //    {
    //        //size of the instance.
    //        return 6 * (long.Size / byte.Size) + (int.Size / byte.Size);
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public virtual void WriteData(IObjectDataOutput output)
    //    {
    //        output.WriteInt(hits);
    //        output.WriteLong(lastStoredTime);
    //        output.WriteLong(lastUpdateTime);
    //        output.WriteLong(lastAccessTime);
    //        output.WriteLong(cost);
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public virtual void ReadData(IObjectDataInput input)
    //    {
    //        hits = input.ReadInt();
    //        lastStoredTime = input.ReadLong();
    //        lastUpdateTime = input.ReadLong();
    //        lastAccessTime = input.ReadLong();
    //        cost = input.ReadLong();
    //    }
    //}
}