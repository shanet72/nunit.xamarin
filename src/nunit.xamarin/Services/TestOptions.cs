﻿// ***********************************************************************
// Copyright (c) 2016 NUnit Project
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
using System.IO;

namespace NUnit.Runner.Services
{
    /// <summary>
    /// Options for the device test suite.
    /// </summary>
    public class TestOptions
    {
        const string OutputXmlReportName = "TestResults.xml";

        /// <summary>
        /// Constructor
        /// </summary>
        public TestOptions()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ResultFilePath = Path.Combine(path, OutputXmlReportName);
        }

        /// <summary>
        /// If True, the tests will run automatically when the app starts
        /// otherwise you must run them manually.
        /// </summary>
        public bool AutoRun { get; set; }

        /// <summary>
        /// If True, the application will terminate automatically after running the tests.
        /// </summary>
        public bool TerminateAfterExecution { get; set; }

        /// <summary>
        /// Information about the tcp listener host and port.
        /// For now, send result as XML to the listening server.
        /// </summary>
        public TcpWriterInfo TcpWriterParameters { get; set; }

        /// <summary>
        /// Creates a NUnit Xml result file on the host file system using PCLStorage library.
        /// </summary>
        public bool CreateXmlResultFile { get; set; }

        /// <summary>
        /// File path for the xml result file
        /// Default is [LocalStorage]/TestResults.xml
        /// </summary>
        public string ResultFilePath { get; set; }

        /// <summary>
        /// An optional callback that will be called before running each test.  Return true if you want the test to run, otherwise false.  The default null value will not apply a filter.
        /// </summary>
        public Func<string, bool> ShouldExecuteTestCallback { get; set; }
    }
}
