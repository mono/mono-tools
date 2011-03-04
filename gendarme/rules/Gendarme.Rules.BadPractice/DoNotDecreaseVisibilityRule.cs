// 
// Gendarme.Rules.BadPractice.DoNotDecreaseVisibilityRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// The rule detect when a method visibility is decreased in an inherited type. 
	/// Decreasing visibility does not prevent calling the base class method unless 
	/// the type is <c>sealed</c>. Note that some language (but not C#) will allow 
	/// you to seal, e.g. <c>final</c>, the method without an <c>override</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Base {
	///	public void Public ()
	///	{
	///	}
	/// }
	/// 
	/// public class BadInheritor : Base {
	/// 	private new void Public ()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (do not hide):
	/// <code>
	/// public class Inheritor : Base {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (sealed type):
	/// <code>
	/// public sealed class Inheritor : Base {
	/// 	private new void Public ()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	[Problem ("A private method is hiding a visible method from a base type")]
	[Solution ("Either seal the inherited type or rename/remove the private method.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
	public class DoNotDecreaseVisibilityRule : Rule, IMethodRule {

		static bool IsHiding (MethodDefinition method, TypeReference type)
		{
			if (type == null)
				return false;

			TypeDefinition td = type.Resolve ();
			if ((td != null) && td.HasMethods) {
				string name = method.Name;
				foreach (MethodDefinition md in td.Methods) {
					if (!md.IsPublic && !md.IsFamily)
						continue;
					if (name != md.Name)
						continue;
					if (method.CompareSignature (md))
						return true;
				}
			}

			return IsHiding (method, type.DeclaringType);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsPrivate || method.IsFinal)
				return RuleResult.DoesNotApply;

			TypeDefinition type = method.DeclaringType;
			if (type.IsSealed)
				return RuleResult.DoesNotApply;

			// we got a private, non-final, method in an unsealed type

			// note: unlike CSC, MCS does not mark .cctor with hidebysig
			// this also covers a private default ctor inheriting from System.Object
			if (method.IsConstructor && !method.HasParameters)
				return RuleResult.Success;

			// are we're hiding something ?
			if (method.IsHideBySig && !IsHiding (method, type.BaseType))
				return RuleResult.Success;

			Runner.Report (method, Severity.High, Confidence.Normal);
			return RuleResult.Failure;
		}
	}
}

