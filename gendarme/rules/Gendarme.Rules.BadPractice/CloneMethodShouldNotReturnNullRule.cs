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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks for <c>Clone()</c> methods which return <c>null</c>.
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

	[Problem ("This implementation of ICloneable.Clone() could return null in some circumstances.")]
	[Solution ("Return an appropriate object instead of returning null.")]
	public class CloneMethodShouldNotReturnNullRule : ReturnNullRule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference System.ICloneable then
			// no type inside will be implementing it
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System", "ICloneable");
					}));
			};
		}

		public override RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only to Clone methods with a body (IL)
			if (!method.HasBody || !MethodSignatures.Clone.Matches (method))
				return RuleResult.DoesNotApply;

			// where the type implements ICloneable
			if (!method.DeclaringType.Implements ("System", "ICloneable"))
				return RuleResult.DoesNotApply;

			// call base class to detect if the method can return null
			return base.CheckMethod (method);
		}

		protected override void Report (MethodDefinition method, Instruction ins)
		{
			Runner.Report (method, ins, Severity.Medium, Confidence.Normal);
		}
	}
}
