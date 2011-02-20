// 
// Gendarme.Rules.Performance.AvoidUnsealedUninheritedInternalClassesRule
//
// Authors:
//	Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 Scott Peterson
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
	/// This rule will fire for classes which are internal to the assembly and have no derived
	/// classes, but are not <c>sealed</c>. Sealing the type clarifies the type hierarchy and
	/// allows the compiler/JIT to perform optimizations such as eliding virtual method calls.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // this one is correct since MyInheritedStuff inherits from this class
	/// internal class MyBaseStuff {
	/// }
	/// 
	/// // this one is bad, since no other class inherit from MyConcreteStuff
	/// internal class MyInheritedStuff : MyBaseStuff {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// // this one is correct since the class is abstract
	/// internal abstract class MyAbstractStuff {
	/// }
	/// 
	/// // this one is correct since the class is sealed
	/// internal sealed class MyConcreteStuff : MyAbstractStuff {
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0 and, before 2.2, was named AvoidUnsealedUninheritedInternalClassesRule</remarks>

	[Problem ("Due to performance issues, types which are not visible outside of the assembly and which have no derived types should be sealed.")]
	[Solution ("You should seal this type, unless you plan to inherit from this type in the near-future.")]
	public class AvoidUnsealedUninheritedInternalTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsAbstract || type.IsSealed || type.IsVisible () || type.IsGeneratedCode ())
				return RuleResult.Success;

			ModuleDefinition module = type.Module;
			string name = type.Name;
			string nspace = type.Namespace;
			foreach (TypeDefinition type_definition in module.GetAllTypes ()) {
				// skip ourself
				if (type_definition.IsNamed (nspace, name))
					continue;
				if (type_definition.Inherits (nspace, name))
					return RuleResult.Success;
			}

			Confidence c = module.Assembly.HasAttribute ("System.Runtime.CompilerServices", "InternalsVisibleToAttribute") ?
				Confidence.High : Confidence.Total;
			Runner.Report (type, Severity.Medium, c);
			return RuleResult.Failure;
		}
	}
}
