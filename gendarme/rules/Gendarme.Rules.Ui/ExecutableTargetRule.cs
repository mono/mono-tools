//
// Gendarme.Rules.Ui.ExecutableTargetRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.UI {

	[Solution ("Recompile the assembly using '/target:winexe' (gmcs syntax).")]
	abstract public class ExecutableTargetRule : Rule, IAssemblyRule {

		private bool CheckReferences (AssemblyDefinition assembly)
		{
			byte[] publicKeyToken = GetAssemblyPublicKeyToken ();

			foreach (AssemblyNameReference a in assembly.MainModule.AssemblyReferences) {
				// check name and public key token (but not version or culture)
				if (a.Name == AssemblyName) {
					byte[] token = a.PublicKeyToken;
					if (token != null) {
						if ((token[0] == publicKeyToken[0]) && (token[1] == publicKeyToken[1]) &&
						    (token[2] == publicKeyToken[2]) && (token[3] == publicKeyToken[3]) &&
						    (token[4] == publicKeyToken[4]) && (token[5] == publicKeyToken[5]) &&
						    (token[6] == publicKeyToken[6]) && (token[7] == publicKeyToken[7]))
							return true;
					}
				}
			}
			return false;
		}

		abstract protected string AssemblyName { get; }

		abstract protected byte[] GetAssemblyPublicKeyToken ();

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			// 1. Check entry point, if no entry point then it's not an executable
			if (assembly.EntryPoint == null)
				return RuleResult.DoesNotApply;

			// 2. Check if the assembly references SWF or GTK#
			if (!CheckReferences (assembly))
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! only Success or Failure from this point on ***

			// 3. On Windows a console window will appear if the subsystem isn't Windows
			//    i.e. the assembly wasn't compiled with /target:winexe
			if (assembly.MainModule.Kind == ModuleKind.Windows)
				return RuleResult.Success;

			Runner.Report (assembly, Severity.Medium, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
