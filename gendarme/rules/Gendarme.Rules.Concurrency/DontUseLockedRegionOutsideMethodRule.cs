//
// Gendarme.Rules.Concurrency.DontUseLockedRegionOutsideMethod.cs: 
//	looks for methods that enter an exclusive region but do not exit
//	(this can imply deadlocks, or just a bad practice).
//
// Authors:
//	Andres G. Aragoneses <aaragoneses@novell.com>
//
// Copyright (C) 2008 Andres G. Aragoneses
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule will fire if a method calls <c>System.Threading.Monitor.Enter</c>, 
	/// but not <c>System.Threading.Monitor.Exit</c>. This is a bad idea for public
	/// methods because the callers must (indirectly) manage a lock which they do not
	/// own. This increases the potential for problems such as dead locks because 
	/// locking/unlocking may not be done together, the callers must do the unlocking
	/// even in the presence of exceptions, and it may not be completely clear that
	/// the public method is acquiring a lock without releasing it.
	///
	/// This is less of a problem for private methods because the lock is managed by
	/// code that owns the lock. So, it's relatively easy to analyze the class to ensure
	/// that the lock is locked and unlocked correctly and that any invariants are 
	/// preserved when the lock is acquired and after it is released. However it is
	/// usually simpler and more maintainable if methods unlock whatever they lock.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class BadExample {
	/// 	int producer = 0;
	/// 	object lock = new object();
	///
	/// 	// This class is meant to be thread safe, but in the interests of
	/// 	// performance it requires clients to manage its lock. This allows
	/// 	// clients to grab the lock, batch up edits, and release the lock
	/// 	// when they are done. But this means that the clients must
	/// 	// now (implicitly) manage the lock which is problematic, especially
	/// 	// if this object is shared across threads.
	/// 	public void BeginEdits ()
	/// 	{
	///		Monitor.Enter (lock);
	///	}
	///
	/// 	public void AddProducer ()
	/// 	{
	/// 		// Real code would either assert or throw if the lock is not held. 
	///		producer++;
	///	}
	///
	/// 	public void EndEdits ()
	/// 	{
	///		Monitor.Exit (lock);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class GoodExample {
	/// 	int producer = 0;
	/// 	object mutex = new object();
	///	
	///	public void AddProducer ()
	///	{
	/// 		// We need a try block in case the assembly is compiled with
	/// 		// checked arithmetic.
	///		Monitor.Enter (mutex);
	/// 		try {
	///			producer++;
	/// 		}
	///		finally {
	///			Monitor.Exit (mutex);
	/// 		}
	///	}
	///	
	///	public void AddProducer2 ()
	///	{
	/// 		// Same as the above, but with C# sugar.
	///		lock (mutex) {
	///			producer++;
	/// 		}
	///	}
	/// }
	/// </code>
	/// </example>

	// TODO: do a rule that checks if Monitor.Enter is used *before* Exit (dumb code, I know)
	// TODO: do a more complex rule that checks that you have used Thread.Monitor.Exit in a finally block
	[Problem ("This method uses Thread.Monitor.Enter() but doesn't use Thread.Monitor.Exit().")]
	[Solution ("Prefer the lock{} statement when using C# or redesign the code so that Monitor.Enter and Exit are called together.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotUseLockedRegionOutsideMethodRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// is this module using Monitor.Enter/Exit ? (lock in c#)
			// if not then this rule does not need to be executed for the module
			// note: mscorlib.dll is an exception since it defines, not refer, System.Threading.Monitor
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Threading", "Monitor");
					}));
			};
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			int enter = 0;
			int exit = 0;
			
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				MethodReference m = (ins.Operand as MethodReference);
				if (m == null)
					continue;

				if (m.IsNamed ("System.Threading", "Monitor", "Enter")) {
					enter++;
				} else if (m.IsNamed ("System.Threading", "Monitor", "Exit")) {
					exit++;
				}
			}
			
			if (enter == exit)
				return RuleResult.Success;

			Runner.Report (method, Severity.High, Confidence.Normal);
			return RuleResult.Failure;
		}
	}
}
