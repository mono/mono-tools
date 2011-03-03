//
// Gendarme.Rules.Globalization.PreferStringComparisonOverrideRule
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Globalization {

	/// <summary>
	/// This rule detects calls to method that could be changed to call an <c>override</c> accepting an
	/// extra <c>System.StringComparison</c> parameter. Using the <c>override</c> makes the code easier
	/// to maintain since it makes the intent clear on how the string needs to be compared.
	/// It is even more important since the default string comparison rules have changed between
	/// .NET 2.0 and .NET 4.0.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool Check (string name)
	/// {
	///	// it's not clear if the string comparison should be culture sensitive or not
	///	return (String.Compare (name, "Software") == 0);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool Check (string name)
	/// {
	///	return (String.Compare (name, "Software", StringComparison.CurrentCulture) == 0);
	/// }
	/// </code>
	/// </example>
	[Problem ("A call is made to a method for which an override, accepting an extra StringComparison, is available")]
	[Solution ("Specify how the string should be compared by adding the right StringComparison value to the call")]
	[FxCopCompatibility ("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
	public class PreferStringComparisonOverrideRule : PreferOverrideBaseRule {

		protected override bool IsPrefered (TypeReference type)
		{
			return type.IsNamed ("System", "StringComparison");
		}

		protected override void Report (MethodDefinition method, Instruction instruction, MethodReference prefered)
		{
			string msg = String.Format (CultureInfo.InvariantCulture, 
				"Consider using the perfered '{0}' override.", prefered.GetFullName ());
			Runner.Report (method, instruction, Severity.Medium, Confidence.High, msg);
		}
	}
}
