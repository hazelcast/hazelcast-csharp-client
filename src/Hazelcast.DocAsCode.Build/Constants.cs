// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.DocAsCode.Build;

public static class Constants
{
    public const string ConceptualKey = "conceptual"; // the dictionary key to markdown content

    public const string BuildOptionsKey = "build-options"; // they key that marks the options page

    public static class BuildOrder
    {
        // for it to work, we need to gather options before we build them, and yet both
        // steps have to take place during pre-build. in addition, conceptual and reference
        // run in parallel, hence the 'gathered' semaphore in HazelcastOptionsState.

        public const int GatherOptions= 0; // important, must run before splitters
        public const int BuildOptions = 2;
    }
}
