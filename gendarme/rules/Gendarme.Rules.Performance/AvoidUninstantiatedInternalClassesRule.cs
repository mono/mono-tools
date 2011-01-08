//
// Gendarme.Rules.Performance.AvoidUninstantiatedInternalClassesRule
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
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule will fire if a type is only visible within its assembly, can be instantiated, but 
	/// is not instantiated. Such types are often leftover (dead code) or are debugging/testing
	/// code and not required. However in some case the types might by needed, e.g. when 
	/// accessed thru reflection or if the <c>[InternalsVisibleTo]</c> attribute is used on the
	/// assembly.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // defined, but never instantiated
	/// internal class MyInternalClass {
	///	// ...
	/// } 
	/// 
	/// public class MyClass {
	///	static void Main ()
	///	{
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// internal class MyInternalClass {
	///	// ...
	/// } 
	/// 
	/// public class MyClass {
	///	static void Main ()
	///	{
	///		MyInternalClass c = new MyInternalClass ();
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The internal type is not instantiated by code within the assembly.")]
	[Solution ("Remove the type or add the code that uses it. If the type contains only static methods then either add the static modifier to the type or add the private construtor to the type to prevent the compiler from emitting a default public instance constructor.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	public class AvoidUninstantiatedInternalClassesRule : Rule, ITypeRule {

		// we use this to cache the information about the assembly
		// i.e. all types instantiated by the assembly
		private Dictionary<AssemblyDefinition, HashSet<TypeReference>> cache = new Dictionary<AssemblyDefinition, HashSet<TypeReference>> ();

		void CacheInstantiationFromAssembly (AssemblyDefinition assembly)
		{
			if (cache.ContainsKey (assembly))
				return;

			var typeset = new HashSet<TypeReference> ();

			foreach (ModuleDefinition module in assembly.Modules)
				foreach (TypeDefinition type in module.Types)
					ProcessType (type, typeset);

			cache.Add (assembly, typeset);
		}

		static void AddType (HashSet<TypeReference> typeset, TypeReference type)
		{
			// we're interested in the array element type, not the array itself
			if (type.IsArray)
				type = type.GetElementType ();

			// only keep stuff from this assembly, which means we have a TypeDefinition (not a TypeReference)
			// and types that are not visible outside the assembly (since this is what we check for)
			TypeDefinition td = (type as TypeDefinition);
			if ((td != null) && !td.IsVisible ())
				typeset.Add (type);
		}

		static void ProcessType (TypeDefinition type, HashSet<TypeReference> typeset)
		{
			if (type.HasFields) {
				foreach (FieldDefinition field in type.Fields) {
					TypeReference t = field.FieldType;
					// don't add the type itself (e.g. enums)
					if (type != t)
						AddType (typeset, t);
				}
			}
			if (type.HasMethods) {
				foreach (MethodDefinition method in type.Methods)
					ProcessMethod (method, typeset);
			}
			if (type.HasNestedTypes) {
				foreach (TypeDefinition nested in type.NestedTypes)
					ProcessType (nested, typeset);
			}
		}

		static void ProcessMethod (MethodDefinition method, HashSet<TypeReference> typeset)
		{
			// this is needed in case we return an enum, a struct or something mapped
			// to p/invoke (i.e. no ctor called). We also need to check for arrays.
			TypeReference t = method.ReturnType;
			AddType (typeset, t);

			if (method.HasParameters) {
				// an "out" from a p/invoke must be flagged
				foreach (ParameterDefinition parameter in method.Parameters) {
					// we don't want the reference (&) on the type
					t = parameter.ParameterType.GetElementType ();
					AddType (typeset, t);
				}
			}

			if (!method.HasBody)
				return;

			MethodBody body = method.Body;
			if (body.HasVariables) {
				// add every type of variables we use
				foreach (VariableDefinition variable in body.Variables) {
					t = variable.VariableType;
					AddType (typeset, t);
				}
			}

			// add every type we create or refer to (e.g. loading fields from an enum)
			foreach (Instruction ins in body.Instructions) {
				if (ins.Operand == null)
					continue;

				t = ins.Operand as TypeReference;
				if (t == null) {
					MethodReference m = ins.Operand as MethodReference;
					if (m != null) {
						t = m.DeclaringType;
						GenericInstanceType generic = (t as GenericInstanceType);
						if (generic != null)
							t = generic.GetElementType ();
					} else {
						FieldReference f = ins.Operand as FieldReference;
						if (f != null)
							t = f.DeclaringType;
					}
				}

				if (t != null)
					AddType (typeset, t);
			}
		}

		static bool HasSinglePrivateConstructor (TypeDefinition type)
		{
			if (!type.HasMethods)
				return false;

			MethodDefinition constructor = null;
			foreach (MethodDefinition method in type.Methods) {
				if (!method.IsConstructor)
					continue;
				if (constructor != null)
					return false; // more than one ctor
				constructor = method;
			}

			if (constructor == null)
				return false;

			return (constructor.IsPrivate && !constructor.HasParameters);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply to internal (non-visible) types
			// note: IsAbstract also excludes static types (2.0)
			if (type.IsVisible () || type.IsAbstract || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// people use this pattern to have a static class in C# 1.
			if (type.IsSealed && HasSinglePrivateConstructor (type))
				return RuleResult.DoesNotApply;

			// used for documentation purpose by monodoc
			if (type.Name == "NamespaceDoc")
				return RuleResult.DoesNotApply;

			// rule applies

			// if the type holds the Main entry point then it is considered useful
			AssemblyDefinition assembly = type.Module.Assembly;
			MethodDefinition entry_point = assembly.EntryPoint;
			if ((entry_point != null) && (entry_point.DeclaringType == type))
				return RuleResult.Success;

			// create a cache of all type instantiation inside this
			CacheInstantiationFromAssembly (assembly);

			HashSet<TypeReference> typeset = null;
			if (cache.ContainsKey (assembly))
				typeset = cache [assembly];

			// if we can't find the non-public type being used in the assembly then the rule fails
			if (typeset == null || !typeset.Contains (type)) {
				// base confidence on whether the internals are visible or not
				Confidence c = assembly.HasAttribute ("System.Runtime.CompilerServices.InternalsVisibleToAttribute") ? 
					Confidence.Low : Confidence.Normal;
				Runner.Report (type, Severity.High, c);
				return RuleResult.Failure;
			}

			return RuleResult.Success;
		}
	}
}
