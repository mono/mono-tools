//
// Gendarme.Rules.BadPractice.CheckNewExceptionWithoutThrowingRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks for exception objects which are created but not thrown, not returned,
	/// and not passed to another method as an argument.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// void MissingThrow (object arg)
	/// {
	///	if (arg == null) {
	///		new ArgumentNullException ("arg");
	///	}
	///	DoWork (arg);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	/// void Throw (object arg)
	/// {
	/// 	if (arg == null) {
	/// 		throw new ArgumentNullException ("arg");
	/// 	}
	/// 	DoWork (arg);
	/// }
	/// 
	/// Exception CreateException ()
	/// {
	///	return new Exception ();
	/// }
	/// </code>
	/// </example>

	[Problem ("This method creates an exception that is never throwed nor returned to the caller.")]
	[Solution ("Make sure the exception is required, throw it (if it is) or remove it (if not).")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class CheckNewExceptionWithoutThrowingRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Newobj instructions in this method
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Newobj))
				return RuleResult.DoesNotApply;

			StackEntryAnalysis sea = null;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Newobj)
					continue;

				MethodReference constructor = (MethodReference) ins.Operand;

				if (!constructor.DeclaringType.Inherits ("System", "Exception"))
					continue;

				// quick check to save resources
				if (ins.Next != null && ins.Next.OpCode.Code == Code.Throw)
					continue;

				if (sea == null)
					sea = new StackEntryAnalysis (method);

				StackEntryUsageResult [] usageResults = sea.GetStackEntryUsage (ins);

				bool exceptionUsed = false;
				foreach (var usage in usageResults) {
					switch (usage.Instruction.OpCode.Code) {
					case Code.Throw: //throw
					case Code.Ret: //return
					case Code.Stind_I: //out / ref
					case Code.Stind_I1:
					case Code.Stind_I2:
					case Code.Stind_I4:
					case Code.Stind_I8:
					case Code.Stind_R4:
					case Code.Stind_R8:
					case Code.Stind_Ref:
						//case Code.Stfld:
						exceptionUsed = true;
						break;
					case Code.Call: //call (to the exception or as an argument)
					case Code.Calli:
					case Code.Callvirt:
					case Code.Newobj:
					case Code.Initobj:
						IMethodSignature calledMethod = (IMethodSignature) usage.Instruction.Operand;
						int pcount = calledMethod.HasParameters ? calledMethod.Parameters.Count : 0;
						if (pcount <= usage.StackOffset)
							break; //not used as a parameter
						exceptionUsed = true;
						break;
					}
					if (exceptionUsed)
						break;
				}

				if (!exceptionUsed) {
					// Critical because code cannot work as intented
					Runner.Report (method, ins, Severity.Critical, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
