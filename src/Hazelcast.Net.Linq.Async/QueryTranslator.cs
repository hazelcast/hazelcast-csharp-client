// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;

namespace Hazelcast.Linq
{
    internal class QueryTranslator
    {
        private readonly string _mapName;

        public QueryTranslator(string mapName)
        {
            _mapName = mapName;
        }

        public (string, IReadOnlyCollection<object>) Translate(Expression root)
        {
            var evaluated = ExpressionEvaluator.EvaluatePartially(root);
            var boundExp = (ProjectionExpression) new QueryBinder().Bind(evaluated) as Expression;
            boundExp = UnusedColumnProcessor.Clean(boundExp);
            boundExp = RedundantSubqueryProcessor.Clean(boundExp!);
            return QueryFormatter.Format(boundExp);
        }
    }
}
