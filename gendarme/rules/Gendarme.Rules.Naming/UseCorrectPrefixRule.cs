//
// Gendarme.Rules.Naming.UseCorrectPrefixRule class
//
// Authors:
//      Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2007 Daniel Abramov
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	// TODO: It would be nice to replace the C check with a more general hungarian name
	// rule.

	/// <summary>
	/// This rule ensures that types are prefixed correctly. Interfaces should always be prefixed
	/// with a <c>I</c>, types should never be prefixed with a <c>C</c> (reminder for MFC folks)
	/// and generic parameters should be a single, uppercased letter or be prefixed with <c>T</c>.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// public interface Phone {
	///	// ...
	/// }
	/// 
	/// public class CPhone : Phone {
	///	// ...
	/// }
	/// 
	/// public class Call&lt;Mechanism&gt; {
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	/// public interface IPhone {
	///	// ...
	/// }
	/// 
	/// public class Phone : IPhone {
	///	// ...
	/// }
	/// 
	/// public class Call&lt;TMechanism&gt; {
	///	// ...
	/// }
	/// </code>
	/// </example>

	[Problem ("This type starts with an incorrect prefix or does not start with the required one. All interface names should start with the 'I' letter, followed by another capital letter. All other type names should not have any specific prefix.")]
	[Solution ("Rename the type to have the correct prefix.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
	public class UseCorrectPrefixRule : Rule, ITypeRule {

		private static bool IsCorrectTypeName (string name)
		{
			if (name.Length < 3)
				return true;

			switch (name [0]) {
			case 'C':	// MFC like CMyClass - but works for CLSCompliant
			case 'I':	// interface-like
				return Char.IsLower (name [1]) == Char.IsLower (name [2]);
			default:
				return true;
			}
		}

		private static bool IsCorrectInterfaceName (string name)
		{
			if (name.Length < 3)
				return false;
			return ((name [0] == 'I') && Char.IsUpper (name [1]));
		}

		private static bool IsCorrectGenericParameterName (string name)
		{
			return (((name.Length > 1) && (name [0] != 'T')) || Char.IsLower (name [0]));
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			string name = type.Name;
			if (type.IsInterface) {
				// interfaces should look like 'ISomething'
				if (!IsCorrectInterfaceName (name)) { 
					string s = String.Format (CultureInfo.InvariantCulture,
						"The '{0}' interface name doesn't have the required 'I' prefix. Acoording to existing naming conventions, all interface names should begin with the 'I' letter followed by another capital letter.", 
						name);
					Runner.Report (type, Severity.Critical, Confidence.High, s);
				}
			} else {
				// class should _not_ look like 'CSomething" or like an interface 'IOops'
				if (!IsCorrectTypeName (name)) { 
					string s = String.Format (CultureInfo.InvariantCulture,
						"The '{0}' type name starts with '{1}' prefix but, according to existing naming conventions, type names should not have any specific prefix.", 
						name, name [0]);
					Runner.Report (type, Severity.Medium, Confidence.High, s);
				}
			}

			if (type.HasGenericParameters) {
				// check generic parameters. They are commonly a single letter T, V, K (ok)
				// but if they are longer (than one char) they should start with a 'T'
				// e.g. EventHandler<TEventArgs>
				foreach (GenericParameter parameter in type.GenericParameters) {
					string param_name = parameter.Name;
					if (IsCorrectGenericParameterName (param_name)) {
						string s = String.Format (CultureInfo.InvariantCulture,
							"The generic parameter '{0}' should be prefixed with 'T' or be a single, uppercased letter.", 
							param_name);
						Runner.Report (type, Severity.High, Confidence.High, s);
					}
				}
			}
			
			return Runner.CurrentRuleResult;
		}
	}
}
