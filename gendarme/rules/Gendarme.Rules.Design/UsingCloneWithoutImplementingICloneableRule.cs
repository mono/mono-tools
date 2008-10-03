//
// Gendarme.Rules.Design.ImplementICloneableCorrectlyRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule warns every time you implement a <c>object Clone()</c> method without 
	/// implementing the <c>System.ICloneable</c> interface. Either implement the interface 
	/// or, if possible, change the return type to the type definition (since can avoid
	/// unnecessary casts).
	/// <list type="bullet"><description>Note: Make sure to document the behavior of your
	/// Clone method since the framework itself is not very clear, or consistent, between 
	/// shallow and deep cloning.</description></list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class MyClass {
	///	public object Clone ()
	///	{
	///		MyClass myClass = new MyClass ();
	///		return myClass;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (ICloneable):
	/// <code>
	/// public class MyClass : ICloneable {
	///	public object Clone ()
	///	{
	///		MyClass myClass = new MyClass ();
	///		return myClass;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (not returning object):
	/// <code>
	/// public class MyClass {
	///	public MyClass Clone ()
	///	{
	///		MyClass myClass = new MyClass ();
	///		return myClass;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named UsingCloneWithoutImplementingICloneableRule</remarks>

	[Problem ("This type provides a Clone() method returning System.Object but does not implement the ICloneable interface.")]
	[Solution ("Implement the ICloneable interface or change the return type to this type.")]
	public class ImplementICloneableCorrectlyRule: Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies to type that doesn't implement System.IClonable
			if (type.Implements ("System.ICloneable"))
				return RuleResult.DoesNotApply;

			foreach (MethodDefinition method in type.Methods) {
				// note: we don't use MethodSignatures.Clone because we want to
				// (a) check the return value ourselves
				// (b) deal with possibly multiple Clone methods
				if (method.Name != "Clone")
					continue;

				if (method.Parameters.Count > 0)
					continue;

				// that return System.Object, e.g. public object Clone()
				// or the current type, e.g. public <type> Clone()
				if (method.ReturnType.ReturnType.FullName == "System.Object")
					Runner.Report (method, Severity.Low, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
