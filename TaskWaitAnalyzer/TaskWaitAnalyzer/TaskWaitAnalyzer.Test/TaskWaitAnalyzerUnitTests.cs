using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using TaskWaitAnalyzer;

namespace TaskWaitAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyCode_NoDiagnostic()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void WhenAny_SingleDiagnostic()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test() => await Task.WhenAny(Task.Delay(1));
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TaskWaitAnalyzer",
                Message = String.Format("Task awaiting '{0}' can be simplified", "Task.WhenAny(Task.Delay(1))"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 47)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void WhenAll_SingleDiagnostic()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test() => await Task.WhenAll(Task.Delay(1));
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TaskWaitAnalyzer",
                Message = String.Format("Task awaiting '{0}' can be simplified", "Task.WhenAll(Task.Delay(1))"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 47)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void WhenAny_SingleFix()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test() => await Task.WhenAny(Task.Delay(1));
        }
    }";

            var fixtest = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test() => await Task.Delay(1);
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TaskWaitAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TaskWaitAnalyzerAnalyzer();
        }
    }
}
