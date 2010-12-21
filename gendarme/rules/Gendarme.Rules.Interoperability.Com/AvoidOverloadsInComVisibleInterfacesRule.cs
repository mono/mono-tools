//
// Gendarme.Rules.Interoperability.Com.AvoidOverloadsInComVisibleInterfacesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
// 
// Copyright (C) 2010 N Lum
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

using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// This rule checks for ComVisible interface which contains overloaded methods.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	///	[ComVisible(true)]
	///	public interface Bad {
	///		void SomeMethod();
	///		void SomeMethod(int SomeValue);
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [ComVisible(true)]
	///	public interface Good {
	///		void SomeMethod();
	///		void SomeMethodWithValue(int SomeValue);
	///	}
	/// </code>
	/// </example>

	[Problem ("ComVisible interfaces which contain overloaded methods cannot be implemented by all COM clients.")]
	[Solution ("Rename the overloaded methods so the names are unique, change visiblity of the interface to internal, or change the ComVisibleAttribute value to false.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1402:AvoidOverloadsInComVisibleInterfaces")]
	public class AvoidOverloadsInComVisibleInterfacesRule : Rule, ITypeRule {

		private HashSet<string> methods;

		public AvoidOverloadsInComVisibleInterfacesRule()
		{
			methods = new HashSet<string> ();
		}

		public RuleResult CheckType(TypeDefinition type)
		{
			// This rule only applies to public interfaces with methods and explicit ComVisible.
			if (!type.IsInterface || !type.IsPublic || !type.HasMethods)
				return RuleResult.DoesNotApply;

			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			methods.Clear ();
			foreach (MethodDefinition method in type.Methods) {
				var name = method.Name;
				if (methods.Contains (name) && (method.IsComVisible () ?? true))
					Runner.Report (method, Severity.Critical, Confidence.Total);
				else
					methods.Add (name);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
