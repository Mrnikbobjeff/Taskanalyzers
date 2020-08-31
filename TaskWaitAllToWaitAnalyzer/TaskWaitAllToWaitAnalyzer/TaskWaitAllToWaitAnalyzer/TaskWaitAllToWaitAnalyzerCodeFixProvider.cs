using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace TaskWaitAllToWaitAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TaskWaitAllToWaitAnalyzerCodeFixProvider)), Shared]
    public class TaskWaitAllToWaitAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Simplify invocation";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TaskWaitAllToWaitAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach(var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
           
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
             Task.WaitAll(Task.Delay(1));
            var newInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                   SyntaxKind.SimpleMemberAccessExpression,
                  invocation.ArgumentList.Arguments.Single().Expression,
                   SyntaxFactory.IdentifierName(
                       "Wait"))
                );
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(invocation, newInvocation);
            var originalSolution = document.Project.Solution;
            return originalSolution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }
    }
}
