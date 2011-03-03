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
using System.Globalization;

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

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference System.IO.FileMode
			// then no code inside the module will be using it
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.IO", "FileMode");
					}));
			};
		}

		// System.IO.File::Open
		// System.IO.FileInfo::Open
		// System.IO.FileStream::.ctor
		// System.IO.IsolatedStorage.IsolatedStorageFileStream::.ctor
		static bool IsCandidate (MemberReference method)
		{
			TypeReference type = method.DeclaringType;
			string tname = type.Name;
			string mname = method.Name;
			switch (type.Namespace) {
			case "System.IO":
				switch (tname) {
				case "File":
				case "FileInfo":
					return (mname == "Open");
				case "FileStream":
					return (mname == ".ctor");
				default:
					return false;
				}
			case "System.IO.IsolatedStorage":
				return ((tname == "IsolatedStorageFileStream") && (mname == ".ctor"));
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				MethodReference m = instruction.GetMethod ();
				if (m == null || !m.HasParameters || !IsCandidate (m))
					continue;

				bool foundFileMode = false;
				bool foundFileAccess = false;
				foreach (ParameterDefinition parameter in m.Parameters) {
					TypeReference ptype = parameter.ParameterType;
					if (!foundFileMode && ptype.IsNamed ("System.IO", "FileMode"))
						foundFileMode = true;
					if (!foundFileAccess && (ptype.IsNamed ("System.IO", "FileAccess") || ptype.IsNamed ("System.Security.AccessControl", "FileSystemRights")))
						foundFileAccess = true;
				}
				if (foundFileMode && !foundFileAccess) {
					string msg = String.Format (CultureInfo.InvariantCulture, 
						"{0}::{1} being called with FileMode parameter but without FileAccess.",
						m.DeclaringType.GetFullName (), m.Name);
					Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, msg);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
