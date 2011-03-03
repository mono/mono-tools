//
// ComVisibleShouldInheritFromComVisibleRule.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// This rule checks that the base type of COM visible types is also 
	/// visible from COM. This is needed reduce the chance of breaking 
	/// COM clients as COM invisible types do not have to follow COM versioning rules.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assemply: ComVisible(false)]
	/// namespace InteropLibrary {
	///	[ComVisible (false)]
	///	public class Base {
	///	}
	///	
	///	[ComVisible (true)]
	///	public class Derived : Base {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [assemply: ComVisible(false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public class Base {
	///	}
	///	
	///	[ComVisible (true)]
	///	public class Derived : Base {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (both types are invisible because of the assembly attribute):
	/// <code>
	/// [assemply: ComVisible(false)]
	/// namespace InteropLibrary {
	///	public class Base {
	///	}
	///	
	///	public class Derived : Base {
	///	}
	/// }
	/// </code>
	/// </example>
	[Problem ("COM visible class is derived from COM invisible class")]
	[Solution ("Make derived type invisible from COM or make base type visible")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	public class ComVisibleShouldInheritFromComVisibleRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.BaseType == null)
				return RuleResult.DoesNotApply;

			// Checks whether specific type is COM visible or not
			// considering nested types, assemblies attributes and default values
			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			TypeDefinition baseType = type.BaseType.Resolve ();
			if ((baseType != null) && !baseType.IsTypeComVisible ()) {
				string msg = String.Format (CultureInfo.InvariantCulture, 
					"Type is derived from invisible from COM type {0}", baseType.GetFullName ());
				Runner.Report (type, Severity.High, Confidence.Total, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
