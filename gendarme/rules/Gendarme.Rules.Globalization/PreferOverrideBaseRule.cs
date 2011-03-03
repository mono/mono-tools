//
// Gendarme.Rules.Globalization.PreferOverrideBaseRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Globalization {

	[EngineDependency (typeof (OpCodeEngine))]
	public abstract class PreferOverrideBaseRule : Rule, IMethodRule {

		protected virtual bool CheckFirstParameter {
			get { return false; }
		}

		protected abstract bool IsPrefered (TypeReference type);

		protected virtual bool IsSpecialCase (MethodReference method)
		{
			return (method == null);
		}

		protected abstract void Report (MethodDefinition method, Instruction instruction, MethodReference prefered);

		static bool MatchParameters (Collection<ParameterDefinition> pmethod, Collection<ParameterDefinition> candidate, int offset)
		{
			int ccount = candidate.Count - offset;
			int count = Math.Min (pmethod.Count, ccount - offset);
			for (int i = 0; i < count; i++) {
				ParameterDefinition pd = candidate [i + offset];
				if (pd.IsParams ())
					return true;
				TypeReference ptype = pd.ParameterType;
				if (!pmethod [i].ParameterType.IsNamed (ptype.Namespace, ptype.Name))
					return false;
			}
			return (ccount - count <= 1);
		}

		// look for a signature identical to ours but that accept an extra parameter
		MethodReference LookForPreferredOverride (MethodReference method)
		{
			TypeDefinition type = method.DeclaringType.Resolve ();
			if (type == null)
				return null;

			var methods = type.Methods;
			// we already know that, if resolved, there's at least one method (the caller)
			// so there's no need to call HasMethods
			if (methods.Count == 1)
				return null;

			string name = method.Name;
			int pcount = 0;
			Collection<ParameterDefinition> mparams = null;
			if (method.HasParameters) {
				mparams = method.Parameters;
				pcount = mparams.Count;
			}

			foreach (MethodDefinition md in methods) {
				// has one more parameter, so non-zero
				if (!md.HasParameters)
					continue;

				Collection<ParameterDefinition> pdc = md.Parameters;
				if (name != md.Name)
					continue;

				// compare parameters and return value
				TypeReference rtype = md.ReturnType;
				if (!method.ReturnType.IsNamed (rtype.Namespace, rtype.Name))
					continue;

				// last parameter could be our "prefered" type
				if (IsPrefered (pdc [pdc.Count - 1].ParameterType)) {
					// special case where the method has no parameter, override has only one (the prefered)
					if ((pcount == 0) && (mparams == null))
						return md;
					if (MatchParameters (mparams, pdc, 0))
						return md;
				} else if (CheckFirstParameter && IsPrefered (pdc [0].ParameterType)) {
					if (MatchParameters (mparams, pdc, 1))
						return md;
				}
			}
			return null;
		}

		Dictionary<MethodReference, MethodReference> prefered_overloads = new Dictionary<MethodReference, MethodReference> ();

		MethodReference GetPreferedOverride (MethodReference method)
		{
			MethodReference prefered = null;
			if (!prefered_overloads.TryGetValue (method, out prefered)) {
				prefered = LookForPreferredOverride (method);
				prefered_overloads.Add (method, prefered);
			}
			return prefered;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// exclude methods that don't have calls
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = ins.GetMethod ();
				// some inheritors have special cases to deal with
				if (IsSpecialCase (mr))
					continue;

				// check if the call starts or ends with our 'prefered' override
				if (mr.HasParameters) {
					Collection<ParameterDefinition> pdc = mr.Parameters;
					if (CheckFirstParameter && IsPrefered (pdc [0].ParameterType))
						continue;
					if (IsPrefered (pdc [pdc.Count - 1].ParameterType))
						continue;
				}

				// if not check if such a 'prefered' override exists to replace the called method
				MethodReference prefered = GetPreferedOverride (mr);
				if (prefered != null)
					Report (method, ins, prefered);
			}

			return Runner.CurrentRuleResult;
		}
	}
}

