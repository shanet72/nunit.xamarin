// ***********************************************************************
// Copyright (c) 2017 NUnit Project
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnit.Runner.Helpers
{
    /// <summary>
    /// Contains all assemblies for a test run, and controls execution of tests and collection of results
    /// </summary>
    internal class TestPackage
    {
        private readonly List<(Assembly, Dictionary<string,object>)> _testAssemblies = new List<(Assembly, Dictionary<string,object>)>();

        public void AddAssembly(Assembly testAssembly, Dictionary<string,object> options = null)
        {
            _testAssemblies.Add( (testAssembly, options) );
        }

        public async Task<TestRunResult> ExecuteTests(Func<string, bool> shouldExecuteTestCallback)
        {
            var resultPackage = new TestRunResult();

            var testFilter = shouldExecuteTestCallback == null
                ? TestFilter.Empty
                : new CallbackTestFilter(shouldExecuteTestCallback);

            foreach (var (assembly,options) in _testAssemblies)
            {
                NUnitTestAssemblyRunner runner = await LoadTestAssemblyAsync(assembly, options).ConfigureAwait(false);
                ITestResult result = await Task.Run(() => runner.Run(new DebugWindowTestListener(runner.CountTestCases(testFilter)), testFilter)).ConfigureAwait(false);
                resultPackage.AddResult(result);
            }
            resultPackage.CompleteTestRun();
            return resultPackage;
        }

        private static async Task<NUnitTestAssemblyRunner> LoadTestAssemblyAsync(Assembly assembly, Dictionary<string, object> options)
        {
            var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            await Task.Run(() => runner.Load(assembly, options ?? new Dictionary<string, object>()));
            return runner;
        }
    }

    public class CallbackTestFilter : TestFilter
    {
        private readonly Func<string, bool> _shouldExecuteTestCallback;

        public CallbackTestFilter(Func<string, bool> shouldExecuteTestCallback)
        {
            _shouldExecuteTestCallback = shouldExecuteTestCallback ?? throw new ArgumentNullException(nameof(shouldExecuteTestCallback));
        }

        public override bool Match(ITest test)
        {
            return _shouldExecuteTestCallback(test.FullName);
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            return parentNode.AddElement("filter");
        }
    }

    public class DebugWindowTestListener : ITestListener
    {
        private readonly int _totalTestCount;
        private int _totalPassed;
        private int _totalFailed;

        public DebugWindowTestListener(int totalTestCount)
        {
            _totalTestCount = totalTestCount;
        }

        private int PercentComplete => (int)Math.Round((_totalPassed + _totalFailed) / (double)_totalTestCount * 100.0);

        public void TestStarted(ITest test)
        {
            if (test.HasChildren)
                return;

            if (Debugger.IsLogging())
                Trace.WriteLine($"{PercentComplete:D2}% Test '{test.FullName}' started.");
        }

        public void TestFinished(ITestResult result)
        {
            if (result.HasChildren)
                return;

            switch (result.ResultState.Status)
            {
                case TestStatus.Passed:
                case TestStatus.Inconclusive:
                case TestStatus.Skipped:
                case TestStatus.Warning:
                    _totalPassed++;
                    break;

                case TestStatus.Failed:
                    _totalFailed++;
                    break;
            }

            var percentPassed = (int)Math.Round(_totalPassed / (double) _totalTestCount * 100.0);

            if (Debugger.IsLogging())
                Trace.WriteLine($"{PercentComplete:D2}% Test '{result.FullName}' finished.  Results:  {result.ResultState}   ( {_totalPassed} / {_totalTestCount} passed = {percentPassed}% )");
        }

        public void TestOutput(TestOutput output)
        {
        }

        public void SendMessage(TestMessage message)
        {
        }
    }
}
