//
// Gendarme.Rules.Interoperability.MarshalStringsInPInvokeDeclarationsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2008 Daniel Abramov
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

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Interoperability {

	// TODO: The summary should say what the default charset is for mono (utf-8) and
	// .NET (?).

	/// <summary>
	/// This rule will fire if a P/Invoke method has System.String or System.Text.StringBuilder
	/// arguments, and the DllImportAttribute does not specify the <code>CharSet</code>, 
	/// and the string arguments are not decorated with <code>[MarshalAs]</code>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [DllImport("coredll.dll")]
	/// static extern int SHCreateShortcut (StringBuilder szShortcut, StringBuilder szTarget);
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	/// [DllImport("coredll.dll", CharSet = CharSet.Auto)]
	/// static extern int SHCreateShortcut (StringBuilder szShortcut, StringBuilder szTarget);
	/// 
	/// [DllImport("coredll.dll")]
	/// static extern int SHCreateShortcut ([MarshalAs(UnmanagedType.LPTStr)] StringBuilder szShortcut, 
	///	[MarshalAs(UnmanagedType.LPTStr)] StringBuilder szTarget);
	/// </code>
	/// </example>

	[Problem ("Marshaling information for string types is missing and what is required may be different from what you expected the default to be.")]
	[Solution ("Add [DllImport CharSet=] to the method or [MarshalAs] on the parameter(s)")]
	[FxCopCompatibility ("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
	public class MarshalStringsInPInvokeDeclarationsRule : Rule, IMethodRule {

		private static bool IsStringOrSBuilder (TypeReference reference)
		{
			switch (reference.GetOriginalType ().FullName) {
			case "System.String":
			case "System.Text.StringBuilder":
				return true;
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to non-pinvoke methods
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			if (!method.PInvokeInfo.IsCharSetNotSpec || !method.HasParameters)
				return RuleResult.Success;

			foreach (ParameterDefinition parameter in method.Parameters) {
				if (IsStringOrSBuilder (parameter.ParameterType) && (parameter.MarshalSpec == null)) {
					string text = string.Format ("Parameter '{0}', of type '{1}', does not have [MarshalAs] attribute, yet no [DllImport CharSet=] is set for the method '{2}'.",
						parameter.Name, parameter.ParameterType.Name, parameter.Method.Name);
					Runner.Report (parameter, Severity.High, Confidence.Total, text);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
