﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    public interface IFuture<T>
    {
        Exception Exception { get; }
        T Result { get; }
        bool IsComplete { get; }
        T GetResult(int miliseconds);
        Task<T> ToTask();
        bool Wait(int miliseconds);
        bool Wait();
    }
}