//
// Gendarme.Rules.Design.DisposableFieldsShouldBeDisposedRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008-2010 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// The rule inspects all fields for disposable types and, if <c>System.IDisposable</c> is
	/// implemented, checks that the type's <c>Dispose</c> method does indeed call <c>Dispose</c> on
	/// all disposable fields.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class DoesNotDisposeMember : IDisposable {
	///	byte[] buffer;
	///	IDisposable field;
	///	
	///	public void Dispose ()
	///	{
	///		buffer = null;
	///		// field is not disposed
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class DisposePattern : IDisposable {
	///	byte[] buffer;
	///	IDisposable field;
	///	bool disposed;
	///	
	///	public void Dispose ()
	///	{
	///		Dispose (true);
	///	}
	///	
	///	private void Dispose (bool disposing)
	///	{
	///		if (!disposed) {
	///			if (disposing) {
	///				field.Dispose ();
	///			}
	///			buffer = null;
	///			disposed = true;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains disposable field(s) which aren't disposed.")]
	[Solution ("Ensure that every disposable field(s) is correctly disposed.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
	public class DisposableFieldsShouldBeDisposedRule : Rule, ITypeRule {

		private List<FieldDefinition> disposeableFields = new List<FieldDefinition> ();

		private static MethodDefinition GetNonAbstractMethod (TypeReference type, MethodSignature signature)
		{
			MethodDefinition method = type.GetMethod (signature);
			if ((method == null) || method.IsAbstract)
				return null;
			return method;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to types and structures (value types)
			if (type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// rule doesn't apply to generated code (out of developer control)
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// note: other rule will complain if there are disposable or native fields
			// in a type that doesn't implement IDisposable, so we don't bother here
			if (!type.Implements ("System", "IDisposable"))
				return RuleResult.DoesNotApply;

			MethodDefinition implicitDisposeMethod = GetNonAbstractMethod (type, MethodSignatures.Dispose);
			MethodDefinition explicitDisposeMethod = GetNonAbstractMethod (type, MethodSignatures.DisposeExplicit);
			bool has_implicit = (implicitDisposeMethod != null);
			bool has_explicit = (explicitDisposeMethod != null);

			// note: handled by TypesWithDisposableFieldsShouldBeDisposableRule
			if (!has_implicit && !has_explicit) 
				return RuleResult.Success;

			//Check for baseDispose
			CheckBaseDispose (type, implicitDisposeMethod, explicitDisposeMethod);

			// skip the rest if the type does not provide any fields itself
			if (!type.HasFields)
				return Runner.CurrentRuleResult;

			//List of disposeableFields
			disposeableFields.Clear ();
			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				if (field.IsGeneratedCode ())
					continue;
				if (field.FieldType.IsArray)
					continue;
				TypeDefinition fieldType = field.FieldType.Resolve ();
				if (fieldType == null)
					continue;
				if (fieldType.Implements ("System", "IDisposable"))
					disposeableFields.Add (field);
			}

			// nothing to check for, take shortcut out
			if (disposeableFields.Count == 0)
				return Runner.CurrentRuleResult;

			List<FieldDefinition> iList;
			List<FieldDefinition> eList;

			if (has_implicit && has_explicit) {
				iList = disposeableFields;
				eList = new List<FieldDefinition> (iList);
			} else {
				eList = iList = disposeableFields;
			}

			if (has_implicit)
				CheckIfAllFieldsAreDisposed (implicitDisposeMethod, iList);
			if (has_explicit)
				CheckIfAllFieldsAreDisposed (explicitDisposeMethod, eList);

			return Runner.CurrentRuleResult;
		}

		private void CheckBaseDispose (TypeDefinition type, MethodDefinition implicitDisposeMethod, MethodDefinition explicitDisposeMethod)
		{
			TypeDefinition baseType = type;
			while (!baseType.BaseType.IsNamed ("System", "Object")) {
				baseType = baseType.BaseType.Resolve ();
				// also checks parents, so no need to search further
				if ((baseType == null) || !baseType.Implements ("System", "IDisposable"))
					break;

				//we just check for Dispose() here
				MethodDefinition baseDisposeMethod = GetNonAbstractMethod (baseType, MethodSignatures.Dispose);
				if (baseDisposeMethod == null)
					continue; // no dispose method (yet) or an abstract one

				if (implicitDisposeMethod != null)
					CheckIfBaseDisposeIsCalled (implicitDisposeMethod, baseDisposeMethod);
				if (explicitDisposeMethod != null)
					CheckIfBaseDisposeIsCalled (explicitDisposeMethod, baseDisposeMethod);
				break;
			}
		}

		private void CheckIfBaseDisposeIsCalled (MethodDefinition method, MemberReference baseMethod)
		{
			bool found = false;

			if (method.HasBody) {
				OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
				if (bitmask.Get (Code.Ldarg_0) && (OpCodeBitmask.Calls.Intersect (bitmask))) {

					//Check for a call to base.Dispose();
					foreach (Instruction ins in method.Body.Instructions) {
						if (ins.OpCode.Code != Code.Ldarg_0) //ldarg_0 (this)
							continue;

						Instruction call = ins.Next; //call baseMethod
						if (call == null)
							continue;
						if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
							continue;
						MethodReference calledMethod = (MethodReference) call.Operand;
						if (calledMethod.GetFullName () != baseMethod.GetFullName ())
							continue;
						found = true;
					}
				}
			}

			if (!found) {
				string s = String.Format (CultureInfo.InvariantCulture, "{0} should call base.Dispose().", method.GetFullName ());
				Runner.Report (method, Severity.Medium, Confidence.High, s);
			}
		}

		static readonly MethodSignature DisposeBool = new MethodSignature ("Dispose", "System.Void", new string [] { "System.Boolean" });

		private void CheckIfAllFieldsAreDisposed (MethodDefinition method, ICollection<FieldDefinition> fields)
		{
			if (method.HasBody) {
				OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
				if (OpCodeBitmask.Calls.Intersect (bitmask)) {
					//Check if Dispose(bool) is called and if all fields are disposed
					foreach (Instruction ins in method.Body.Instructions) {
						switch (ins.OpCode.Code) {
						case Code.Call:
						case Code.Callvirt:
							MethodDefinition md = (ins.Operand as MethodDefinition);
							if ((md != null) && DisposeBool.Matches (md))
								ProcessMethod (md, fields);
							break;
						}
					}
					// besides the call[virt] if must have a Ldarg0 and Ldfld
					if (bitmask.Get (Code.Ldarg_0) && bitmask.Get (Code.Ldfld))
						ProcessMethod (method, fields);
				}
			}

			if (fields.Count == 0)
				return;

			foreach (FieldDefinition field in fields) {
				string s = String.Format (CultureInfo.InvariantCulture, 
					"Since {0} is Disposable {1}() should call {0}.Dispose()", field.Name, method.Name);
				Runner.Report (field, Severity.High, Confidence.High, s);
			}
		}

		// note: we CANNOT use OpCodeEngine inside this call since it's also used on code
		// that can resides outside the assembly set (i.e. that the engine did not process)
		private static void ProcessMethod (MethodDefinition method, ICollection<FieldDefinition> fieldsToDispose)
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
				if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
					continue;
				if (!MethodSignatures.Dispose.Matches (call.Operand as MethodReference))
					continue;

				FieldDefinition field = (ldfld.Operand as FieldDefinition);
				if (field != null)
					fieldsToDispose.Remove (field);
			}
		}
	}
}
