//
// Gendarme.Rules.Maintainability.AvoidUnnecessarySpecializationRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
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
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Maintainability {

	[Problem ("This method has a parameter whose type is more specialized than necessary. It can be harder to reuse and/or extend the method in derived types.")]
	[Solution ("Replace parameter type with the least specialized type necessary, or make use of the specifics of the actual parameter type.")]
	public class AvoidUnnecessarySpecializationRule : Rule, IMethodRule {

		private StackEntryAnalysis sea;
		private TypeReference[] types_least;
		private int[] depths_least;

		private static TypeReference GetActualType (TypeReference type)
		{
			GenericParameter gp = (type as GenericParameter);
			if ((gp != null) && (gp.Constraints.Count == 1))
				type = gp.Constraints [0];
			return type;
		}

		private static int GetActualTypeDepth (TypeReference type)
		{
			return GetTypeDepth (GetActualType (type));
		}

		private static int GetTypeDepth (TypeReference type)
		{
			return GetTypeDepth (GetActualType (type).Resolve ());
		}

		private static int GetTypeDepth (TypeDefinition type)
		{
			int depth = 0;
			while (null != type && type.BaseType != null) {
				depth++;
				type = type.BaseType.Resolve ();
			}
			return depth;
		}

		private static TypeDefinition GetBaseImplementor (TypeReference type, MethodSignature signature)
		{
			return GetBaseImplementor (type.Resolve(), signature);
		}

		private static TypeDefinition GetBaseImplementor (TypeDefinition type, MethodSignature signature)
		{
			TypeDefinition implementor = type;

			while (null != type && type.IsVisible ()) {
				//search for matching interface
				TypeDefinition ifaceDef = GetInterfaceImplementor (type, signature);
				if (null != ifaceDef)
					implementor = ifaceDef;
				else if (null != type.GetMethod (signature))
					implementor = type;
				type = type.BaseType.Resolve ();
			}

			return implementor;
		}

		private static TypeDefinition GetInterfaceImplementor (TypeDefinition type, MethodSignature signature)
		{
			TypeDefinition ifaceDef = null;

			foreach (TypeReference iface in type.Interfaces) {
				TypeDefinition candidate = iface.Resolve ();

				if (!candidate.IsVisible () || candidate.Name.StartsWith("_"))
					continue; //ignore non-cls-compliant interfaces
				if (null == candidate.GetMethod (signature))
					continue; //does not implement

				if (null == ifaceDef) {
					ifaceDef = candidate;
				} else if (ifaceDef.GenericParameters.Count < candidate.GenericParameters.Count) {
					//prefer the most generic interface
					ifaceDef = candidate;
				} else if (ifaceDef.Interfaces.Count < candidate.Interfaces.Count) {
					//prefer the most specific interface
					ifaceDef = candidate;
				} else if (ifaceDef.Interfaces.Count >= candidate.Interfaces.Count
					|| ifaceDef.GenericParameters.Count >= candidate.GenericParameters.Count) {
					continue; //we already have a better match
				} else {
					//ambiguous/explicit, ignore
					ifaceDef = null;
					break;
				}
			}

			return ifaceDef;
		}

		private static MethodSignature GetSignature (MethodReference method)
		{
			string[] parameters = new string [method.Parameters.Count];
			for (int i = 0; i < method.Parameters.Count; ++i) {
				TypeReference pType = method.Parameters [i].ParameterType;
				if (pType is GenericParameter)
					parameters [i] = null; //TODO: constructed mapping?
				else
					parameters [i] = pType.FullName;
			}
			return new MethodSignature (method.Name, GetReturnTypeSignature (method), parameters);
		}

		private static string GetReturnTypeSignature (MethodReference method)
		{
			if (method.Name == "GetEnumerator" && 0 == method.Parameters.Count) //special exception
				return null;
			return method.ReturnType.ReturnType.FullName;
		}

		private static bool IsSystemObjectMethod (MethodReference method)
		{
			switch (method.Name) {
			case "Finalize" :
			case "GetHashCode" :
			case "GetType" :
			case "MemberwiseClone" :
			case "ToString" :
				return method.Parameters.Count == 0;
			case "Equals" :
				return method.Parameters.Count == 1 || method.Parameters.Count == 2;
			case "ReferenceEquals" :
				return method.Parameters.Count == 2;

			//HACK: BOO:
			case "EqualityOperator" :
				return method.Parameters.Count == 2 && method.DeclaringType.FullName == "Boo.Lang.Runtime.RuntimeServices";
			}
			return false;
		}

		private void UpdateParameterLeastType (ParameterReference parameter, StackEntryUsageResult [] usageResults)
		{
			int pIndex = parameter.Sequence - 1;
			if (pIndex < 0) throw new InvalidOperationException("parameter.Sequence < 1");
			int parameterDepth = GetActualTypeDepth (parameter.ParameterType);

			TypeReference currentLeastType = null;
			int currentLeastDepth = 0;

			foreach (var usage in usageResults) {
				bool needUpdate = false;

				switch (usage.Instruction.OpCode.Code) {
				case Code.Newobj :
				case Code.Call :
				case Code.Callvirt :
					MethodReference method = (MethodReference) usage.Instruction.Operand;

					//potential generalization to object does not really make sense
					//from a readability/maintainability point of view
					if (IsSystemObjectMethod (method)) continue;

					MethodSignature signature = GetSignature (method);

					if (usage.StackOffset == method.Parameters.Count) {
						//argument is used as `this` in the call
						currentLeastType = GetBaseImplementor (GetActualType (method.DeclaringType), signature);
					} else {
						//argument is also used as an argument in the call
						currentLeastType = method.Parameters [method.Parameters.Count - usage.StackOffset - 1].ParameterType;
					}

					//if the best we could find is object, ignore this round
					if ((currentLeastType == null) || (currentLeastType.FullName == "System.Object"))
						continue;

					needUpdate = true;
					break;

				case Code.Stfld :
				case Code.Stsfld :
					FieldReference field = (FieldReference) usage.Instruction.Operand;
					currentLeastType = field.FieldType;

					needUpdate = true;
					break;
				}

				if (needUpdate) {
					currentLeastDepth = GetActualTypeDepth (currentLeastType);
					if (null == types_least [pIndex] || currentLeastDepth > depths_least [pIndex]) {
						types_least [pIndex] = currentLeastType;
						depths_least [pIndex] = currentLeastDepth;
					}
					if (currentLeastDepth == parameterDepth) //no need to check further
						return;
				}
			}
		}

		private void CheckParameter (MethodDefinition method, Instruction ins)
		{
			ParameterDefinition parameter = ins.GetParameter (method);
			if (null == parameter) //this is `this`, we do not care
				return;
			if (parameter.IsOut || parameter.IsOptional || parameter.ParameterType.IsValueType)
				return;
			if (parameter.ParameterType.IsArray () || parameter.ParameterType.IsDelegate ())
				return; //TODO: these are more complex to handle, not supported for now

			if (null == sea || sea.Method != method)
				sea = new StackEntryAnalysis (method);

			StackEntryUsageResult [] usage = sea.GetStackEntryUsage (ins);

			//FIXME: below opt. would require a non-repeating SEA with multiple ldarg
			//if (null != leastTypes [parameter.Sequence-1]) //already analyzed, next!
			//	return;

			UpdateParameterLeastType (parameter, usage);
		}

		static bool SignatureDictatedByInterface (MethodReference method)
		{
			TypeDefinition type = (method.DeclaringType as TypeDefinition);
			if (type.Interfaces.Count > 0) {
				MethodSignature sig = GetSignature (method);
				foreach (TypeReference intf_ref in type.Interfaces) {
					TypeDefinition intr = intf_ref.Resolve ();
					if (intr == null)
						continue;
					foreach (MethodDefinition md in intr.Methods) {
						if (sig.Matches (md))
							return true;
					}
				}
			}
			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode () || method.IsCompilerControlled)
				return RuleResult.DoesNotApply;
			if (method.Parameters.Count == 0 || method.IsProperty ())
				return RuleResult.DoesNotApply;

			// we can't change parameter types if they were specified by an interface
			if (SignatureDictatedByInterface (method))
				return RuleResult.DoesNotApply;

			types_least = new TypeReference [method.Parameters.Count];
			depths_least = new int [types_least.Length];

			//look at each argument usage
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.IsLoadArgument ())
					CheckParameter (method, ins);
			}

			CheckParametersSpecializationDelta (method);

			return Runner.CurrentRuleResult;
		}

		private void CheckParametersSpecializationDelta (MethodDefinition method)
		{
			for (int i = 0; i < method.Parameters.Count; ++i) {
				if (null == types_least [i]) continue; //argument is not used

				ParameterDefinition parameter = method.Parameters [i];
				int delta = GetActualTypeDepth (parameter.ParameterType) - depths_least [i];

				if (delta > 0) {
					string message = GetSuggestionMessage (parameter);
					Severity sev = (delta < 3) ? Severity.Medium : Severity.High;
					Runner.Report (method, sev, Confidence.High, message);
				}
			}
		}

		private string GetSuggestionMessage (ParameterReference parameter)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Parameter '");
			sb.Append (parameter.Name);
			if (parameter.ParameterType is GenericParameter)
				sb.Append ("' could be constrained to type '");
			else
				sb.Append ("' could be of type '");
			sb.Append (types_least [parameter.Sequence - 1].FullName);
			sb.Append ("'.");
			return sb.ToString ();
		}

	}

}

