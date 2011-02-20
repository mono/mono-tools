// 
// Gendarme.Rules.Gendarme.DoNotThrowExceptionRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Gendarme {
	/// <summary>
	/// This rule finds Gendarme rules that throw exceptions because runner's behavior
	/// in case of rule throwing an exception is undefined.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class ExampleRule : Rule, IMethodRule {
	///	public RuleResult CheckMethod (MethodDefinition method)
	///	{
	///		if (method == null)
	///			throw new ArgumentNullException ("method");
	///		// other rule logic
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class ExampleRule : Rule, IMethodRule {
	///	public RuleResult CheckMethod (MethodDefinition method)
	///	{
	///		// method is not null by contract
	///		return RuleResult.Success;
	///	}
	/// }
	/// </code>
	/// </example>
	[Problem ("Gendarme rules should not throw exceptions because runners behaviour in this case is undefined.")]
	[Solution ("Change the code so that it catches exceptions or does not throw them at all.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotThrowExceptionRule : GendarmeRule, IMethodRule {
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeType += (object sender, RunnerEventArgs e) =>
			{
				Active = e.CurrentType.Implements ("Gendarme.Framework", "IRule");
			};
		}


		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!OpCodeEngine.GetBitmask (method).Get (Code.Throw))
				return RuleResult.Success;

			if (method.IsSetter && method.IsPublic) {
				PropertyDefinition property = method.GetPropertyByAccessor ();
				if (property.HasAttribute ("System.ComponentModel", "DescriptionAttribute"))
					return RuleResult.Success;
			}

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code != Code.Throw)
					continue;

				if (!instruction.Previous.Is (Code.Newobj))
					continue;

				MethodReference m = (instruction.Previous.Operand as MethodReference);
				if (m == null)
					continue;

				TypeReference type = m.DeclaringType;
				if (type.Inherits ("System", "Exception"))
					Runner.Report (method, instruction, Severity.Medium, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
