//
// Gendarme.Rules.BadPractice.EqualsShouldHandleNullArgRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule ensures that <c>Equals(object)</c> methods return <c>false</c> when the 
	/// object parameter is <c>null</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool Equals (object obj)
	/// {
	///	// this would throw a NullReferenceException instead of returning false
	///	return ToString ().Equals (obj.ToString ());
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public override bool Equals (object obj)
	/// {
	///	if (obj == null) {
	///		return false;
	///	}
	///	return ToString ().Equals (obj.ToString ());
	/// }
	/// </code>
	/// </example>

	[Problem ("This Equals method does not handle null arguments as it should.")]
	[Solution ("Modify the method implementation to return false if a null argument is found.")]
	public class EqualsShouldHandleNullArgRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rules applies to types that overrides System.Object.Equals(object)
			MethodDefinition method = type.GetMethod (MethodSignatures.Equals);
			if ((method == null) || !method.HasBody || method.IsStatic)
				return RuleResult.DoesNotApply;

			// rule applies

			// scan IL to see if null is checked and false returned
			if (CheckSequence (method.Body.Instructions [0], type))
				return RuleResult.Success;

			Runner.Report (method, Severity.Medium, Confidence.High);
			return RuleResult.Failure;
		}

		private static bool CheckSequence (Instruction ins, TypeDefinition type)
		{
			bool previous_ldarg1 = false;
			while (ins != null) {
				switch (ins.OpCode.Code) {
				case Code.Ldarg_1:
					previous_ldarg1 = true;
					ins = ins.Next;
					continue;
				case Code.Brfalse:
				case Code.Brfalse_S:
					if (previous_ldarg1)
						return CheckSequence (ins.Operand as Instruction, type);
					break;
				case Code.Ceq:
					if ((previous_ldarg1) && (ins.Next.OpCode.Code == Code.Ret))
						return true;
					break;
				case Code.Bne_Un:
				case Code.Bne_Un_S:
					if (CheckSequence (ins.Operand as Instruction, type))
						return true;
					break;
				case Code.Isinst:
					if (!previous_ldarg1)
						break;
					if ((ins.Operand as TypeReference) != type)
						break;
					switch (ins.Next.OpCode.Code) {
					case Code.Brfalse:
					case Code.Brfalse_S:
						return CheckSequence (ins.Next.Operand as Instruction, type);
					}
					ins = ins.Next;
					continue;
				case Code.Ret:
					// we're not sure of what the other call will return
					// but since it's probably a base class then the same rule apples there
					if (ins.Previous.OpCode.FlowControl == FlowControl.Call)
						return true;
					return (ins.Previous.OpCode.Code != Code.Ldc_I4_1);
				case Code.Throw:
					// don't report when an exception is thrown
					return true;
				default:
					ins = ins.Next;
					continue;
				}

				previous_ldarg1 = false;
				ins = ins.Next;
			}
			return false;
		}
	}
}
