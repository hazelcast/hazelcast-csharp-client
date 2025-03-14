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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Hazelcast.Net.BuildAnalyzers
{
    /// <summary>
    /// Checks that all public APIs have either NotNull or MaybeNull parameter. 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicApiNullAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HZL0001";
        private static readonly LocalizableString Title = "Public API should have NotNull or MaybeNull attribute";
        private static readonly LocalizableString MessageFormat = "Public API '{0}' should have NotNull or MaybeNull attribute";
        private static readonly LocalizableString Description = "All public APIs should be annotated with NotNull or MaybeNull attributes.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;

            if (node is MethodDeclarationSyntax methodDeclaration)
            {
                AnalyzeMethod(context, methodDeclaration);
            }
            else if (node is PropertyDeclarationSyntax propertyDeclaration)
            {
                AnalyzeProperty(context, propertyDeclaration);
            }
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                var attributes = methodDeclaration.AttributeLists.SelectMany(a => a.Attributes);
                if (!HasNotNullOrMaybeNull(attributes))
                {
                    var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeProperty(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                var attributes = propertyDeclaration.AttributeLists.SelectMany(a => a.Attributes);
                if (!HasNotNullOrMaybeNull(attributes))
                {
                    var diagnostic = Diagnostic.Create(Rule, propertyDeclaration.Identifier.GetLocation(), propertyDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool HasNotNullOrMaybeNull(IEnumerable<AttributeSyntax> attributes)
        {
            foreach (var attribute in attributes)
            {
                var name = attribute.Name.ToString();
                if (name == "NotNull" || name == "MaybeNull")
                {
                    return true;
                }
            }
            return false;
        }
    }
}