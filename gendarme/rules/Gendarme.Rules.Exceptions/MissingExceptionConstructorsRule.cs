// 
// Gendarme.Rules.Exceptions.MissingExceptionConstructorsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule will fire if an exception class is missing one or more of the following
	/// constructors:
	/// <list>
	/// <item><description><c>public E ()</c> is required for XML serialization. Public access is required
	/// in case the assembly uses CAS to prevent reflection on non-public members.</description></item>
	/// <item><description><c>public E (string message)</c> is a .NET convention.</description></item>
	/// <item><description><c>public E (string message, ..., Exception inner)</c> is a .NET convention.</description></item>
	/// <item><description><c>(non)public E (SerializationInfo info, StreamingContext context)</c> is required for binary serialization.</description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class GeneralException : Exception {
	///	// access should be public
	/// 	private GeneralException ()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class GeneralException : Exception {
	/// 	public GeneralException ()
	/// 	{
	/// 	}
	/// 	
	/// 	public GeneralException (string message) : base (message)
	/// 	{
	/// 	}
	/// 	
	/// 	public GeneralException (string message, Exception inner) : base (message, inner)
	/// 	{
	/// 	}
	/// 	
	/// 	protected GeneralException (SerializationInfo info, StreamingContext context) : base (info, context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The exception does not provide all of the constructors required by the runtime or by .NET programming conventions.")]
	[Solution ("Add the missing constructor(s).")]
	[FxCopCompatibility ("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	public class MissingExceptionConstructorsRule : Rule, ITypeRule {

		// non-localizable
		private const string Exception = "System.Exception";

		// localizable
		private const string MissingConstructor = "Exception is missing '{0} {1}{2}' constructor.";

		private static bool CheckForStringConstructor (MethodDefinition ctor)
		{
			if (!ctor.IsPublic)
				return false;

			return (ctor.Parameters [0].ParameterType.FullName == "System.String");
		}

		private static bool CheckForInnerExceptionConstructor (IMethodSignature ctor)
		{
			IList<ParameterDefinition> pdc = ctor.Parameters;
			string first = pdc [0].ParameterType.FullName;
			string last = pdc [pdc.Count - 1].ParameterType.FullName;
			return ((first == "System.String") && (last == Exception));
		}

		private static bool CheckForSerializationConstructor (MethodDefinition ctor)
		{
			if (ctor.IsPrivate || ctor.IsFamily)
				return MethodSignatures.SerializationConstructor.Matches (ctor);

			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only to type that inherits from System.Exception
			if (!type.Inherits (Exception))
				return RuleResult.DoesNotApply;

			// rule applies, only Success or Failure from the point on

			// check if the type implements all the needed exception constructors

			bool empty_ctor = false;		// MyException ()
			bool string_ctor = false;		// MyException (string message)
			bool inner_exception_ctor = false;	// MyException (string message, Exception innerException)
			bool serialization_ctor = false;	// MyException (SerializationInfo info, StreamingContext context)

			foreach (MethodDefinition ctor in type.Methods) {
				// skip non-constructors and cctor
				if (!ctor.IsConstructor || ctor.IsStatic)
					continue;

				if (!ctor.HasParameters) {
					// there can be only one so only it's visibility matters
					empty_ctor = ctor.IsPublic;
					continue;
				}

				switch (ctor.Parameters.Count) {
				case 1:
					string_ctor |= CheckForStringConstructor (ctor);
					break;
				case 2:
					if (ctor.IsPublic) {
						if (!inner_exception_ctor) {
							inner_exception_ctor = CheckForInnerExceptionConstructor (ctor);
							if (inner_exception_ctor)
								break;
						}

						string_ctor |= CheckForStringConstructor (ctor);
					} else {
						serialization_ctor |= CheckForSerializationConstructor (ctor);
					}
					break;
				default:
					inner_exception_ctor |= CheckForInnerExceptionConstructor (ctor);
					break;
				}
			}

			if (!empty_ctor) {
				string s = String.Format (MissingConstructor, "public", type.Name, "()");
				Runner.Report (type, Severity.High, Confidence.Total, s);
			}
			if (!string_ctor) {
				string s = String.Format (MissingConstructor, "public", type.Name, "(string message)");
				Runner.Report (type, Severity.High, Confidence.Total, s);
			}
			if (!inner_exception_ctor) {
				string s = String.Format (MissingConstructor, "public", type.Name,
					"(string message, Exception innerException)");
				Runner.Report (type, Severity.High, Confidence.Total, s);
			}
			if (!serialization_ctor) {
				string s = String.Format (MissingConstructor, (type.IsSealed) ? "private" : "protected",
					type.Name, "(SerializationInfo info, StreamingContext context)");
				Runner.Report (type, Severity.High, Confidence.Total, s);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
