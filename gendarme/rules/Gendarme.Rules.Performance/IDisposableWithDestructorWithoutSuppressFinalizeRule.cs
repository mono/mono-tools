//
// Gendarme.Rules.Performance.IDisposableWithDestructorWithoutSuppressFinalizeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {
	
	[Problem ("The type has a destructor and implements IDisposable. However it doesn't call System.GC.SuppressFinalize inside it's Dispose method.")]
	[Solution ("Add a call to GC.SuppressFinalize inside your Dispose method.")]
	public class IDisposableWithDestructorWithoutSuppressFinalizeRule : Rule, ITypeRule {

		private static bool MethodMatchNameVoidEmpty (MethodDefinition md, string methodName)
		{
			if (md.Name != methodName)
				return false;
			if (md.Parameters.Count > 0)
				return false;
			return (md.ReturnType.ReturnType.ToString () == "System.Void");
		}

		private RuleResult Recurse (MethodDefinition method, int level)
		{
			// some methods have no body (e.g. p/invokes, icalls)
			if (!method.HasBody)
				return RuleResult.Failure;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					// are we calling GC.SuppressFinalize ?
					if (ins.Operand.ToString () == "System.Void System.GC::SuppressFinalize(System.Object)")
						return RuleResult.Success;
					else if (level < 3) {
						MethodDefinition callee = (ins.Operand as MethodDefinition);
						if (callee != null) {
							if (Recurse (callee, level + 1) == RuleResult.Success)
								return RuleResult.Success;
						}
					}
					break;
				}
			}
			return RuleResult.Failure;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// #1 - does the type implements System.IDisposable ?
			if (!type.Implements ("System.IDisposable"))
				return RuleResult.DoesNotApply;

			// #2 - look for the Dispose method
			MethodDefinition dispose = null;
			foreach (MethodDefinition md in type.Methods) {
				if (MethodMatchNameVoidEmpty (md, "Dispose") ||
					MethodMatchNameVoidEmpty (md, "System.IDisposable.Dispose")) {

					dispose = md;
					break;
				}
			}
			if (dispose == null)
				return RuleResult.Success;

			// #3 - look for a destructor
			if (type.GetMethod (MethodSignatures.Finalize) == null)
				return RuleResult.Success;

			// #4 - look if GC.SuppressFinalize is being called in the
			// Dispose method - or one of the method it calls
			return Recurse (dispose, 0);
		}
	}
}
