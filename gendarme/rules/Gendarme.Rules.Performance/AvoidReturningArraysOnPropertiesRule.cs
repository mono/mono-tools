//
// Gendarme.Rules.Performance.AvoidReturningArraysOnPropertiesRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule check for properties which return arrays. This can be a problem because
	/// properties are supposed to execute very quickly so it's likely that this property
	/// is returning a reference to the internal state of the object. This means that
	/// the caller can change the object's internal state via a back-door channel which
	/// is usually a very bad thing and it means that the array's contents may change
	/// unexpectedly if the caller holds onto the array.
	///
	/// The preferred approach is to either return a read-only collection or to change
	/// the property to a method and return a copy of the array (it's important to use
	/// a method so that callers are not misled about the performance of the property).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public byte[] Foo {
	///	get {
	///		// return the data inside the instance
	///		return foo;
	///	}
	/// }
	/// 
	/// public byte[] Bar {
	///	get {
	///		// return a copy of the instance's data
	///		// (this is bad because users expect properties to execute quickly)
	///		return (byte[]) bar.Clone ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public byte[] GetFoo ()
	/// {
	///	return (byte[]) foo.Clone ();
	/// }
	/// 
	/// public byte[] GetFoo ()
	/// {
	///	return (byte[]) bar.Clone ();
	/// }
	/// </code>
	/// </example>

	[Problem ("By convention properties should not return arrays.")]
	[Solution ("Return a read-only collection or replace the property by a method and return a copy of the array.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public class AvoidReturningArraysOnPropertiesRule : Rule, IMethodRule {
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// avoid checking all methods unless the type has some properties
			Runner.AnalyzeType += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentType.HasProperties;
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsGetter)
				return RuleResult.DoesNotApply;

			if (!method.ReturnType.IsArray)
				return RuleResult.Success;

			Runner.Report (method, Severity.Medium, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
