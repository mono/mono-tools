// 
// Gendarme.Rules.Design.StronglyTypeICollectionMembersRule
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
	/// This rule checks that types which implements <c>System.Collections.ICollection</c> interface
	/// have strongly typed version of the ICollection.CopyTo method.
	/// This is needed to avoid casting every time this method is used.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class Bad : ICollection {
	///	public void CopyTo (Array array, int index)
	///	{
	///		// method code
	///	}
	///	// other ICollection members
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Good : ICollection {
	///	public void ICollection.CopyTo (Array array, int index)
	///	{
	///		// method code
	///	}
	///	public void CopyTo (Exception [] array, int index)
	///	{
	///		((ICollection)this).CopyTo(array, index);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("Types that implement ICollection interface should have strongly typed version of ICollection.CopyTo method")]
	[Solution ("Explicitly implement ICollection.CopyTo and add strongly typed alternative to it")]
	[FxCopCompatibility ("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	public class StronglyTypeICollectionMembersRule : StronglyTypedRule, ITypeRule {

		private static string [] Empty = new string [] { };

		private static MethodSignature [] CopyTo = new MethodSignature [] {
			new MethodSignature ("CopyTo", "System.Void", new string [] { "System.Array", "System.Int32" })
		};

		protected override MethodSignature [] GetMethods ()
		{
			return CopyTo;
		}

		protected override string [] GetProperties ()
		{
			return Empty;
		}

		protected override string InterfaceName {
			get { return "ICollection"; }
		}

		protected override string InterfaceNamespace {
			get { return "System.Collections"; }
		}
	}
}
