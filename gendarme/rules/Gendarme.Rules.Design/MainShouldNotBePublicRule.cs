// 
// Gendarme.Rules.Design.MainShouldNotBePublicRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an assembly's entry point (typically named <c>Main</c>) is visible 
	/// to other assemblies. It is better to make this method private so that only the CLR
	/// can call the method.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class MainClass {
	///	public void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (type is not externally visible):
	/// <code>
	/// internal class MainClass {
	///	public void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (method is not externally visible):
	/// <code>
	/// public class MainClass {
	///	internal void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The entry point (Main) of this assembly is visible to the outside world (ref: C# Programming Guide).")]
	[Solution ("Reduce the visibility of the method or type if your language allows it. It may not be possible in some language, like VB.NET).")]
	public class MainShouldNotBePublicRule : Rule, IAssemblyRule {

		private const string VisualBasic = "Microsoft.VisualBasic";

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			// assembly must have an entry point to be examined
			MethodDefinition entry_point = assembly.EntryPoint;
			if (entry_point == null)
				return RuleResult.DoesNotApply;

			// RULE APPLIES

			// we have to check declaringType's visibility so 
			// if we can't get access to it (is this possible?) we abandon
			// also, if it is not public, we don't have to continue our work
			// - we can't reach Main () anyways
			TypeDefinition type = entry_point.DeclaringType.Resolve ();
			if (type == null || !type.IsPublic)
				return RuleResult.Success;

			// at last, if Main () is not public, then it's okay
			if (!entry_point.IsPublic)
				return RuleResult.Success;

			if (assembly.References (VisualBasic)) {
				Runner.Report (type, Severity.Medium, Confidence.High, "Reduce class or module visibility (from public).");
			} else {
				Runner.Report (entry_point, Severity.Medium, Confidence.Total, "Change method visibility to private or internal.");
			}
			return RuleResult.Failure;
		}
	}
}
