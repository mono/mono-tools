// 
// Gendarme.Rules.Design.EnumeratorsShouldBeStronglyTypedRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks that types which implements <c>System.Collections.IEnumerator</c> interface
	/// have strongly typed version of the IEnumerator.Current property.
	/// This is needed to avoid casting every time this property is used.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class Bad : IEnumerator {
	///	object Current
	///	{
	///		get { return current; }
	///	}
	///	// other IEnumerator members
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Good : IEnumerator {
	///	object IEnumerator.Current
	///	{
	///		get { return current; }
	///	}
	///	public Exception Current
	///	{
	///		get { return (Exception)current; }
	///	}
	///	// other IEnumerator members
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// Types inheriting from <c>System.Collections.CollectionBase</c>, <c>System.Collections.DictionaryBase</c> 
	/// or <c>System.Collections.ReadOnlyCollectionBase</c> are exceptions to this rule.</remarks>

	[Problem ("Types that implement IEnumerator interface should have strongly typed version of IEnumerator.Current property")]
	[Solution ("Explicitly implement IEnumerator.Current and add strongly typed alternative to it")]
	[FxCopCompatibility ("Microsoft.Design", "CA1038:EnumeratorsShouldBeStronglyTyped")]
	public class EnumeratorsShouldBeStronglyTypedRule : StronglyTypedRule, ITypeRule {

		private MethodSignature [] Empty = { };
		private static string [] Current = { "Current" };

		protected override MethodSignature [] GetMethods ()
		{
			return Empty;
		}

		protected override string [] GetProperties ()
		{
			return Current;
		}

		protected override string InterfaceName {
			get { return "IEnumerator"; }
		}

		protected override string InterfaceNamespace {
			get { return "System.Collections"; }
		}

		override public RuleResult CheckType (TypeDefinition type)
		{
			TypeReference baseType = type;
			while (baseType != null) {
				if (baseType.Namespace == "System.Collections") {
					switch (baseType.Name) {
					case "CollectionBase":
					case "DictionaryBase":
					case "ReadOnlyCollectionBase":
						return RuleResult.DoesNotApply;
					}
				}

				TypeDefinition td = baseType.Resolve ();
				if (td != null)
					baseType = td.BaseType;
				else
					baseType = null;
			}

			return base.CheckType (type);
		}
	}
}
