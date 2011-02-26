// 
// Gendarme.Rules.Design.ListsAreStronglyTypedRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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

namespace Gendarme.Rules.Design {

	abstract public class StronglyTypedRule : Rule, ITypeRule {

		abstract protected MethodSignature [] GetMethods ();
		abstract protected string [] GetProperties ();
		abstract protected string InterfaceName { get; }
		abstract protected string InterfaceNamespace { get; }

		MethodSignature [] signatures;
		string [] propertyNames;
		int methodsLeft, propertiesLeft;

		virtual public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsAbstract || type.IsGeneratedCode () || !type.Implements (InterfaceNamespace, InterfaceName))
				return RuleResult.DoesNotApply;

			signatures = GetMethods ();
			propertyNames = GetProperties ();

			methodsLeft = signatures.Length;
			propertiesLeft = propertyNames.Length;

			TypeDefinition baseType = type;
			while (methodsLeft > 0 || propertiesLeft > 0) {
				ProcessType (baseType);
				if (baseType.BaseType == null)
					break;
				TypeDefinition td = baseType.BaseType.Resolve ();
				if (td == null)
					break;
				baseType = td;

			}

			if (propertiesLeft > 0) {
				foreach (string propertyName in propertyNames) {
					if (propertyName == null)
						continue;
					Runner.Report (type, Severity.Medium, Confidence.High,
						"Type does not have strongly typed version of property " + propertyName);
				}
			}

			if (methodsLeft > 0) {
				foreach (MethodSignature signature in signatures) {
					if (signature == null)
						continue;
					Runner.Report (type, Severity.Medium, Confidence.High,
						"Type does not have strongly typed version of method " + signature.Name);
				}
			}

			return Runner.CurrentRuleResult;
		}

		private void ProcessType (TypeDefinition baseType)
		{
			if (baseType.HasMethods && methodsLeft > 0)
				ProcessMethods (baseType);

			if (baseType.HasProperties && propertiesLeft > 0)
				ProcessProperties (baseType);
		}

		static bool IsWeak (TypeReference type)
		{
			if (type.Namespace != "System")
				return false;
			string name = type.Name;
			return ((name == "Object") || (name == "Array") || (name == "Object[]"));
		}

		static bool IsWeak (string typeName)
		{
			return ((typeName == "System.Object") || (typeName == "System.Array") || (typeName == "System.Object[]"));
		}

		private void ProcessProperties (TypeDefinition baseType)
		{
			foreach (PropertyDefinition property in baseType.Properties) {
				for (int i = 0; i < propertyNames.Length; i++) {
					if (propertyNames [i] == null || propertyNames [i] != property.Name)
						continue;
					if (!IsWeak (property.PropertyType)) {
						propertiesLeft--;
						propertyNames [i] = null;
					}
				}
			}
		}

		private void ProcessMethods (TypeDefinition baseType)
		{
			foreach (MethodDefinition method in baseType.Methods) {
				if (!method.HasParameters || method.IsProperty ())
					continue;
				for (int i = 0; i < signatures.Length; i++) {
					var methodParameters = method.Parameters;
					if (signatures [i] == null || method.Name != signatures [i].Name ||
						methodParameters.Count != signatures [i].Parameters.Count)
						continue;

					bool foundStrong = true;
					for (int j = 0; j < methodParameters.Count; j++) {
						if (!IsWeak (signatures [i].Parameters [j]))
							continue;
						if (IsWeak (methodParameters [j].ParameterType))
							foundStrong = false;
					}

					if (foundStrong) {
						methodsLeft--;
						// null means strongly typed version of this signature was found
						signatures [i] = null;
					}
				}
			}
		}
	}
}
