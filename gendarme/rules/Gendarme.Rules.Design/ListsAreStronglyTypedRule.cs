// 
// Gendarme.Rules.Design.ListsAreStronglyTypedRule
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
	/// This rule checks that types which implements <c>System.Collections.IList</c> interface
	/// have strongly typed versions of IList.Item, IList.Add, IList.Contains, IList.IndexOf, IList.Insert and IList.Remove.
	/// This is needed to avoid casting every time these members are used.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class Bad : IList {
	///	public int Add (object value)
	///	{
	///		// method code
	///	}
	///	// other IList methods and properties without their strongly typed versions
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Good : Ilist {
	///	public int Add (object value)
	///	{
	///		// method code
	///	}			
	///	public int Add (Exception value)
	///	{
	///		return ((IList)this).Add ((object)value);
	///	}
	///	// other IList methods and properties with their strongly typed versions
	/// }
	/// </code>
	/// </example>

	
	[Problem ("Types that implement IList should have strongly typed versions of IList.Item, IList.Add, IList.Contains, IList.IndexOf, IList.Insert and IList.Remove")]
	[Solution ("Explicitly implement IList members and provide strongly typed alternatives to them.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class ListsAreStronglyTypedRule : StronglyTypedRule, ITypeRule {

		private static string [] Item = new string [] { "Item" };

		private static string[] SystemObject = new string[] {"System.Object"};
		private static MethodSignature Add = new MethodSignature ("Add", "System.Int32", SystemObject);
		private static MethodSignature Contains = new MethodSignature ("Contains", "System.Boolean", SystemObject);
		private static MethodSignature IndexOf = new MethodSignature ("IndexOf", "System.Int32", SystemObject);
		private static MethodSignature Insert = new MethodSignature ("Insert", "System.Void", new string [] { "System.Int32", "System.Object" });
		private static MethodSignature Remove = new MethodSignature ("Remove", "System.Void", SystemObject);

		private static MethodSignature [] Signatures = {
			Add,
			Contains,
			IndexOf,
			Insert,
			Remove,
		};

		protected override MethodSignature [] GetMethods ()
		{
			return Signatures;
		}

		protected override string [] GetProperties ()
		{
			return Item;
		}

		protected override string InterfaceName {
			get { return "IList"; }
		}

		protected override string InterfaceNamespace {
			get { return "System.Collections"; }
		}
	}
}
