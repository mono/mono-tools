// 
// Gendarme.Rules.Design.UseFlagsAttributeRule
//
// Authors:
//	Jesse Jones  <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule will fire if an enum's values look like they are intended to
	/// be composed together with the bitwise OR operator and the enum is not
	/// decorated with <c>System.FlagsAttribute</c>. Using <c>FlagsAttribute</c> will
	/// allow <c>System.Enum.ToString()</c> to return a better string when 
	/// values are ORed together and helps indicate to readers of the code 
	/// the intended usage of the enum.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Serializable]
	/// enum Options {
	///		First = 1,
	///		Second = 2,
	///		Third = 4,
	///		All = First | Second | Third,
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Flags]
	/// [Serializable]
	/// enum Options {
	///		First = 1,
	///		Second = 2,
	///		Third = 4,
	///		All = First | Second | Third,
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("The enum seems to be composed of flag values, but is not decorated with [Flags].")]
	[Solution ("Add [Flags] to the enum,  change the values so that they are not powers of two, or ignore the defect.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public sealed class UseFlagsAttributeRule : Rule, ITypeRule {
	
		private List<ulong> values = new List<ulong> ();

		private void GetValues (TypeDefinition type)
		{
			values.Clear ();
			
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsStatic) {
					object o = field.Constant;
					ulong value;
					if (o is ulong) {
						value = (ulong) o;

						if (value != 0 && !values.Contains (value))
							values.Add (value);

					} else {
						long v = Convert.ToInt64 (o, CultureInfo.InvariantCulture);

						if (v > 0) {
							value = (ulong) v;
							if (!values.Contains (value))
								values.Add (value);
						} else if (v < 0) {
							values.Clear ();
							break;
						}
					}
				}
			}
		}
		
		static bool IsPowerOfTwo (ulong x)
		{
			Debug.Assert (x > 0, "x is not positive");

			return (x & (x - 1)) == 0;
		}
		
		private bool IsBitmask (ulong x)
		{
			for (int i = 0; i < values.Count && x != 0; ++i) {
				ulong bit = values [i];
				if (IsPowerOfTwo (bit))
					x &= ~bit;
			}
			
			return x == 0;
		}
		
		private int CountSequential ()
		{
			int count = 0;
			int currentCount = 1;
			
			for (int i = 1; i < values.Count; ++i) {
				if (values [i] == values [i - 1] + 1) {
					++currentCount;
				} else {
					count = Math.Max (currentCount, count);
					currentCount = 0;
				}
			}
						
			return Math.Max (currentCount, count);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			if (type.IsFlags ())
				return RuleResult.DoesNotApply;

			GetValues (type);
			if (values.Count < 3)
				return RuleResult.Success;

#if DEBUG
			Log.WriteLine (this);
			Log.WriteLine (this, "------------------------------------");
			Log.WriteLine (this, type);
			Log.WriteLine (this, "values: {0}", string.Join (" ", (from x in values select x.ToString ("X4")).ToArray ()));
#endif

			int numFlags = 0;
			int numMasks = 0;
			foreach (ulong value in values) {
				if (IsPowerOfTwo (value))
					++numFlags;
				else if (IsBitmask (value))
					++numMasks;
			}
			
			Log.WriteLine (this, "numFlags: {0}", numFlags);
			Log.WriteLine (this, "numMasks: {0}", numMasks);

			// The enum is bad if all of the values are powers of two or composed 
			// of defined powers of two,
			if (numFlags + numMasks == values.Count) {
				values.Sort ();		// sometimes enums are all sequential but not in order
				
				int numSequential = CountSequential ();				
				Log.WriteLine (this, "numSequential: {0}", numSequential);

				// and there are not too many sequential values (so we don't
				// complain about stuff like 1, 2, 3, 4, 5, 6).
				if (numSequential < 3) {
					Confidence confidence = values.Count >= 4 && numMasks == 0 ? Confidence.High : Confidence.Normal;
					Runner.Report (type, Severity.Medium, confidence);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
