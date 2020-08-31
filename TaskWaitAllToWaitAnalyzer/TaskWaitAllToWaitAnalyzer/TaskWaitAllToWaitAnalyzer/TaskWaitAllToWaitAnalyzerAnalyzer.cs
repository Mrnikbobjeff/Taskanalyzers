using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TaskWaitAllToWaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TaskWaitAllToWaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TaskWaitAllToWaitAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Style";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeWaitAll, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeWaitAll(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation.ArgumentList.Arguments.Count != 1)
                return;//Single argument invocations only
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.ValueText.Equals("WaitAll") && memberAccess.Expression is IdentifierNameSyntax id && id.Identifier.ValueText.Equals("Task")))
                return; //WaitAll call
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), invocation);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
