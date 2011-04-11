//
// Gendarme.Rules.Correctness.TypesShouldBeDisposableBaseRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
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

using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	public abstract class TypesShouldBeDisposableBaseRule : Rule, ITypeRule {

		protected TypesShouldBeDisposableBaseRule ()
		{
			FieldCandidates = new HashSet<FieldDefinition> ();
		}

		protected HashSet<FieldDefinition> FieldCandidates { get; private set; }

		protected abstract string AbstractTypeMessage { get; }
		protected abstract string TypeMessage { get; }
		protected abstract string AbstractDisposeMessage { get; }

		static bool IsAbstract (MethodDefinition method)
		{
			return ((method != null) && (method.IsAbstract));
		}

		protected abstract void CheckMethod (MethodDefinition method, bool abstractWarning);

		protected abstract bool FieldTypeIsCandidate (TypeDefinition type);

		public RuleResult CheckType (TypeDefinition type)
		{
			// that will cover interfaces, delegates too
			if (!type.HasFields)
				return RuleResult.DoesNotApply;

			// rule doesn't apply to enums, interfaces, structs, delegates or generated code
			if (type.IsEnum || type.IsValueType || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition explicitDisposeMethod = null;
			MethodDefinition implicitDisposeMethod = null;

			bool abstractWarning = false;

			if (type.Implements ("System", "IDisposable")) {
				implicitDisposeMethod = type.GetMethod (MethodSignatures.Dispose);
				explicitDisposeMethod = type.GetMethod (MethodSignatures.DisposeExplicit);

				if (IsAbstract (implicitDisposeMethod) || IsAbstract (explicitDisposeMethod)) {
					abstractWarning = true;
				} else {
					return RuleResult.Success;
				}
			}

			FieldCandidates.Clear ();

			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				TypeDefinition fieldType = field.FieldType.GetElementType ().Resolve ();
				if (fieldType == null)
					continue;
				if (FieldTypeIsCandidate (fieldType))
					FieldCandidates.Add (field);
			}

			// if there are fields types that implements IDisposable
			if (type.HasMethods && (FieldCandidates.Count > 0)) {
				// check if we're assigning new object to them
				foreach (MethodDefinition method in type.Methods) {
					CheckMethod (method, abstractWarning);
				}
			}

			// Warn about possible confusion if the Dispose methods are abstract
			if (IsAbstract (implicitDisposeMethod))
				Runner.Report (implicitDisposeMethod, Severity.Medium, Confidence.High, AbstractDisposeMessage);

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Stfld);
			mask.Set (Code.Stelem_Ref);
			Console.WriteLine (mask);
		}
#endif
	}
}

