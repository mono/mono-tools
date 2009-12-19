//
// Gendarme.Rules.Serialization.MarkEnumerationsAsSerializableRule
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

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule warns when it founds an <c>enum</c> that is not decorated with
	/// a <c>[Serializable]</c> attribute. Enums, even without the attribute,
	/// are always serializable. Marking them as such makes the source code more readable.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public enum Colors {
	///	Black,
	///	White
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Serializable]
	/// public enum Colors {
	///	Black,
	///	White
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("An enumeration, even if not decorated with [Serializable], is always serializable.")]
	[Solution ("For better source code readability always decorate enumerations with [Serializable].")]
	public class MarkEnumerationsAsSerializableRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			if (type.IsSerializable)
				return RuleResult.Success;

			Runner.Report (type, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
