//
// Gendarme.Rules.Design.DisposableFieldsShouldBeDisposedRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	[Problem ("This type contains disposable field(s) that aren't disposed.")]
	[Solution ("Ensure that every disposable field(s) are disposed correctly.")]
	public class DisposableFieldsShouldBeDisposedRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule doesn't apply to generated code (out of developer control)
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// note: other rule will complain if there are disposable or native fields
			// in a type that doesn't implement IDisposable, so we don't bother here
			if (!type.Implements ("System.IDisposable"))
				return RuleResult.DoesNotApply;

			MethodDefinition implicitDisposeMethod = type.GetMethod (MethodSignatures.Dispose);
			MethodDefinition explicitDisposeMethod = type.GetMethod (MethodSignatures.DisposeExplicit);

			if (implicitDisposeMethod == null || implicitDisposeMethod.IsAbstract)
				implicitDisposeMethod = null;
			if (explicitDisposeMethod == null || explicitDisposeMethod.IsAbstract)
				explicitDisposeMethod = null;

			// note: handled by TypesWithDisposableFieldsShouldBeDisposableRule
			if (implicitDisposeMethod == null && explicitDisposeMethod == null) 
				return RuleResult.Success;

			//Check for baseDispose
			TypeDefinition baseType = type;
			while (baseType.BaseType.FullName != "System.Object") {
				baseType = baseType.BaseType.Resolve ();
				if (baseType == null)
					break;
				// also checks parents, so no need to search further
				if (!baseType.Implements ("System.IDisposable"))
					break;
				//we just check for Dispose() here
				MethodDefinition baseDisposeMethod = baseType.GetMethod (MethodSignatures.Dispose); 
				if (baseDisposeMethod == null)
					continue; //no dispose method (yet)
				if (baseDisposeMethod.IsAbstract)
					break; //abstract
				if (implicitDisposeMethod != null)
					CheckIfBaseDisposeIsCalled (implicitDisposeMethod, baseDisposeMethod);
				if (explicitDisposeMethod != null)
					CheckIfBaseDisposeIsCalled (explicitDisposeMethod, baseDisposeMethod);
				break;
			}

			//List of disposeableFields
			List<FieldDefinition> disposeableFields = new List<FieldDefinition> ();
			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				if (field.FieldType.IsArray ())
					continue;
				TypeDefinition fieldType = field.FieldType.Resolve ();
				if (fieldType == null)
					continue;
				if (fieldType.Implements ("System.IDisposable"))
					disposeableFields.Add (field);
			}

			List<FieldDefinition> iList;
			List<FieldDefinition> eList;

			if (implicitDisposeMethod != null && explicitDisposeMethod != null) {
				iList = disposeableFields;
				eList = new List<FieldDefinition> (iList);
			} else {
				eList = iList = disposeableFields;
			}

			if (implicitDisposeMethod != null)
				CheckIfAllFieldsAreDisposed (implicitDisposeMethod, iList);
			if (explicitDisposeMethod != null)
				CheckIfAllFieldsAreDisposed (explicitDisposeMethod, eList);

			return Runner.CurrentRuleResult;
		}

		private void CheckIfBaseDisposeIsCalled (MethodDefinition method, MethodDefinition baseMethod)
		{
			bool found = false;
			//Check for a call to base.Dispose();
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Ldarg_0) //ldarg_0 (this)
					continue;

				Instruction call = ins.Next; //call baseMethod
				if (call == null)
					continue;
				if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Calli && call.OpCode.Code != Code.Callvirt)
					continue;
				MethodReference calledMethod = (MethodReference) call.Operand;
				if (calledMethod.ToString () != baseMethod.ToString ())
					continue;
				found = true;
			}

			if (!found) {
				string s = String.Format ("{0} should call base.Dispose().", method.ToString ());
				Runner.Report (method, Severity.Medium, Confidence.High, s);
			}
		}

		private void CheckIfAllFieldsAreDisposed (MethodDefinition method, List<FieldDefinition> fields)
		{
			//Check if Dispose(bool) is called and if all fields are disposed
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Calli && ins.OpCode.Code != Code.Callvirt)
					continue;
				MethodDefinition calledMethod = ins.Operand as MethodDefinition;
				if (calledMethod == null || calledMethod.DeclaringType != method.DeclaringType)
					continue;
				if (calledMethod.Name != "Dispose")
					continue;
				if (calledMethod.Parameters.Count != 1)
					continue;
				if (calledMethod.Parameters [0].ParameterType.FullName != "System.Boolean")
					continue;
				ProcessMethod (calledMethod, fields);
				break;
			}
			ProcessMethod (method, fields);

			if (fields.Count == 0)
				return;

			foreach (FieldDefinition field in fields) {
				string s = string.Format ("{0} is Disposable. {1}() should call {0}.Dispose()", field.Name, method.Name);
				Runner.Report (field, Severity.High, Confidence.High, s);
			}
		}

		private static void ProcessMethod (MethodDefinition method, List<FieldDefinition> fieldsToDispose)
		{
			if (!method.HasBody)
				return;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Ldarg_0) //ldarg_0 (this)
					continue;

				Instruction ldfld = ins.Next;
				if (ldfld == null)
					continue;
				if (ldfld.OpCode.Code != Code.Ldfld) //ldfld
					continue;

				Instruction call = ldfld.Next; //call Dispose
				if (call == null)
					continue;
				if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Calli && call.OpCode.Code != Code.Callvirt)
					continue;
				MethodReference calledMethod = (MethodReference) call.Operand;
				if (calledMethod.Name != "Dispose")
					continue;
				if (calledMethod.Parameters.Count != 0)
					continue;
				if (calledMethod.ReturnType.ReturnType.FullName != "System.Void")
					continue;

				FieldDefinition field = (ldfld.Operand as FieldDefinition);
				if (field != null)
					fieldsToDispose.Remove (field);
			}
		}
	}
}
