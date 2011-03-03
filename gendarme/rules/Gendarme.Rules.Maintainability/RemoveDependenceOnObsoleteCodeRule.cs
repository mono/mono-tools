//
// Gendarme.Rules.Maintainability.RemoveDependenceOnObsoleteCodeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule will warn you if your code depends on (e.g. inherit, implement, call...)
	/// code that is decorated with the <c>[Obsolete]</c> attribute.
	/// Note that the rule does not report <c>[Obsolete]</c> types, methods...
	/// but only their use by your code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Obsolete ("Limited to In32.MaxValue, use new Int64 ReportAll method")]
	/// abstract int Report (IList list);
	/// 
	/// abstract long ReportAll (IList list);
	/// 
	/// public int GetCount ()
	/// {
	///	// this method is not ok since it use an obsolete method
	///	return Report (list);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (dependency removed):
	/// <code>
	/// [Obsolete ("Limited to In32.MaxValue, use new Int64 ReportAll method")]
	/// abstract int Report (IList list);
	/// 
	/// abstract long ReportAll (IList list);
	/// 
	/// // this method is correct but this changed the public API
	/// public long GetCount ()
	/// {
	///	return ReportAll (list);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (decorated as [Obsolete]):
	/// <code>
	/// [Obsolete ("Limited to In32.MaxValue, use new Int64 ReportAll method")]
	/// abstract int Report (IList list);
	/// 
	/// abstract long ReportAll (IList list);
	/// 
	/// [Obsolete ("Limited to In32.MaxValue, use new Int64 GetLongCount method")]
	/// public int GetCount ()
	/// {
	///	// this method is now correct since it is decorated with [Obsolete]
	///	return Report (list);
	/// }
	///
	/// public long GetLongCount ()
	/// {
	///	return ReportAll (list);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("This type or method is not obsolete but depends on code decorated with [Obsolete] attribute.")]
	[Solution ("Remove the dependence on obsolete code or decorate this code with the [Obsolete] attribute.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class RemoveDependenceOnObsoleteCodeRule : Rule, ITypeRule, IMethodRule {

		static Dictionary<TypeReference, bool> types = new Dictionary<TypeReference, bool> ();
		static Dictionary<MethodReference, bool> methods = new Dictionary<MethodReference, bool> ();
		static Dictionary<FieldReference, bool> fields = new Dictionary<FieldReference, bool> ();

		static bool IsObsolete (TypeReference type)
		{
			if (type == null)
				return false;

			bool obsolete = false;
			if (!types.TryGetValue (type, out obsolete)) {
				TypeDefinition t = type.Resolve ();
				obsolete = t.HasAttribute ("System", "ObsoleteAttribute");
				types.Add (type, obsolete);
			}
			return obsolete;
		}

		static bool IsObsolete (MethodReference method)
		{
			if (method == null)
				return false;

			bool obsolete = false;
			if (!methods.TryGetValue (method, out obsolete)) {
				MethodDefinition md = method.Resolve ();
				obsolete = md.HasAttribute ("System", "ObsoleteAttribute");
				methods.Add (method, obsolete);
			}
			return obsolete;
		}

		static bool IsObsolete (FieldReference field)
		{
			if (field == null)
				return false;

			bool obsolete = false;
			if (!fields.TryGetValue (field, out obsolete)) {
				FieldDefinition fd = field.Resolve ();
				obsolete = fd.HasAttribute ("System", "ObsoleteAttribute");
				fields.Add (field, obsolete);
			}
			return obsolete;
		}

		void CheckBaseType (TypeDefinition type)
		{
			if (!IsObsolete (type.BaseType))
				return;

			string msg = String.Format (CultureInfo.InvariantCulture, "Inherit from obsolete type '{0}'.", type.BaseType);
			Runner.Report (type, type.IsVisible () ? Severity.High : Severity.Medium, Confidence.Total, msg);
		}

		void CheckInterfaces (TypeDefinition type)
		{
			foreach (TypeReference intf in type.Interfaces) {
				if (IsObsolete (intf)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Implement obsolete interface '{0}'.", intf);
					Runner.Report (type, type.IsVisible () ? Severity.Medium : Severity.Low, Confidence.Total, msg);
				}
			}
		}

		void CheckFields (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (IsObsolete (field.FieldType)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Field type '{0}' is obsolete.", field.FieldType);
					Runner.Report (field, field.IsVisible () ? Severity.Medium : Severity.Low, Confidence.Total, msg);
				}
			}
		}

		// check the properties (not the individual getter/setters)
		void CheckProperties (TypeDefinition type)
		{
			foreach (PropertyDefinition property in type.Properties) {
				if (IsObsolete (property.PropertyType)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Property type '{0}' is obsolete.", property.PropertyType);
					bool visible = (IsVisible (property.GetMethod) || IsVisible (property.SetMethod));
					Runner.Report (property, visible ? Severity.High : Severity.Medium, Confidence.Total, msg);
				}
			}
		}

		void CheckEvents (TypeDefinition type)
		{
			foreach (EventDefinition evnt in type.Events) {
				if (IsObsolete (evnt.EventType)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Event type '{0}' is obsolete.", evnt.EventType);
					bool visible = (IsVisible (evnt.AddMethod) || IsVisible (evnt.RemoveMethod) || 
						IsVisible (evnt.InvokeMethod));
					Runner.Report (evnt, visible ? Severity.High : Severity.Medium, Confidence.Total, msg);
				}
			}
		}

		// helper method to avoid calling the same (large) properties more than once -> AvoidRepetitiveCallsToPropertiesRule
		static bool IsVisible (MethodReference method)
		{
			return ((method != null) && method.IsVisible ());
		}

		// Type			Visible		Non-Visible
		// ------------------------------------------------
		// BaseType*		High		Medium
		// Interfaces*		Medium		Low
		// Fields		Medium		Low
		// Properties		High		Medium
		// Events		High		Medium
		// * type visibility

		public RuleResult CheckType (TypeDefinition type)
		{
			// we're not interested in the details of [Obsolete] types
			if (type.HasAttribute ("System", "ObsoleteAttribute"))
				return RuleResult.DoesNotApply;

			// check if we inherit from an [Obsolete] class / struct / enum
			CheckBaseType (type);

			// check if we implement an [Obsolete] interface
			if (type.HasInterfaces)
				CheckInterfaces (type);

			// check fields types
			if (type.HasFields)
				CheckFields (type);

			// check properties (not the getter / setter)
			if (type.HasProperties)
				CheckProperties (type);

			// check events (not add / remove / invoke)
			if (type.HasEvents)
				CheckEvents (type);

			return Runner.CurrentRuleResult;
		}

		void CheckParameters (MethodReference method)
		{
			foreach (ParameterDefinition p in method.Parameters) {
				if (IsObsolete (p.ParameterType)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Parameter type '{0}' is obsolete.", p.ParameterType);
					Runner.Report (p, method.IsVisible () ? Severity.High : Severity.Medium, Confidence.Total, msg);
				}
			}
		}

		void CheckReturnType (MethodReference method)
		{
			TypeReference rt = method.ReturnType;
			if (!IsObsolete (rt))
				return;

			string msg = String.Format (CultureInfo.InvariantCulture, "Return type '{0}' is obsolete.", rt);
			Runner.Report (method, method.IsVisible () ? Severity.High : Severity.Medium, Confidence.Total, msg);
		}

		void CheckVariables (MethodDefinition method)
		{
			foreach (VariableDefinition v in method.Body.Variables) {
				if (IsObsolete (v.VariableType)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Variable type '{0}' is obsolete.", v.VariableType);
					Runner.Report (method, Severity.Low, Confidence.High, msg);
				}
			}
		}

		void CheckTypeCreation (MethodDefinition method, Instruction ins, TypeReference type)
		{
			if (!IsObsolete (type))
				return;

			string msg = String.Format (CultureInfo.InvariantCulture, "Type '{0}' is obsolete.", type);
			Severity severity = type.IsVisible () ? Severity.Medium : Severity.Low;
			Runner.Report (method, ins, severity, Confidence.High, msg);
		}

		void CheckMethodCall (MethodDefinition method, Instruction ins, MethodReference call)
		{
			if (call == null)
				return;

			string msg = null;
			if (IsObsolete (call)) {
				msg = String.Format (CultureInfo.InvariantCulture, "Method '{0}' is obsolete.", call);
			} else {
				TypeReference type = call.DeclaringType;
				if (IsObsolete (type))
					msg = String.Format (CultureInfo.InvariantCulture, "Type '{0}' is obsolete.", type);
			}

			if (msg != null) {
				Severity severity = call.IsVisible () ? Severity.Medium : Severity.Low;
				Runner.Report (method, ins, severity, Confidence.High, msg);
			}
		}

		void CheckFieldAccess (MethodDefinition method, Instruction ins, FieldReference field)
		{
			if (field == null)
				return;

			string msg = null;
			if (IsObsolete (field)) {
				msg = String.Format (CultureInfo.InvariantCulture, "Field '{0}' is obsolete.", field.Name);
			} else if (IsObsolete (field.DeclaringType)) {
				msg = String.Format (CultureInfo.InvariantCulture, "Field type '{0}' is obsolete.", field.FieldType);
			} else {
				return;
			}

			Severity severity = field.IsVisible () ? Severity.Medium : Severity.Low;
			Runner.Report (method, ins, severity, Confidence.High, msg);
		}

		// Method		Visible		Non-Visible
		// ------------------------------------------------
		// Parameters		High		Medium
		// ReturnType		High		Medium
		// Variables		Low		Low
		// Method calls		Medium*		Low*
		// Fields access	Medium*		Low*
		// * target visibility

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// [Obsolete] cannot be applied to property or event accessors
			if (method.IsProperty () || method.IsAddOn || method.IsRemoveOn || method.IsFire)
				return RuleResult.DoesNotApply;

			// if the method is obsolete (directly or because it's type is)
			if (method.HasAttribute ("System", "ObsoleteAttribute") || method.DeclaringType.HasAttribute ("System", "ObsoleteAttribute"))
				return RuleResult.DoesNotApply;

			// check method signature (parameters, return value)
			if (method.HasParameters)
				CheckParameters (method);

			CheckReturnType (method);

			// then check what the IL calls/access
			if (method.HasBody) {
				MethodBody body = method.Body;
				if (body.HasVariables)
					CheckVariables (method);

				foreach (Instruction ins in body.Instructions) {
					switch (ins.OpCode.Code) {
					case Code.Newarr:
					case Code.Newobj:
					case Code.Call:
					case Code.Callvirt:
						CheckMethodCall (method, ins, (ins.Operand as MethodReference));
						break;
					case Code.Initobj:
						CheckTypeCreation (method, ins, (ins.Operand as TypeReference));
						break;
					case Code.Ldfld:
					case Code.Ldflda:
					case Code.Ldsfld:
					case Code.Ldsflda:
					case Code.Stfld:
					case Code.Stsfld:
						CheckFieldAccess (method, ins, (ins.Operand as FieldReference));
						break;
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

