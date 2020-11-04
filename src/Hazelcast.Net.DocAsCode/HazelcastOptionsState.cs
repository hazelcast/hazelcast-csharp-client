// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Composition;
using System.Threading;
using Microsoft.DocAsCode.Plugins;

namespace Hazelcast.Net.DocAsCode
{
    [Export]
    [Shared] // singleton
    public class HazelcastOptionsState
    {
        public string ConceptualKey { get; } = "conceptual";

        public string BuildOptionsKey { get; } = "build-options";

        public List<FileModel> OptionFiles { get; } = new List<FileModel>();

        // for it to work, we need to gather options before we build them, and yet both
        // steps have to take place during pre-build. in addition, conceptual and reference
        // run in parallel, hence the 'gathered' flag here

        public int GatherBuilderOrder { get; } = 0; // important, must run before splitters

        public int BuildBuilderOrder { get; } = 2;

        public ManualResetEventSlim Gathered { get; } = new ManualResetEventSlim();
    }
}