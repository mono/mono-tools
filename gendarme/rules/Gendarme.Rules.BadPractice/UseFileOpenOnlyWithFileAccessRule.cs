//
// Gendarme.Rules.BadPractice.UseFileOpenOnlyWithFileAccessRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;


namespace Gendarme.Rules.BadPractice {
	
	/// <summary>
	/// This rule checks that when file open method is called with FileMode parameter
	/// it is also called with FileAccess (or FileSystemRights) parameter. It is needed 
	/// because default behaviour of file open methods when they are called only with 
	/// FileMode is to require read-write access while it is commonly expected that they 
	/// will require only read access.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void OpenFile ()
	/// {
	///	FileStream f = File.Open ("Filename.ext", FileMode.Open);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void OpenFile ()
	/// {
	///	FileStream f = File.Open ("Filename.ext", FileMode.Open, FileAccess.Read);
	/// }
	/// </code>
	/// </example>
	[Problem ("File open methods should be called with FileAccess parameter if they are called with FileMode parameter")]
	[Solution ("Add FileAccess parameter to your call")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseFileOpenOnlyWithFileAccessRule : Rule, IMethodRule {

		HashSet<string> methods = new HashSet<string> {
			"System.IO.File::Open",
			"System.IO.FileInfo::Open",
			"System.IO.FileStream::.ctor",
			"System.IO.IsolatedStorage.IsolatedStorageFileStream::.ctor",
			
		};

		const string fileMode = "System.IO.FileMode";
		const string fileAccess = "System.IO.FileAccess";
		const string fileSystemRights = "System.Security.AccessControl.FileSystemRights";

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;
				
				MethodReference m = (instruction.Operand as MethodReference);
				// note: namespace check should be extended if anything outside of System.IO.* 
				// was added to the 'methods'
				if (m == null || !m.HasParameters || !m.DeclaringType.Namespace.StartsWith("System.IO"))
					continue;

				if (methods.Contains (m.DeclaringType.FullName + "::" + m.Name)) {
					bool foundFileMode = false;
					bool foundFileAccess = false;
					foreach (ParameterDefinition parameter in m.Parameters) {
						if (!foundFileMode && parameter.ParameterType.FullName == fileMode)
							foundFileMode = true;
						if (!foundFileAccess && (parameter.ParameterType.FullName == fileAccess ||
							parameter.ParameterType.FullName == fileSystemRights))
							foundFileAccess = true;
					}
					if (foundFileMode && !foundFileAccess)
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal,
							String.Format("{0}::{1} being called with FileMode parameter but without FileAccess.",
								m.DeclaringType.FullName, m.Name));
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
