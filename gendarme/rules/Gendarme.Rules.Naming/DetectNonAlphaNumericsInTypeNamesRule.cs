//
// Gendarme.Rules.Naming.AvoidNonAlphanumericIdentifierRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Naming {

	// TODO: This is a confusing rule. It's called "AvoidNonAlphanumericIdentifierRule" and 
	// the summary suggests that it will fire for non-alphanumeric characters, but the
	// problem/solution text and the code only check for underscores. The code suggests
	// that it's only neccesary to check for underscores because the compiler will catch the
	// others which seems to be true for gmcs 2.4. However section 9.4.2 of the standard
	// clearly states that character classes like Lm should be allowed which seems like something
	// the rule should fire for (see http://www.fileformat.info/info/unicode/category/Lm/index.htm).
	// Also other compilers may very well be more lenient about which characters they accept.
	//
	// It seems to me that this rule should explain why underscores and unusual letters are
	// a problem and fire if a character not in [a-zA-Z0-9] is used. And of course the summary
	// and problem texts should be synced up. 

	/// <summary>
	/// This rule ensures that identifiers like assembly names, namespaces, types and 
	/// members names don't have any non-alphanumerical characters inside them. The rule 
	/// will ignore interfaces used for COM interoperability - i.e. decorated with both 
	/// <c>[InterfaceType]</c> and <c>[Guid]</c> attributes.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// namespace New_Namespace {
	/// 
	///	public class My_Custom_Class {
	///	
	///		public int My_Field;
	/// 
	///		public void My_Method (string my_string)
	///		{
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// namespace NewNamespace {
	/// 
	///	public class MyCustomClass {
	/// 
	///		public int MyField;
	///		
	///		public void MyMethod (string myString)
	///		{
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named DetectNonAlphanumericInTypeNamesRule</remarks>

	[Problem ("This namespace, type or member name contains underscore(s).")]
	[Solution ("Remove the underscore from the specified name.")]
	[EngineDependency (typeof (NamespaceEngine))]
	[FxCopCompatibility ("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
	public class AvoidNonAlphanumericIdentifierRule : Rule, IAssemblyRule, IMethodRule, ITypeRule {

		// Compiler generates an error for any other non alpha-numerics than underscore ('_'), 
		// so we just need to check the presence of underscore in method names
		private static bool CheckName (string name, bool special)
		{
			int start = special ? name.IndexOf ('_') + 1: 0;
			return (name.IndexOf ('_', start) == -1);
		}

		private static bool UsedForComInterop (TypeDefinition type)
		{
			return (type.IsInterface && type.HasAttribute ("System.Runtime.InteropServices", "GuidAttribute") &&
				type.HasAttribute ("System.Runtime.InteropServices", "InterfaceTypeAttribute"));
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			if (!CheckName (assembly.Name.Name, false))
				Runner.Report (assembly, Severity.Medium, Confidence.High);

			// check every namespaces inside the assembly using the NamespaceEngine
			foreach (string ns in NamespaceEngine.NamespacesInside (assembly)) {
				if (!CheckName (ns, false))
					Runner.Report (NamespaceDefinition.GetDefinition (ns), Severity.Medium, Confidence.High);
			}
			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// type must be visible and not generated by the compiler (or a tool)
			if (!type.IsVisible () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// the rule does not apply if the code is an interface to COM objects
			if (UsedForComInterop (type))
				return RuleResult.DoesNotApply;

			// check the type name
			if (!CheckName (type.Name, false)) {
				Runner.Report (type, Severity.Medium, Confidence.High);
			}

			// CheckMethod covers methods, properties and events (indirectly)
			// but we still need to cover fields
			if (type.HasFields) {
				bool is_enum = type.IsEnum;
				foreach (FieldDefinition field in type.Fields) {
					if (!field.IsVisible ())
						continue;

					// ignore "value__" inside every enumeration
					if (is_enum && !field.IsStatic)
						continue;

					if (!CheckName (field.Name, false)) {
						Runner.Report (field, Severity.Medium, Confidence.High);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// exclude constrcutors, non-visible methods and generated code
			if (method.IsConstructor || !method.IsVisible () || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// the rule does not apply if the code is an interface to COM objects
			if (UsedForComInterop (method.DeclaringType as TypeDefinition))
				return RuleResult.DoesNotApply;

			// check the method name
			if (!CheckName (method.Name, method.IsSpecialName))
				Runner.Report (method, Severity.Medium, Confidence.High);

			if (method.HasParameters) {
				foreach (ParameterDefinition parameter in method.Parameters) {
					if (!CheckName (parameter.Name, false))
						Runner.Report (parameter, Severity.Medium, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

