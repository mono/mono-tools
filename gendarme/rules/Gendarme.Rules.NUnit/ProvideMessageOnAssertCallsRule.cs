// 
// Gendarme.Rules.NUnit.ProvideMessageOnAssertCallsRule
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.NUnit {

	/// <summary>
	/// This rule checks that all Assert.* methods are calling with 'message'
	/// parameter, which helps to easily identify failing test.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Test]
	/// public void TestThings ()
	/// {
	///	Assert.AreEqual(10, 20);
	///	Assert.AreEqual(30, 40);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Test]
	/// public void TestThings ()
	/// {
	///	Assert.AreEqual(10, 20, "10 equal to 20 test");
	///	Assert.AreEqual(30, 40, "30 equal to 40 test");
	/// </code>
	/// </example>
	/// <remarks>
	/// This rule will not report any problems if only one Assert.* call was made 
	/// inside a method, because it's easy to identify failing test in this case.</remarks>

	[Problem ("Assert.* methods being called without 'message' parameter, which helps to easily identify failing test.")]
	[Solution ("Add string 'message' parameter to the calls.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ProvideMessageOnAssertCallsRule : NUnitRule, IMethodRule {

		// Assert.* methods that do not have an override with the 'string message'
		HashSet<string> exceptions = new HashSet<string> {
			"Equals",
			"ReferenceEquals",
		};

		private int reportCounter = 0;
		private Defect defectDelayed;

		public RuleResult CheckMethod (MethodDefinition method)
		{
			reportCounter = 0;
			if (!method.HasBody || !method.IsTest ())
				return RuleResult.DoesNotApply;

			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;


			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;

				MethodReference m = (instruction.Operand as MethodReference);
				if (m == null || !m.DeclaringType.IsNamed ("NUnit.Framework", "Assert") ||
					exceptions.Contains (m.Name))
					continue;

				bool foundMessage = false;
				if (m.HasParameters) {
					MethodDefinition resolvedMethod = m.Resolve ();
					if (resolvedMethod == null)
						continue;
					foreach (ParameterDefinition parameter in resolvedMethod.Parameters) {
						if (parameter.ParameterType.IsNamed ("System", "String") &&
							parameter.Name == "message") {
							foundMessage = true;
							break;
						}
					}
				}
				if (!foundMessage)
					DelayedReport (new Defect (this, method, method, instruction, Severity.Medium, Confidence.High));
			}
			return Runner.CurrentRuleResult;
		}

		// reports only if it was called more than one time
		private void DelayedReport (Defect defect)
		{
			reportCounter++;
			if (reportCounter > 1) {
				if (reportCounter == 2)
					Runner.Report (defectDelayed);
				Runner.Report (defect);
			} else
				defectDelayed = defect;
		}
	}
}
