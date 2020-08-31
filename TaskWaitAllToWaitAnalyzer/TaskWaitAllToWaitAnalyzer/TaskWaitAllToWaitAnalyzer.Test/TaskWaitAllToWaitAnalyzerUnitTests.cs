using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using TaskWaitAllToWaitAnalyzer;
using System.Threading.Tasks;

namespace TaskWaitAllToWaitAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
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
            public async Task Test() => await Task.WaitAll(Task.Delay(1));
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "TaskWaitAllToWaitAnalyzer",
                Message = String.Format("Task.WaitAll invocation '{0}' can be simplified", "Task.WaitAll(Task.Delay(1))"),
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
            public async Task Test() => Task.WaitAll(Task.Delay(1));
        }
    }";

            var fixtest = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test() => Task.Delay(1).Wait();
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TaskWaitAllToWaitAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TaskWaitAllToWaitAnalyzerAnalyzer();
        }
    }
}
