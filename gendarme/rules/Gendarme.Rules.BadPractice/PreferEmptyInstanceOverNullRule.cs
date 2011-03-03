//
// Gendarme.Rules.BadPractice.PreferEmptyInstanceOverNullRule
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks that all methods and properties which return a string, an array,
	/// a collection, or an enumerable do not return <c>null</c>.
	/// It is usually better to return an empty instance, as this allows
	/// the caller to use the result without having to perform a null-check first.
	/// </summary>
	/// <example>
	/// Bad example (string):
	/// <code>
	/// public string DisplayName {
	/// 	get {
	/// 		if (IsAnonymous) {
	/// 			return null;
	///		}
	/// 		return name;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (string):
	/// <code>
	/// public string DisplayName {
	/// 	get {
	/// 		if (IsAnonymous) {
	/// 			return string.Empty;
	///		}
	/// 		return name;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (array):
	/// <code>
	/// public int [] GetOffsets ()
	/// {
	/// 	if (!store.HasOffsets) {
	/// 		return null;
	///	}
	/// 	store.LoadOffsets ();
	/// 	return store.Offsets;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (array):
	/// <code>
	/// static const int [] Empty = new int [0];
	/// public int [] GetOffsets ()
	/// {
	/// 	if (!store.HasOffsets) {
	/// 		return Empty;
	///	}
	/// 	store.LoadOffsets ();
	/// 	return store.Offsets.ToArray ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (enumerable):
	/// <code>
	/// public IEnumerable&lt;int&gt; GetOffsets ()
	/// {
	/// 	if (!store.HasOffsets) {
	/// 		return null;
	///	}
	/// 	store.LoadOffsets ();
	/// 	return store.Offsets;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (enumerable):
	/// <code>
	/// public IEnumerable&lt;int&gt; GetOffsets ()
	/// {
	/// 	if (!store.HasOffsets) {
	/// 		yield break;
	///	}
	/// 	store.LoadOffsets ();
	/// 	foreach (int offset in store.Offsets) {
	/// 		yield return offset;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This method returns null but returning an empty instance would make it easier to use.")]
	[Solution ("Return an empty instance instead of null.")]
	public class PreferEmptyInstanceOverNullRule : ReturnNullRule, IMethodRule {

		TypeReference return_type;
		bool string_return_type;
		bool array_return_type;
		bool ienumerable_return_type;

		public override RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			//the rule does not apply to the particular case of ToString()
			//that have its own ToStringShouldNotReturnNullRule
			if (!method.HasParameters && method.Name == "ToString")
				return RuleResult.DoesNotApply;

			//only apply to methods returning string, array, or IEnumerable-impl
			return_type = method.ReturnType;
			string_return_type = return_type.IsNamed ("System", "String");
			array_return_type = return_type.IsArray;
			ienumerable_return_type = return_type.Implements ("System.Collections", "IEnumerable");

			if (!string_return_type && !array_return_type && !ienumerable_return_type)
				return RuleResult.DoesNotApply;

			return base.CheckMethod (method);
		}

		protected override void Report (MethodDefinition method, Instruction ins)
		{
			string msg = String.Format (CultureInfo.InvariantCulture, "Replace null with {0}.", SuggestReturnType ());
			Runner.Report (method, ins, method.IsVisible () ? Severity.Medium : Severity.Low, Confidence.Normal, msg);
		}

		string SuggestReturnType ()
		{
			if (string_return_type)
				return "string.Empty";
			else if (array_return_type)
				return String.Format (CultureInfo.InvariantCulture, "an empty {0} array", return_type.Name);
			else if (ienumerable_return_type)
				return "yield break (or equivalent)";
			return "an empty collection";
		}
	}
}
