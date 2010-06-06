//
// Abstract Gendarme.Rules.BadPractice.ReturnsNullRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	// Notes: 
	// * We don't implement IMethodRule on purpose since a rule that inherit
	//   from us can be an ITypeRule (checking for a specific method in the type)

	[EngineDependency (typeof (OpCodeEngine))]
	abstract public class ReturnNullRule : Rule {

		void CheckReturn (Instruction ins, MethodDefinition method)
		{
			// trace back what is being returned
			Instruction previous = ins.TraceBack (method);
			while (previous != null) {
				// most of the time we'll find the null value on the first trace back call
				if (previous.OpCode.Code == Code.Ldnull) {
					Report (method, ins);
					break;
				}

				// but CSC non-optimized code generation results in strange IL that needs a few 
				// more calls. e.g. "return null" == "nop | ldnull | stloc.0 | br.s | ldloc.0 | ret"
				if ((previous.OpCode.FlowControl == FlowControl.Branch) || (previous.IsLoadLocal ()
					|| previous.IsStoreLocal ())) {
					previous = previous.TraceBack (method);
				} else
					break;
			}
		}

		void CheckLdnull (Instruction ins, MethodDefinition method)
		{
			Instruction branch = (ins.Next.Operand as Instruction);
			if (branch.Is (Code.Ret))
				Report (method, ins);
		}

		public virtual RuleResult CheckMethod (MethodDefinition method)
		{
			// is there any Ldnull instructions in this method
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Ldnull))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Ret:
					// look if a null is returned
					CheckReturn (ins, method);
					break;
				case Code.Ldnull:
					// look for branching, e.g. immediate if
					CheckLdnull (ins, method);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}

		protected abstract void Report (MethodDefinition method, Instruction ins);
	}
}
