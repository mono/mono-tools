// 
// Gendarme.Rules.Exceptions.AvoidThrowingBasicExceptionsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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

namespace Gendarme.Rules.Exceptions {

	[Problem ("This method creates (and probably throws) an exception of Exception, ApplicationException or SystemException type.")]
	[Solution ("Try to use more specific exception types. If none of existing types meet your needs, create custom exception class that inherits from System.Exception or any appropriate descendant of it.")]
	public class AvoidThrowingBasicExceptionsRule : Rule, IMethodRule {
	
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);
			// if the module does not reference any of these types, don't analyze it
			// (unless this is corlib itself since they're defined in it :-)
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == Constants.Corlib) ||
					e.CurrentModule.TypeReferences.ContainsType ("System.Exception") ||
					e.CurrentModule.TypeReferences.ContainsType ("System.ApplicationException") ||
					e.CurrentModule.TypeReferences.ContainsType ("System.SystemException");
			};
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, the rule doesn't apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			
			// look for newobj instructions
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Newobj)
					continue;
				
				// obtain a reference to constructor
				MethodReference ctor = (ins.Operand as MethodReference);
				
				// what type is it?
				string name = ctor.DeclaringType.FullName;

				// if this is not (Application|System)?Exception, go on
				if (name != "System.Exception"
				    && name != "System.SystemException"
				    && name != "System.ApplicationException")
					continue;

				// report a defect (exception of a basic type is created)
				Runner.Report (method, ins, Severity.Medium, Confidence.High, string.Empty);
			}
			
			return Runner.CurrentRuleResult;
		}
	}
}
