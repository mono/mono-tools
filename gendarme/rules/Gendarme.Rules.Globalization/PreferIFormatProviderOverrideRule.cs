//
// Gendarme.Rules.Globalization.PreferIFormatProviderOverrideRule
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
	/// extra <c>System.IFormatProvider</c> or <c>System.Globalization.CultureInfo</c> parameter (the
	/// later implements <c>System.IFormatProvider</c>).
	/// Generally data displayed to the end user should be using 
	/// <c>System.Globalization.CultureInfo.CurrentCulture</c> while other data (e.g. used internally,
	/// stored in files/databases) should use <c>System.Globalization.CultureInfo.InvariantCulture</c>.
	/// The rule will ignore the following special methods:
	/// <list>
	/// <item><c>System.Activator.CreateInstance</c></item>
	/// <item><c>System.Resources.ResourceManager.GetObject</c></item>
	/// <item><c>System.Resources.ResourceManager.GetString</c></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool Confirm (double amount)
	/// {
	///	string msg = String.Format ("Accept payment of {0} ?", amount);
	///	Transaction.Log ("{0} {1}", DateTime.Now, amount);
	///	return Prompt (msg);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool Confirm (double amount)
	/// {
	///	string msg = String.Format (CultureInfo.CurrentCulture, "Accept payment of {0} ?", amount);
	///	Transaction.Log (CultureInfo.InvariantCulture, "{0} {1}", DateTime.Now, amount);
	///	return Prompt (msg);
	/// }
	/// </code>
	/// </example>
	[Problem ("A call is made to a method for which an override, accepting an extra IFormatProvider or CultureInfo, is available")]
	[Solution ("Specify how the string should be compared by adding the right IFormatProvider/CultureInfo value to the call")]
	[FxCopCompatibility ("Microsoft.Globalization", "CA1304:SpecifyCultureInfo")]
	[FxCopCompatibility ("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
	public class PreferIFormatProviderOverrideRule : PreferOverrideBaseRule {

		protected override bool CheckFirstParameter
		{
			get { return true; }
		}

		protected override bool IsPrefered (TypeReference type)
		{
			return (type.IsNamed ("System", "IFormatProvider") || type.IsNamed ("System.Globalization", "CultureInfo"));
		}

		protected override bool IsSpecialCase (MethodReference method)
		{
			if ((method == null) || method.IsNamed ("System", "Activator", "CreateInstance"))
				return true;

			TypeReference type = method.DeclaringType;
			if (!type.IsNamed ("System.Resources", "ResourceManager"))
				return false;

			string name = method.Name;
			return (name == "GetObject" || name == "GetString");
		}

		protected override void Report (MethodDefinition method, Instruction instruction, MethodReference prefered)
		{
			string msg = String.Format (CultureInfo.InvariantCulture,
				"Consider using the perfered '{0}' override.", prefered.GetFullName ());
			Runner.Report (method, instruction, Severity.Medium, Confidence.High, msg);
		}
	}
}
