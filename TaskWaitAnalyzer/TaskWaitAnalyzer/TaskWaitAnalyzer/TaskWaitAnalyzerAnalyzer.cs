using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TaskWaitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TaskWaitAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TaskWaitAnalyzer";
        static readonly string[] knownCalls = new string[] { "WhenAll", "WhenAny" };
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Performance";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
           // await Task.WhenAny(Task.Delay(100));
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation.ArgumentList.Arguments.Count != 1)
                return;//Single argument invocations only
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess 
                && knownCalls.Contains(memberAccess.Name.Identifier.ValueText) && memberAccess.Expression is IdentifierNameSyntax id && id.Identifier.ValueText.Equals("Task")))
                return; //WhenX call
            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), invocation);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
