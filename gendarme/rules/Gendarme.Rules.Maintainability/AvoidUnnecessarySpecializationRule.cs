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

using System.Collections.Generic;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks methods for over specialized parameters - i.e. parameter types
	/// that are unnecessarily specialized with respect to what the method needs to do
	/// its job. This often leads to reduced reusability potential of the method. 
	/// The rule will suggest the minimal type, or interface, required for the method to
	/// work.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class DefaultEqualityComparer : IEqualityComparer {
	/// 	public int GetHashCode (object obj)
	/// 	{
	/// 		return o.GetHashCode ();
	/// 	}
	/// }
	/// 
	/// public int Bad (DefaultEqualityComparer ec, object o)
	/// {
	///	return ec.GetHashCode (o);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class DefaultEqualityComparer : IEqualityComparer {
	/// 	public int GetHashCode (object obj)
	/// 	{
	/// 		return o.GetHashCode ();
	/// 	}
	/// }
	/// 
	/// public int Good (IEqualityComparer ec, object o)
	/// {
	///	return ec.GetHashCode (o);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method has a parameter whose type is more specialized than necessary. It can be harder to reuse and/or extend the method in derived types.")]
	[Solution ("Replace parameter type with the least specialized type necessary, or make use of the specifics of the actual parameter type.")]
	public class AvoidUnnecessarySpecializationRule : Rule, IMethodRule {

		private StackEntryAnalysis sea;
		private TypeReference [] types_least = new TypeReference [64];
		private int [] depths_least = new int [64];

		// FIXME: handle more than one constraint
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
			TypeDefinition typeDef = GetActualType (type).Resolve ();
			//if we cannot resolve then return depth as 1 by default as it will
			//be eventually be sorted out by signature matching.
			return (null == typeDef) ? 1 : GetTypeDepth (typeDef);
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

		private static bool DoesAllSignaturesMatchType (TypeReference type, IEnumerable<MethodSignature> signatures)
		{
			bool match = true;
			foreach (MethodSignature signature in signatures) {
				match &= (null != type.GetMethod (signature));
			}
			return match;
		}

		private static TypeDefinition GetBaseImplementor (TypeReference type, IEnumerable<MethodSignature> signatures)
		{
			return GetBaseImplementor (type.Resolve (), signatures);
		}

		private static TypeDefinition GetBaseImplementor (TypeDefinition type, IEnumerable<MethodSignature> signatures)
		{
			TypeDefinition implementor = type;

			while (null != type && type.IsVisible ()) {
				//search for matching interface
				TypeDefinition ifaceDef = GetInterfaceImplementor (type, signatures);
				if (null != ifaceDef)
					implementor = ifaceDef;
				else if (DoesAllSignaturesMatchType (type, signatures))
					implementor = type;

				type = type.BaseType.Resolve ();
			}

			return implementor;
		}

		private static TypeDefinition GetInterfaceImplementor (TypeDefinition type, IEnumerable<MethodSignature> signatures)
		{
			TypeDefinition ifaceDef = null;

			foreach (TypeReference iface in type.Interfaces) {
				// ignore non-cls-compliant interfaces
				if (iface.Name.StartsWith ("_", StringComparison.Ordinal))
					continue;

				TypeDefinition candidate = iface.Resolve ();
				if ((candidate == null) || !candidate.IsVisible ())
					continue; 

				if (!DoesAllSignaturesMatchType (candidate, signatures))
					continue;

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
			string [] parameters = new string [method.Parameters.Count];
			for (int i = 0; i < method.Parameters.Count; ++i) {
				TypeReference pType = method.Parameters [i].ParameterType;

				// handle reference type (ref in C#)
				ReferenceType ref_type = (pType as ReferenceType);
				if (ref_type != null)
					pType = ref_type.ElementType;

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
			if (method.Name.Length < 6 /*Equals*/ || method.Name.Length > 16 /*EqualityOperator*/)
				return false; //no need to do the string comparisons

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

		private static bool IsFromNonGenericCollectionNamespace (string nameSpace)
		{
			return ((nameSpace == "System.Collections") || (nameSpace == "System.Collections.Specialized"));
		}

		private static bool IsIgnoredSuggestionType (TypeReference type)
		{
			return ((type.FullName == "System.Object") || IsFromNonGenericCollectionNamespace (type.Namespace));
		}

		private void UpdateParameterLeastType (ParameterReference parameter, IEnumerable<StackEntryUsageResult> usageResults)
		{
			int pIndex = parameter.Sequence - 1;
			if (pIndex < 0) throw new InvalidOperationException ("parameter.Sequence < 1");
			int parameterDepth = GetActualTypeDepth (parameter.ParameterType);

			int currentLeastDepth = 0;
			TypeReference currentLeastType = null;
			List<MethodSignature> signatures = new List<MethodSignature> ();

			//accumulate all used signatures first
			foreach (var usage in usageResults) {

				switch (usage.Instruction.OpCode.Code) {
				case Code.Newobj :
				case Code.Call :
				case Code.Callvirt :
					MethodReference method = (MethodReference) usage.Instruction.Operand;
					if (IsSystemObjectMethod (method) || IsFromNonGenericCollectionNamespace (method.DeclaringType.Namespace))
						continue;
					signatures.Add (GetSignature (method));
					break;
				}
			}

			//update the result array as in if (needUpdate) block below
			foreach (var usage in usageResults) {
				bool needUpdate = false;

				switch (usage.Instruction.OpCode.Code) {
				case Code.Newobj :
				case Code.Call :
				case Code.Callvirt :
					MethodReference method = (MethodReference) usage.Instruction.Operand;

					//potential generalization to object does not really make sense
					//from a readability/maintainability point of view
					if (IsSystemObjectMethod (method))
						continue;
					//we cannot really know if suggestion would work since the collection
					//is non-generic thus we ignore it
					if (IsFromNonGenericCollectionNamespace (method.DeclaringType.Namespace))
						continue;

					if (usage.StackOffset == method.Parameters.Count) {
						//argument is used as `this` in the call
						currentLeastType = GetBaseImplementor (GetActualType (method.DeclaringType), signatures);
					} else {
						//argument is also used as an argument in the call
						currentLeastType = method.Parameters [method.Parameters.Count - usage.StackOffset - 1].ParameterType;

						//if parameter type is a generic, find the 'real' constructed type
						GenericParameter gp = (currentLeastType as GenericParameter);
						if (gp != null)
							currentLeastType = GetConstructedGenericType (method, gp);
					}

					//if the best we could find is object or non-generic collection, ignore this round
					if (currentLeastType == null || IsIgnoredSuggestionType (currentLeastType))
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

		private void CheckParameter (MethodDefinition method, IEnumerable<Instruction> ldarg)
		{
			Dictionary<ParameterDefinition, List<StackEntryUsageResult>> usages = new Dictionary<ParameterDefinition, List<StackEntryUsageResult>> ();

			foreach (Instruction ins in ldarg) {

				ParameterDefinition parameter = ins.GetParameter (method);
				if (null == parameter) //this is `this`, we do not care
					continue;
				if (parameter.IsOut || parameter.IsOptional || parameter.ParameterType.IsValueType)
					continue;
				if (parameter.ParameterType.IsArray () || parameter.ParameterType.IsDelegate ())
					continue; //TODO: these are more complex to handle, not supported for now

				if (null == sea || sea.Method != method)
					sea = new StackEntryAnalysis (method);

				if (!usages.ContainsKey (parameter))
					usages [parameter] = new List<StackEntryUsageResult> ();
				usages [parameter].AddRange (sea.GetStackEntryUsage (ins));
			}

			foreach (var usage in usages)
				UpdateParameterLeastType (usage.Key, usage.Value);
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

			if (method.Parameters.Count > types_least.Length) {
				// that should be quite rare (does not happen for mono 2.0 class libs)
				types_least = new TypeReference [method.Parameters.Count];
				depths_least = new int [method.Parameters.Count];
			}

			List<Instruction> instructions = new List<Instruction> ();
			//look at each argument usage
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.IsLoadArgument ())
					instructions.Add (ins);
			}

			CheckParameter (method, instructions);

			CheckParametersSpecializationDelta (method);

			Array.Clear (types_least, 0, types_least.Length);
			Array.Clear (depths_least, 0, depths_least.Length);

			return Runner.CurrentRuleResult;
		}

		private void CheckParametersSpecializationDelta (MethodReference method)
		{
			foreach (ParameterDefinition parameter in method.Parameters){

				int i = parameter.Sequence - 1;
				if (null == types_least [i])
					continue; //argument is not used

				// the rule currently does not handle more than one generic constraint
				// so we prefer skipping them than reporting a bunch of false positives
				GenericParameter gp = (parameter.ParameterType as GenericParameter);
				if (gp != null) {
					if (gp.Constraints.Count > 1)
						continue;
				}

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

			TypeReference type = types_least [parameter.Sequence - 1];
			AppendPrettyTypeName (sb, type);
			sb.Append ("'.");
			return sb.ToString ();
		}

		private static TypeReference GetConstructedGenericType (MethodReference method, GenericParameter parameter)
		{
			if (parameter.Owner is MethodReference)
				return ((GenericInstanceMethod) method).GenericArguments [parameter.Position];
			if (parameter.Owner is TypeReference)
				return ((GenericInstanceType) method.DeclaringType).GenericArguments [parameter.Position];
			return parameter.Owner.GenericParameters [parameter.Position];
		}

		private static void AppendPrettyTypeName (StringBuilder sb, TypeReference type)
		{
			int nRemoveTrail;
			if (type.GenericParameters.Count == 0)
				nRemoveTrail = 0;
			else if (type.GenericParameters.Count < 10)
				nRemoveTrail = 2;
			else
				nRemoveTrail = 3;

			sb.Append (type.FullName.Substring (0, type.FullName.Length - nRemoveTrail));
			if (type.GenericParameters.Count > 0) {
				int n = 0;
				sb.Append ("<");
				foreach (GenericParameter gp in type.GenericParameters) {
					if (n > 0)
						sb.Append (",");
					AppendPrettyTypeName (sb, gp);
					n++;
				}
				sb.Append (">");
			}
		}
	}
}
