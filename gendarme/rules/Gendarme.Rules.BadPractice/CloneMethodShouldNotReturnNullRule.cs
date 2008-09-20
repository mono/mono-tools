//
// Gendarme.Rules.BadPractice.CloneMethodShouldNotReturnNullRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule check that a <c>Clone()</c> method, if existing, never returns a <c>null</c> 
	/// value.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class MyClass : ICloneable {
	///	public object Clone ()
	///	{
	///		MyClass myClass = new MyClass ();
	///		// set some internals
	///		return null;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class MyClass : ICloneable {
	///	public object Clone ()
	///	{
	///		MyClass myClass = new MyClass ();
	///		// set some internals
	///		return myClass;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The implementation ICloneable.Clone () seems to return null in some circumstances.")]
	[Solution ("Return an appropriate object instead of returning null.")]
	public class CloneMethodShouldNotReturnNullRule : ReturnNullRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to types implementing System.ICloneable
			if (!type.Implements ("System.ICloneable"))
				return RuleResult.DoesNotApply;

			// rule applies only if a body is available (e.g. not for pinvokes...)
			MethodDefinition method = type.GetMethod (MethodSignatures.Clone);
			if ((method == null) || (!method.HasBody))
				return RuleResult.DoesNotApply;

			// call base class to detect if the method can return null
			return CheckMethod (method);
		}
	}
}
