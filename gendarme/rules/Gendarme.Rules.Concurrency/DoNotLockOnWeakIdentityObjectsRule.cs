//
// Gendarme.Rules.Concurrency.DoNotLockOnWeakIdentityObjectsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule ensures there aren't locked objects with weak identity.
	/// An object with weak identity means that it can be accessed across
	/// different application domains and may cause deadlocks or other
	/// concurrency issues.
	/// The following types have a weak identity:
	/// <list type="bullet"> 
	/// <item> 
	/// <description><c>System.MarshalByRefObject</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.OutOfMemoryException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Reflection.MemberInfo</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Reflection.ParameterInfo</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.ExecutionEngineException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.StackOverflowException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.String</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Threading.Thread</c></description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void WeakIdLocked () 
	/// {
	/// 	lock ("CustomString") {
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void WeakIdNotLocked ()
	/// {
	/// 	Phone phone = new Phone ();
	///     lock (phone) {
	///     }
	/// }
	/// </code>
	/// </example>

	[Problem ("This method use a lock on a object with a weak identity, i.e. accessible across application domains.")]
	[Solution ("To be safe from outside always lock on something that is totally private to your code.")]
	public class DoNotLockOnWeakIdentityObjectsRule : LockAnalyzerRule {

		private static TypeReference GetType (MethodDefinition method, Instruction ins)
		{
			VariableDefinition variable = ins.GetVariable (method);
			if (variable != null)
				return variable.VariableType;

			switch (ins.OpCode.Code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int index = (ins.OpCode.Code - Code.Ldarg_0);
				if (!method.IsStatic) {
					index--;
					if (index < 0)
						return method.DeclaringType; // this
				}
				return method.Parameters [index].ParameterType;
			case Code.Ldarg:
			case Code.Ldarg_S:
				return (ins.Operand as ParameterDefinition).ParameterType;
			case Code.Ldfld:
			case Code.Ldsfld:
				return (ins.Operand as FieldReference).FieldType;
			case Code.Call:
			case Code.Callvirt:
				return (ins.Operand as MethodReference).ReturnType.ReturnType;
			default:
				return null;
			}
		}

		private static string [] unsealed_types = new string[] {
			"System.MarshalByRefObject",
			"System.OutOfMemoryException",
			"System.Reflection.MemberInfo",
			"System.Reflection.ParameterInfo"
		};

		public override void Analyze (MethodDefinition method, Instruction ins)
		{
			// well original instruction since this is where we will report the defect
			Instruction call = ins;
			while (ins.Previous != null) {
				ins = ins.Previous;
				TypeReference type = GetType (method, ins);
				if (type == null)
					continue;

				// fast check for sealed types
				switch (type.FullName) {
				case "System.ExecutionEngineException":
				case "System.StackOverflowException":
				case "System.String":
				case "System.Threading.Thread":
					Runner.Report (method, call, Severity.High, Confidence.Normal, type.FullName);
					return;
				default:
					foreach (string unsealed in unsealed_types) {
						if (!type.Inherits (unsealed))
							continue;

						string msg = String.Format ("'{0}' inherits from '{1}'.", type.FullName, unsealed);
						Runner.Report (method, call, Severity.High, Confidence.Normal, msg);
					}
					return;
				}
			}
		}
	}
}
