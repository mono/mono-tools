// 
// Gendarme.Rules.BadPractice.DoNotUseGetInterfaceToCheckAssignabilityRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	// based on an IRC discussion from jb and robertj leading to
	// http://lists.ximian.com/archives/public/mono-patches/2008-September/128122.html

	/// <summary>
	/// This rule checks for calls to <c>Type.GetInterface</c> that look like they query if
	/// a type is supported, i.e. the result is only used to compare against <c>null</c>.
	/// The problem is that only assembly qualified names uniquely identify a type so if
	/// you just use the interface name or even just the name and namespace you may
	/// get unexpected results.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// if (type.GetInterface ("IConvertible") != null)  {
	///	// then the type can be assigned to IConvertible
	///	// but what if there is another IConvertible in there ?!?
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// if (typeof (IConvertible).IsAssignableFrom (type))  {
	///	// then the type can be assigned to IConvertible
	///	// without a doubt!
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This method calls Type.GetInterface(string) to query if an interface is supported by the type, but the result may be incorrect if the type is not qualified with a namespace (and not even that is guaranteed to work).")]
	[Solution ("Instead use Type.IsAssignableFrom(Type) method to query for types known at compile time.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotUseGetInterfaceToCheckAssignabilityRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if there's no IL code
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if ((ins.OpCode.Code != Code.Call) && (ins.OpCode.Code != Code.Callvirt))
					continue;

				MethodReference call = (ins.Operand as MethodReference);
				if (call.Name != "GetInterface")
					continue;
				if (!call.DeclaringType.Inherits ("System", "Type")) // not a sealed type
					continue;

				// check for a null compare
				if (ins.Next.OpCode.Code != Code.Ldnull)
					continue;

				// there's a problem, but our confidence depends on whether a
				// *constant* string was used for the first parameter or not
				Instruction p = ins.TraceBack (method, -1);
				Confidence c = (p.OpCode.Code == Code.Ldstr) ? Confidence.Normal : Confidence.Low;

				Runner.Report (method, ins, Severity.Medium, c);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
