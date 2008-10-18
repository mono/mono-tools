//
// Gendarme.Rules.Naming.UseCorrectPrefixRule class
//
// Authors:
//      Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2007 Daniel Abramov
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

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensure that type are prefixed correctly. E.g. interface should start with a <c>I</c>
	/// and classes should not start with a <c>C</c> (remainder for MFC folks).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public interface Phone {
	/// }
	/// 
	/// public class CPhone : Phone {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public interface IPhone {
	/// }
	/// 
	/// public class Phone : IPhone {
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
			bool bad = name [0] == 'C' && char.IsUpper (name [1]) && char.IsLower (name [2]);
			return !bad;
		}

		private static bool IsCorrectInterfaceName (string name)
		{
			if (name.Length < 3)
				return false;
			return name [0] == 'I' && char.IsUpper (name [1]);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsInterface) {
				if (!IsCorrectInterfaceName (type.Name)) { // interfaces should look like 'ISomething'
					string s = String.Format ("The '{0}' interface name doesn't have the required 'I' prefix. Acoording to existing naming conventions, all interface names should begin with the 'I' letter followed by another capital letter.", type.Name);
					Runner.Report (type, Severity.Critical, Confidence.High, s);
					return RuleResult.Failure;
				}
			} else {
				if (!IsCorrectTypeName (type.Name)) { // class should _not_ look like 'CSomething"
					string s = String.Format ("The '{0}' type name starts with 'C' prefix but, according to existing naming conventions, type names should not have any specific prefix.", type.Name);
					Runner.Report (type, Severity.Medium, Confidence.High, s);
					return RuleResult.Failure;
				}
			}
			return RuleResult.Success;
		}
	}
}
