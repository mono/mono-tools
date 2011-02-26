//
// Gendarme.Rules.Correctness.FloatComparisonRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	[EngineDependency (typeof (OpCodeEngine))]
	abstract public class FloatingComparisonRule : Rule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we want to avoid checking all methods if the module doesn't refer to either
			// System.Single or System.Double (big performance difference)
			// note: mscorlib.dll is an exception since it defines, not refer, System.Single and Double
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib") ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsFloatingPoint ();
					});
			};
		}

		static protected bool IsApplicable (MethodDefinition method)
		{
			// we only check methods with a body
			if ((method == null) || !method.HasBody)
				return false;

			// we don't check System.Single and System.Double
			// special case for handling mscorlib.dll
			if (method.DeclaringType.IsFloatingPoint ())
				return false;

			// rule applies only if the method contains Call[virt] (calls to Equals)
			OpCodeBitmask mask = OpCodeEngine.GetBitmask (method);
			if (OpCodeBitmask.Calls.Intersect (mask))
				return true;
			// *or* Ceq instructions
			return mask.Get (Code.Ceq);
		}
	}
}
