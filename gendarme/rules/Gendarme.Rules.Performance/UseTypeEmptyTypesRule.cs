//
// Gendarme.Rules.Performance.UseTypeEmptyTypesRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule fires if a zero length array of <c>System.Type</c> is created.
	/// This value is so often required by the framework API that the <c>System.Type</c> includes 
	/// an <c>EmptyTypes</c> field. Using this field avoids the memory allocation (and GC tracking)
	/// of your own array.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// ConstructorInfo ci = type.GetConstructor (new Type[0]);
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// ConstructorInfo ci = type.GetConstructor (Type.EmptyTypes);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The method creates an empty System.Type array instead of using Type.EmptyTypes.")]
	[Solution ("Use Type.EmptyTypes instead of creating your own array.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseTypeEmptyTypesRule : Rule, IMethodRule {

		static OpCodeBitmask zero = new OpCodeBitmask (0x180400000, 0x0, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// check if the method creates array (Newarr)
			OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
			if (!bitmask.Get (Code.Newarr))
				return RuleResult.DoesNotApply;
			// check if the method loads the 0 constant
			if (!bitmask.Intersect (zero))
				return RuleResult.DoesNotApply;

			// look for array of Type creation
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode != OpCodes.Newarr)
					continue;

				if (!(ins.Operand as TypeReference).IsNamed ("System", "Type"))
					continue;

				if (ins.Previous.IsOperandZero ())
					Runner.Report (method, ins, Severity.Medium, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask zero = new OpCodeBitmask ();
			zero.Set (Code.Ldc_I4);
			zero.Set (Code.Ldc_I4_S);
			zero.Set (Code.Ldc_I4_0);
			Console.WriteLine (zero);
		}
#endif
	}
}
