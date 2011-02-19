//
// Gendarme.Rules.Naming.AvoidTypeInterfaceInconsistencyRule
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
using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule will fire if an assembly has a namespace which contains an interface IFoo
	/// and a type Foo, but the type does not implement the interface. If an interface and
	/// a type name differ only by the <c>I</c> prefix (of the interface) then we can
	/// logically expect the type to implement this interface.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public interface IMember {
	///	string Name {
	///		get;
	///	}
	/// }
	/// 
	/// public class Member {
	///	public string Name {
	///		get {
	///			return String.Empty;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public interface IMember {
	///	string Name {
	///		get;
	///	}
	/// }
	/// 
	/// public class Member : IMember {
	///	public string Name {
	///		get {
	///			return String.Empty;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This interface is not implemented by the type of the same name (minus the 'I' prefix).")]
	[Solution ("Rename either the interface or the type to something else or implement the interface for the type.")]
	public class AvoidTypeInterfaceInconsistencyRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to interfaces
			if (!type.IsInterface)
				return RuleResult.DoesNotApply;

			// badly named interface, we let another rule report this
			string name = type.Name;
			if (name [0] != 'I')
				return RuleResult.DoesNotApply;

			string nspace = type.Namespace;
			TypeDefinition candidate = type.Module.GetType (nspace, name.Substring (1));
			if (candidate != null) {
				// does Foo implement IFoo ?
				if (!candidate.Implements (nspace, name)) {
					Runner.Report (candidate, Severity.High, Confidence.High);
				}
			}
			return RuleResult.Success;
		}
	}
}
