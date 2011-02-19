// 
// Gendarme.Rules.Gendarme.DefectsMustBeReportedRule
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
	/// This rule checks if at least one method in types implementing
	/// IRule is calling Runner.Report.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BadRule : Rule, ITypeRule 
	/// {
	///	public RuleResult CheckType (TypeDefinition type) 
	///	{
	///		return RuleResult.Failure;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class BadRule : Rule, ITypeRule 
	/// {
	///	public RuleResult CheckType (TypeDefinition type) 
	///	{
	///		Runner.Report(type, Severity.Low, Confidence.Total);
	///		return RuleResult.Failure;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// This rule checks if Runner.Report is called directly anywhere in rules' methods but it does not 
	/// check if it being called in the base type or somewhere else, so some false positives are possible.</remarks>

	[Problem ("Gendarme rule does not call Runner.Report, so found failures will not appear in Gendarme report.")]
	[Solution ("Add Runner.Report call")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DefectsMustBeReportedRule : GendarmeRule, ITypeRule {
		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsAbstract || !type.HasMethods || !type.Implements ("Gendarme.Framework", "IRule"))
				return RuleResult.DoesNotApply;

			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor || !method.HasBody || !OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
					continue;

				foreach (Instruction instruction in method.Body.Instructions) {
					if (instruction.OpCode.FlowControl != FlowControl.Call)
						continue;
					
					MethodReference m = (instruction.Operand as MethodReference);
					if (m == null || (m.Name != "Report"))
						continue;
					if (m.DeclaringType.IsNamed ("Gendarme.Framework", "IRunner"))
						return RuleResult.Success;
				}
				
			}
			
			// we have not found a call, so report failure
			Runner.Report (type, Severity.High, Confidence.Normal);
			return RuleResult.Failure;
		}
	}
}
