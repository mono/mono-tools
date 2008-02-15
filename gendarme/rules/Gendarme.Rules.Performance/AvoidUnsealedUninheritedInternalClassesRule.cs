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

	[Problem ("Due to performance issues, types which are not visible outside of the assembly and which have no inherited types within the assembly should be sealed.")]
	[Solution ("You should seal this type, unless you plan to inherit from this type in the near-future.")]
	public class AvoidUnsealedUninheritedInternalClassesRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsAbstract || type.IsSealed || type.IsVisible () || type.IsGeneratedCode ())
				return RuleResult.Success;

			foreach (TypeDefinition type_definition in type.Module.Types) {
				// skip ourself
				if (type_definition.FullName == type.FullName)
					continue;
				if (type_definition.Inherits (type.FullName))
					return RuleResult.Success;
			}
			Runner.Report (type, Severity.High, Confidence.High, "Types which are not visible outside the assembly and without inherited types within the assembly should be sealed");
			return RuleResult.Failure;
		}
	}
}
