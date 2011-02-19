// 
// Gendarme.Rules.Exceptions.AvoidArgumentExceptionDefaultConstructorRule
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

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule checks that every <c>System.ArgumentException</c>, 
	/// <c>System.ArgumentNullException</c>, <c>System.ArgumentOutOfRangeException</c>, or
	/// <c>System.DuplicateWaitObjectException</c> exception created is provided with some
	/// useful information about the exception being thrown, minimally the parameter name.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Add (object key, object value)
	/// {
	///	if ((obj == null) || (key == null)) {
	///		throw new ArgumentNullException ();
	///	}
	///	Inner.Add (key, value);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void Add (object key, object value)
	/// {
	///	if (key == null) {
	///		throw new ArgumentNullException ("key");
	///	}
	///	if (obj == value) {
	///		throw new ArgumentNullException ("value");
	///	}
	///	Inner.Add (key, value);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method creates an ArgumentException (or derived class) but does not supply any useful information, such as the argument name, to it.")]
	[Solution ("Provide more information when creating the exception.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidArgumentExceptionDefaultConstructorRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains a NewObj instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Newobj))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				// look where the exception was created (which might be very far from where it's actually thrown)
				if (ins.OpCode.Code != Code.Newobj)
					continue;

				// check if the ctor used is the default, parameter-less, one
				MethodReference ctor = (ins.Operand as MethodReference);
				if (ctor.HasParameters)
					continue;

				TypeReference type = ctor.DeclaringType;
				string name = type.Name;
				if (type.Namespace == "System") {
					// most common cases
					switch (name) {
					case "ArgumentException":
					case "ArgumentNullException":
					case "ArgumentOutOfRangeException":
					case "DuplicateWaitObjectException":
						Runner.Report (method, ins, Severity.Medium, Confidence.Total, type.GetFullName ());
						continue;
					}
				}

				if (name.EndsWith ("Exception", StringComparison.Ordinal)) {
					if (type.Inherits ("System", "ArgumentException"))
						Runner.Report (method, ins, Severity.Medium, Confidence.Total, type.GetFullName ());
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
