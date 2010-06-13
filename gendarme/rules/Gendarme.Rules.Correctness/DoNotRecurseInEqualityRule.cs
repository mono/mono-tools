//
// DoNotRecurseInEqualityRule: flag recursive operator== and !=.
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// An operator== or operator!= method is calling itself recursively. This is
	/// usually caused by neglecting to cast an argument to System.Object before 
	/// comparing it to null.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	///	public static bool operator== (Customer lhs, Customer rhs)
	///	{
	///		if (object.ReferenceEquals (lhs, rhs)) {
	///			return true;
	///		}
	///		if (lhs == null || rhs == null) {
	///			return false;
	///		}
	///		return lhs.name == rhs.name &amp;&amp; lhs.address == rhs.address;
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	///	public static bool operator== (Customer lhs, Customer rhs)
	///	{
	///		if (object.ReferenceEquals (lhs, rhs)) {
	///			return true;
	///		}
	///		if ((object) lhs == null || (object) rhs == null) {
	///			return false;
	///		}
	///		return lhs.name == rhs.name &amp;&amp; lhs.address == rhs.address;
	///	}
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("An operator== or operator!= method is calling itself recursively.")]
	[Solution ("Fix null argument checks so that they first cast the argument to System.Object.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DoNotRecurseInEqualityRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
				
			// ignore everything but operator== and operator!=
			string name = method.Name;
			if (!method.IsSpecialName || (name != "op_Equality" && name != "op_Inequality"))
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no Call[virt] in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;
				
			Log.WriteLine (this);
			Log.WriteLine (this, "---------------------------------------");
			Log.WriteLine (this, method);

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference callee = (ins.Operand as MethodReference).Resolve();	// need the resolve for generics
					if (callee != null) {
						if (callee.MetadataToken == method.MetadataToken) {
							// MethodReference.ToString is very costly but, in this case, won't be called often
							if (callee.ToString () == method.ToString ()) {
								Log.WriteLine (this, "recursive call at {0:X4}", ins.Offset);
								Runner.Report (method, ins, Severity.Critical, Confidence.Normal);
							}
						}
					}
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
