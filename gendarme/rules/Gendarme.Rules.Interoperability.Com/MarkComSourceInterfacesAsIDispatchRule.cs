// 
// Gendarme.Rules.Interoperability.Com.MarkComSourceInterfacesAsIDispatchRule
//
// Authors:
//	Nicholas Rioux	
//
// Copyright (C) 2010 Nicholas Rioux
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// When a type is marked with the ComSourceInterfacesAttribute, every specified interface must
	/// be marked with a InterfaceTypeAttribute set to ComInterfaceType.InterfaceIsIDispatch.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// interface IBadInterface { }
	/// [ComSourceInterfaces("Project.IBadInterface")]
	/// class TestClass { }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	/// interface IGoodInterface { }
	/// [ComSourceInterfaces("Project.IGoodInterface")]
	/// class TestClass { }
	/// </code>
	/// </example>
	[Problem ("A type is marked with ComSourceInterfacesAttribute, but a specified interface is not marked with InterfaceTypeAttribute set to InterfaceIsIDispatch.")]
	[Solution ("Add an InterfaceTypeAttribute set to InterfaceIsIDispatch for all specified interfaces.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1412:MarkComSourceInterfacesAsIDispatch")]
	public class MarkComSourceInterfacesAsIDispatchRule : Rule, ITypeRule {
		private SortedDictionary<string, TypeDefinition> interfaces = new SortedDictionary<string, TypeDefinition> ();

		// Iterate through all assemblies and add the interfaces found to a list.
		private void FindInterfaces ()
		{
			foreach (AssemblyDefinition assembly in Runner.Assemblies) {
				foreach (ModuleDefinition module in assembly.Modules) {
					foreach (TypeDefinition type in module.GetAllTypes ()) {
						if (!type.IsInterface)
							continue;
						interfaces.Add (type.GetFullName (), type);
					}
				}
			}
		}

		// Finds a CustomAttribute on a type from the given name.
		private static CustomAttribute FindCustomAttribute (ICustomAttributeProvider type, string nameSpace, string name)
		{
			foreach (var attribute in type.CustomAttributes) {
				if (attribute.AttributeType.IsNamed (nameSpace, name))
					return attribute;
			}
			return null;
		}

		// Ensures the interface has a InterfaceTypeAttribute with 
		// ComInterfaceType.InterfaceIsIDispatch passed to it.
		private void CheckInterface (TypeDefinition def)
		{
			if (def == null)
				return;
			if (!def.HasCustomAttributes) {
				Runner.Report (def, Severity.High, Confidence.Total, "No attributes are present on a specified interface");
				return;
			}

			var attribute = FindCustomAttribute (def, "System.Runtime.InteropServices", "InterfaceTypeAttribute");
			if (attribute == null) {
				Runner.Report (def, Severity.High, Confidence.Total, "No [InterfaceType] attribute is present on a specified interface");
				return;
			}

			// default to bad value - anything not InterfaceIsIDispatch (2) will be reported
			// ToString covers both the ComInterfaceType (int) and short ctor variations
			if (attribute.ConstructorArguments [0].Value.ToString () != "2")
				Runner.Report (def, Severity.High, Confidence.Total, "The [InterfaceType] attribute is not set to InterfaceIsIDispatch");
		}

		private void CheckInterface (string interface_name)
		{
			TypeDefinition td;
			if (interfaces.TryGetValue (interface_name, out td))
				CheckInterface (td);
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			interfaces.Clear ();
			FindInterfaces ();
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || !type.HasCustomAttributes)
				return RuleResult.DoesNotApply;

			var attribute = FindCustomAttribute (type, "System.Runtime.InteropServices", "ComSourceInterfacesAttribute");
			if (attribute == null)
				return RuleResult.DoesNotApply;
			// The attribute's paramemters may be a single null-delimited string, or up to four System.Types.
			foreach (var arg in attribute.ConstructorArguments) {
				string string_value = arg.Value as string;
				if (string_value != null) {
					if (string_value.IndexOf ('\0') == -1)
						CheckInterface (string_value);
					else {
						foreach (var name in string_value.Split ('\0'))
							CheckInterface (name);
					}
				} else {
					TypeDefinition def_value = arg.Value as TypeDefinition;
					if (def_value != null)
						CheckInterface (def_value);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
