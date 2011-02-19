//
// Gendarme.Rules.Design.MarshalBooleansInPInvokeDeclarationsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule warns the developer if a <code>[MarshalAs]</code> attribute has not been 
	/// specified for boolean parameters of a P/Invoke method. The size of boolean types varies
	/// across language (e.g. the C++ <c>bool</c> type is four bytes on some platforms and
	/// one byte on others). By default the CLR will marshal <b>System.Boolean</b> as a 32 bit value 
	/// (<c>UnmanagedType.Bool</c>) like the Win32 API <b>BOOL</b> uses. But, for clarity, 
	/// you should always specify the correct value.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // bad assuming the last parameter is a single byte being mapped to a bool
	/// [DllImport ("liberty")]
	/// private static extern bool Bad (bool b1, ref bool b2);
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [DllImport ("liberty")]
	/// [return: MarshalAs (UnmanagedType.Bool)]
	/// private static extern bool Good ([MarshalAs (UnmanagedType.Bool)] bool b1, [MarshalAs (UnmanagedType.U1)] ref bool b2);
	/// </code>
	/// </example>

	[Problem ("No marshaling information is supplied for boolean parameters(s) on this p/invoke declaration.")]
	[Solution ("Specify, even if just for clarity, the size of the unmanaged boolean representation.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs")]
	public class MarshalBooleansInPInvokeDeclarationsRule : Rule, IMethodRule {

		static bool CheckBooleanMarshalling (IMarshalInfoProvider spec, TypeReference type)
		{
			// is marshalling information provided
			if (spec.MarshalInfo != null)
				return true;
			// using StartsWith to catch references (ref)
			return !(type.Namespace == "System" && type.Name.StartsWith ("Boolean", StringComparison.Ordinal));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only to p/invoke methods
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			// check all parameters
			if (method.HasParameters) {
				foreach (ParameterDefinition parameter in method.Parameters) {
					if (!CheckBooleanMarshalling (parameter, parameter.ParameterType)) {
						// we can't be sure (confidence) that the MarshalAs is needed
						// since most of the time the default (4 bytes) is ok - but 
						// the severity is high because this mess up the stack
						Runner.Report (parameter, Severity.High, Confidence.Normal);
					}
				}
			}

			// and check return value
			MethodReturnType mrt = method.MethodReturnType;
			if (!CheckBooleanMarshalling (mrt, mrt.ReturnType)) {
				Runner.Report (mrt, Severity.High, Confidence.Normal);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
