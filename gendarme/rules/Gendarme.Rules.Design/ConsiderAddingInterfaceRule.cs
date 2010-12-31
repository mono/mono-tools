//
// Gendarme.Rules.Design.ConsiderAddingInterfaceRule
//
// Authors:
//	Cedric Vivier  <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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
using System.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;


namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if a type implements members which are declared in an
	/// interface, but the type does not implement the interface. Implementing
	/// the interface will normally make the type more reuseable and will help
	/// clarify the type's semantics.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public interface IDoable {
	///	public void Do ();
	/// }
	/// 
	/// public class MyClass {
	///	public void Do ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public interface IDoable {
	///	public void Do ();
	/// }
	/// 
	/// public class MyClass : IDoable {
	///	public void Do ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type implements an interface's members, but does not implement the interface.")]
	[Solution ("If the semantics of the type's  members are compatible with the interface then inherit from the interface. Otherwise ignore the defect.")]
	public class ConsiderAddingInterfaceRule : Rule, ITypeRule {

		private bool reference_only = true;

		public bool ReferencesOnly {
			get { return reference_only; }
			set { reference_only = value; }
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			//type does not apply if not an interface or is an empty interface
			if (!type.IsInterface || !type.HasMethods)
				return RuleResult.DoesNotApply;

			//TODO: take into account [InternalsVisibleTo] on iface's assembly
			AssemblyDefinition current_assembly = type.Module.Assembly;
			if (type.IsVisible ()) {
				// We should not, by default, promote the implementation of interfaces in assemblies that
				// do not, already, refer to the current one because:
				// (a) we could be suggesting circular references (solvable, or not, by refactoring)
				// (b) it has a very HIGH performance cost, with verry LITTLE value (in # of defects)
				string current_assembly_name = current_assembly.Name.Name;
				foreach (AssemblyDefinition assembly in Runner.Assemblies) {
					// by default only process assemblies (from the set) that refers to the current one
					// or the current one itself
					if (!ReferencesOnly || (current_assembly_name == assembly.Name.Name) ||
						assembly.References (current_assembly_name)) {
						CheckAssemblyTypes (assembly, type);
					}
				}
			} else {
				// if the interface is not visible then we only check this assembly
				CheckAssemblyTypes (current_assembly, type);
			}

			return Runner.CurrentRuleResult;
		}

		private void CheckAssemblyTypes (AssemblyDefinition assembly, TypeDefinition iface)
		{
			foreach (ModuleDefinition module in assembly.Modules) {
				foreach (TypeDefinition type in module.GetAllTypes ()) {
					if (DoesTypeStealthilyImplementInterface (type, iface)) {
						string msg = string.Format ("Type implements '{0}' interface but does not declare it.", iface);
						// use our own Defect since the *real* target (of analysis) is 'type' not 'iface'
						Runner.Report (new Defect (this, type, iface, Severity.Medium, Confidence.High, msg));
					}
				}
			}
		}

		private static bool DoesTypeStealthilyImplementInterface (TypeDefinition type, TypeDefinition iface)
		{
			//ignore already uninteresting types below (self, enum, struct, static class)
			if (type == iface || type.IsEnum || type.IsValueType || type.IsStatic ())
				return false;

			//if type has less methods than the interface no need to check further
			if (!type.HasMethods)
				return false;
			IList<MethodDefinition> mdc = iface.Methods;
			if (type.Methods.Count < mdc.Count)
				return false;

			//type already publicly says it implements the interface
			if (type.Implements (iface.FullName))
				return false;

			foreach (MethodDefinition m in mdc) {
				//if any candidate fails we can return right away
				//since the interface will never be fully implemented
				MethodDefinition candidate = type.GetMethod (MethodAttributes.Public, m.Name);
				if (null == candidate || !candidate.IsPublic || candidate.IsStatic)
					return false;

				//ok interesting candidate! let's check if it matches the signature
				if (!m.CompareSignature (candidate))
					return false;
			}

			return true;
		}
	}
}

