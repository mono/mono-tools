//
// Gendarme.Rules.Maintainability.AvoidComplexMethodsRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// 	(C) 2008 Cedric Vivier
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	[Problem ("Methods with a cyclomatic complexity equal or greater than 25 are harder to understand and maintain.")]
	[Solution ("You should apply an Extract Method refactoring, but there are other solutions.")]
	public class AvoidComplexMethodsRule : Rule,IMethodRule {

		// defaults match fxcop rule http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1575061&SiteID=1
		// so people using both tools should not see conflicting results
		private int success = 25;
		private int low = 0;
		private int medium = 0;
		private int high = 0;

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// works if only SuccessThreshold is configured in rules.xml
			if (low == 0)
				low = success * 2;
			if (medium == 0)
				medium = success * 3;
			if (high == 0)
				high = success * 4;
		}

		public int SuccessThreshold {
			get { return success; }
			set { success = value; }
		}

		public int LowThreshold {
			get { return low; }
			set { low = value; }
		}

		public int MediumThreshold {
			get { return medium; }
			set { medium = value; }
		}

		public int HighThreshold {
			get { return high; }
			set { high = value; }
		}
	
		public RuleResult CheckMethod (MethodDefinition method)
		{
			//does rule apply?
			if (!method.HasBody || method.IsGeneratedCode () || method.IsCompilerControlled)
				return RuleResult.DoesNotApply;

			//yay! rule do apply!
			int cc = GetCyclomaticComplexityForMethod(method);
			if (cc < SuccessThreshold)
				return RuleResult.Success;

			//how's severity?
			Severity sev = GetCyclomaticComplexitySeverity(cc);

			string s = (Runner.VerbosityLevel < 2) ? String.Empty :
					String.Format ("Method's cyclomatic complexity : {0}.", cc);

			Runner.Report (method, sev, Confidence.High, s);
			return RuleResult.Failure;
		}

		public Severity GetCyclomaticComplexitySeverity(int cc)
		{
			// 25 <= CC < 50 is not good but not catastrophic either
			if (cc < LowThreshold)
				return Severity.Low;
			// 50 <= CC < 75 this should be refactored asap
			if (cc < MediumThreshold)
				return Severity.Medium;
			// 75 <= CC < 100 this SHOULD be refactored asap
			if (cc < HighThreshold)
				return Severity.High;
			// CC > 100, don't touch it since it may become a classic in textbooks 
			// anyway probably no one can understand it ;-)
			return Severity.Critical;
		}

		public static int GetCyclomaticComplexityForMethod(MethodDefinition method)
		{
			if (!method.HasBody) return 1;

			int cc = 1;

			foreach (Instruction inst in method.Body.Instructions)
			{
				if (FlowControl.Branch == inst.OpCode.FlowControl)
				{
					//detect ternary pattern
					if (null != inst && null != inst.Previous && inst.Previous.OpCode.Name.StartsWith("ld"))
						cc++;
				}
				if (FlowControl.Cond_Branch != inst.OpCode.FlowControl)
				{
					continue;
				}

				if (OpCodes.Switch == inst.OpCode)
				{
					cc += GetNumberOfSwitchTargets(inst);
				}
				else //'normal' conditional branch
				{
					cc++;
				}
			}

			return cc;
		}

		private static int GetNumberOfSwitchTargets(Instruction inst)
		{
			List<Instruction> targets = new List<Instruction> ();
			foreach (Instruction target in ((Instruction[]) inst.Operand))
			{
				if (!targets.Contains (target))
				{
					targets.Add (target);
				}
			}
			int nTargets = targets.Count;
			//detect 'default' branch
			if (FlowControl.Branch == inst.Next.OpCode.FlowControl)
			{
				if (inst.Next.Operand != FindFirstUnconditionalBranchTarget (targets[0]))
				{
					nTargets++;
				}
			}
			return nTargets;
		}

		private static Instruction FindFirstUnconditionalBranchTarget(Instruction inst)
		{
			while (null != inst)
			{
				if (FlowControl.Branch == inst.OpCode.FlowControl)
				{
					return ((Instruction) inst.Operand);
				}
				inst = inst.Next;
			}
			return null;
		}

	}

}
