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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;


namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks if a type could declare it implements an interface.
	/// Usually, adding the interface in the type declaration will make code
	/// more reusable and easier to understand.
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

	[Problem ("This type implements an interface that it does not declare to implement.")]
	[Solution ("If the interface matches the semantics of the type, add it to the type.")]
	public class ConsiderAddingInterfaceRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			//type does not apply if not an interface or is an empty interface
			if (!type.IsInterface || type.Methods.Count == 0)
				return RuleResult.DoesNotApply;

			foreach (AssemblyDefinition assembly in Runner.Assemblies)
				CheckAssemblyTypes (assembly, type);

			return Runner.CurrentRuleResult;
		}

		private void CheckAssemblyTypes (AssemblyDefinition assembly, TypeDefinition iface)
		{
			//return now if iface is an internal interface and we are not on same assenbly
			//TODO: take into account [InternalsVisibleTo] on iface's assembly
			if (!iface.IsVisible() && iface.Module.Assembly != assembly)
				return;

			foreach (ModuleDefinition module in assembly.Modules) {
				foreach (TypeDefinition type in module.Types) {
					if (DoesTypeStealthilyImplementInterface (type, iface)) {
						string msg = string.Format ("Type implements '{0}' interface but does not declare it.", iface);
						// use our own Defect since the *real* target (of analysis) is 'type' not 'iface'
						Runner.Report (new Defect (this, type, iface, Severity.Medium, Confidence.High, msg));
					}
				}
			}
		}

		private static bool DoesTypeStealthilyImplementInterface(TypeDefinition type, TypeDefinition iface)
		{
			//ignore already uninteresting types below (self, enum, struct, static class)
			if (type == iface || type.IsEnum || type.IsValueType || type.IsStatic ())
				return false;

			//if type has less methods than the interface no need to check further
			if (type.Methods.Count < iface.Methods.Count)
				return false;

			//type already publicly says it implements the interface
			if (type.Implements (iface.FullName))
				return false;

			foreach (MethodDefinition m in iface.Methods) {
				//if any candidate fails we can return right away
				//since the interface will never be fully implemented
				MethodDefinition candidate = type.GetMethod (MethodAttributes.Public, m.Name);
				if (null == candidate || !candidate.IsPublic || candidate.IsStatic)
					return false;

				//ok interesting candidate! let's check if it matches the signature
				if (!AreSameOriginalTypes (m.ReturnType.ReturnType, candidate.ReturnType.ReturnType))
					return false;
				if (m.Parameters.Count != candidate.Parameters.Count)
					return false;
				if (m.GenericParameters.Count != candidate.GenericParameters.Count)
					return false;
				for (int i = 0; i < m.Parameters.Count; ++i)
					if (!AreSameOriginalTypes (m.Parameters [i].ParameterType, candidate.Parameters [i].ParameterType))
						return false;
			}

			return true;
		}

		private static bool AreSameOriginalTypes (TypeReference a, TypeReference b)
		{
			return a.GetOriginalType ().FullName == b.GetOriginalType ().FullName;
		}

	}

}

