//
// Gendarme.Rules.Globalization.AvoidUnusedInternalResourceRule
//
// Authors:
//	Antoine Vandecreme <ant.vand@gmail.com>
//
// Copyright (C) 2011 Antoine Vandecreme
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
using System.Linq;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Globalization {

	/// <summary>
	/// This rule will check for internally visible resources (resx) which are never called.
	/// You should remove unused internal resources to avoid useless translations.
	/// </summary>
	[Problem ("This internal (assembly-level) resource (resx) does not have callers in the assembly.")]
	[Solution ("Remove the unused resource or add code to call it.")]
	public class AvoidUnusedInternalResourceRule : Rule, IMethodRule {

		static private bool Applicable (MethodDefinition method)
		{
			// only internal resources
			if (!method.IsAssembly)
				return false;

			// resources are static getters
			if (!method.IsStatic || !method.IsGetter)
				return false;

			// Ignore well known static getters of resources classes
			string name = method.Name;
			if ("get_Culture".Equals (name, StringComparison.InvariantCulture) ||
				"get_ResourceManager".Equals (name, StringComparison.InvariantCulture))
				return false;

			// rule apply only to static getters in a generated resx class
			TypeDefinition typeDefinition = method.DeclaringType;
			if (!typeDefinition.HasCustomAttributes)
				return false;

			if (typeDefinition.HasAttribute ("System.CodeDom.Compiler", "GeneratedCodeAttribute"))
				return true;
			if (typeDefinition.HasAttribute ("System.Diagnostics", "DebuggerNonUserCodeAttribute"))
				return true;
			if (typeDefinition.HasAttribute ("System.Runtime.CompilerServices", "CompilerGeneratedAttribute"))
				return true;

			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// check if the the rule applies to this method
			if (!Applicable (method))
				return RuleResult.DoesNotApply;

			if (CheckAssemblyForMethodUsage (method))
				return RuleResult.Success;

			// resource is unused and unneeded
			Runner.Report (method, Severity.Medium, Confidence.Normal, "The resource is not visible outside its declaring assembly, nor used within.");
			return RuleResult.Failure;
		}

		#region FIXME (following code is a copy of AvoidUncalledPrivateCodeRule)

		public override void TearDown ()
		{
			// reusing the cache (e.g. the wizard) is not a good thing if an exception
			// occured while building it (future analysis results would be bad)
			cache.Clear ();
			base.TearDown ();
		}

		private static bool CheckAssemblyForMethodUsage (MethodReference method)
		{
			// scan each module in the assembly that defines the method
			AssemblyDefinition assembly = method.DeclaringType.Module.Assembly;
			foreach (ModuleDefinition module in assembly.Modules) {
				// scan each type
				foreach (TypeDefinition type in module.GetAllTypes ()) {
					if (CheckTypeForMethodUsage (type, method))
						return true;
				}
			}
			return false;
		}

		static Dictionary<TypeDefinition, HashSet<ulong>> cache = new Dictionary<TypeDefinition, HashSet<ulong>> ();

		private static ulong GetToken (MethodReference method)
		{
			return (ulong) method.DeclaringType.Module.Assembly.GetHashCode () << 32 | method.GetElementMethod ().MetadataToken.ToUInt32 ();
		}

		private static bool CheckTypeForMethodUsage (TypeDefinition type, MethodReference method)
		{
			if (type.HasGenericParameters)
				type = type.GetElementType ().Resolve ();

			HashSet<ulong> methods = GetCache (type);
			if (methods.Contains (GetToken (method)))
				return true;

			MethodDefinition md = method.Resolve ();
			if ((md != null) && md.HasOverrides) {
				foreach (MethodReference mr in md.Overrides) {
					if (methods.Contains (GetToken (mr)))
						return true;
				}
			}
			return false;
		}

		private static HashSet<ulong> GetCache (TypeDefinition type)
		{
			HashSet<ulong> methods;
			if (!cache.TryGetValue (type, out methods)) {
				methods = new HashSet<ulong> ();
				cache.Add (type, methods);
				if (type.HasMethods) {
					foreach (MethodDefinition md in type.Methods) {
						if (!md.HasBody)
							continue;
						BuildMethodUsage (methods, md);
					}
				}
			}
			return methods;
		}

		private static void BuildMethodUsage (HashSet<ulong> methods, MethodDefinition method)
		{
			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = (ins.Operand as MethodReference);
				if (mr == null)
					continue;

				TypeReference type = mr.DeclaringType;
				if (!type.IsArray) {
					// if (type.GetElementType ().HasGenericParameters)
					// the simpler ^^^ does not work under Mono but works on MS
					type = type.Resolve ();
					if (type != null && type.HasGenericParameters) {
						methods.Add (GetToken (type.GetMethod (mr.Name)));
					}
				}
				methods.Add (GetToken (mr));
			}
		}

		#endregion
	}
}

