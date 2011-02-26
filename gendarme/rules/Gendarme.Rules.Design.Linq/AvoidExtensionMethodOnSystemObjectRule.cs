//
// Gendarme.Rules.Design.Linq.AvoidExtensionMethodOnSystemObjectRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Linq {

	// ref: http://blogs.msdn.com/mirceat/archive/2008/03/13/linq-framework-design-guidelines.aspx

	/// <summary>
	/// Extension methods should not be used to extend <c>System.Object</c>.
	/// Such extension methods cannot be consumed by some languages, like VB.NET,
	/// which use late-binding on <c>System.Object</c> instances.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public static class Extensions {
	///	public static string ToDebugString (this object self)
	///	{
	///		return String.Format ("'{0}', type '{1}', hashcode: {2}", 
	///			self.ToString (), self.GetType (), self.GetHashCode ());
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public static class Extensions {
	///	public static string ToDebugString (this DateTime self)
	///	{
	///		return String.Format ("'{0}', type '{1}', hashcode: {2}", 
	///			self.ToString (), self.GetType (), self.GetHashCode ());
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This method extends System.Object. This will not work for VB.NET consumer.")]
	[Solution ("Either extend a subclass of System.Object or ignore the defect.")]
	public class AvoidExtensionMethodOnSystemObjectRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);
			// extension methods are only available in FX3.5
			// check runtime >= NET2_0 (fast) then check if [ExtensionAttribute] is referenced
			Runner.AnalyzeModule += (object o, RunnerEventArgs e) => {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0 &&
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Runtime.CompilerServices", "ExtensionAttribute");
					})
				);
			};
		}

		// rock-ify
		// not 100% bullet-proof against buggy compilers (or IL)
		static bool IsExtension (MethodDefinition method)
		{
			if (!method.IsStatic)
				return false;

			if (!method.HasParameters)
				return false;

			return method.HasAttribute ("System.Runtime.CompilerServices", "ExtensionAttribute");
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!IsExtension (method))
				return RuleResult.DoesNotApply;

			if (!method.Parameters [0].ParameterType.IsNamed ("System", "Object"))
				return RuleResult.Success;

			Runner.Report (method, Severity.High, Confidence.High);
			return RuleResult.Failure;
		}
	}
}
