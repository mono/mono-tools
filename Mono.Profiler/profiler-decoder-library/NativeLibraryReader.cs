// Author:
// Massimiliano Mantione (massi@ximian.com)
//
// (C) 2008 Novell, Inc  http://www.novell.com
//

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
//

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace  Mono.Profiler {
	public class NativeLibraryReader {
		static string[] RunExternalProcess (string executableName, string arguments) {
			Process p = new Process ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;

			p.StartInfo.FileName = executableName;
			p.StartInfo.Arguments = arguments;

			p.Start();
			// Do not wait for the child process to exit before
			// reading to the end of its redirected stream.
			// p.WaitForExit ();
			// Read the output stream first and then wait.
			string output = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			string[] outputLines = output.Split ('\n');
			
			return outputLines;
		}
		
		static Regex nmSymbolLine = new Regex ("([a-fA-F0-9]+)\\s([a-zA-Z])\\s(.*)$");
		static Regex nmFileNotFound = new Regex ("(.*)nm:(.*)No such file$");
		static Regex nmNoSymbols = new Regex ("(.*)nm:(.*)No symbols$");
		static Regex nmUnknownProblem = new Regex ("(.*)nm:(.*)$");
		
		public static void FillFunctions<MR,UFR> (MR region) where UFR : IUnmanagedFunctionFromRegion<UFR> where MR : IExecutableMemoryRegion <UFR> {
			FillFunctionsUsingNm<MR,UFR> (region);
		}
		
		static void FillFunctionsUsingNm<MR,UFR> (MR region) where UFR : IUnmanagedFunctionFromRegion<UFR> where MR : IExecutableMemoryRegion <UFR> {
			try {
				string[] outputLines = RunExternalProcess ("/usr/bin/nm", "-n " + region.Name);
				if (outputLines.Length == 1) {
					Match m = nmFileNotFound.Match (outputLines [0]);
					if (m.Success) {
						return;
					}
					m = nmNoSymbols.Match (outputLines [0]);
					if (m.Success || (outputLines [0].Length == 0)) {
						outputLines = RunExternalProcess ("/usr/bin/nm", "-n -D " + region.Name);
						if (outputLines.Length == 1) {
							m = nmUnknownProblem.Match (outputLines [0]);
							if (m.Success) {
								return;
							}
						}
					} else {
						m = nmUnknownProblem.Match (outputLines [0]);
						if (m.Success) {
							return;
						}
					}
				} else if (outputLines.Length == 0) {
					outputLines = RunExternalProcess ("/usr/bin/nm", "-n -D " + region.Name);
				}
				
				IUnmanagedFunctionFromRegion<UFR> lastFunction = null;
				foreach (string outputLine in outputLines) {
					Match m = nmSymbolLine.Match (outputLine);
					if (m.Success) {
						ulong symbolOffset = (ulong) Int64.Parse (m.Groups [1].Value, NumberStyles.HexNumber);
						String symbolType = m.Groups [2].Value;
						String symbolName = m.Groups [3].Value;
						
						if (symbolOffset >= region.StartAddress + region.FileOffset) {
							symbolOffset -= region.StartAddress;
							
							if (lastFunction != null) {
								lastFunction.EndOffset = (uint) symbolOffset -1;
								lastFunction = null;
							}
							
							if ((symbolType == "T") || (symbolType == "t")) {
								lastFunction = region.NewFunction (symbolName, (uint) symbolOffset);
							}
						}
					}
				}
				if (lastFunction != null) {
					lastFunction.EndOffset = (uint) (region.EndAddress - region.StartAddress);
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("Exception: {0}", e.Message);
			}
		}
	}
}
