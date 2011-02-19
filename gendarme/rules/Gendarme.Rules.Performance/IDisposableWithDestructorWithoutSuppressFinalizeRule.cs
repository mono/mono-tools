//
// Gendarme.Rules.Performance.UseSuppressFinalizeOnIDisposableTypeWithFinalizerRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
	/// This rule will fire if a type implements <c>System.IDisposable</c> and has a finalizer
	/// (called a destructor in C#), but the Dispose method does not call <c>System.GC.SuppressFinalize</c>.
	/// Failing to do this should not cause properly written code to fail, but it does place a non-trivial
	/// amount of extra pressure on the garbage collector and on the finalizer thread.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class BadClass : IDisposable {
	/// 	~BadClass ()
	/// 	{
	/// 		Dispose (false);
	/// 	}
	/// 	
	/// 	public void Dispose ()
	/// 	{
	/// 		// GC.SuppressFinalize is missing so the finalizer will be called
	/// 		// which puts needless extra pressure on the garbage collector.
	/// 		Dispose (true);
	/// 	}
	/// 	
	/// 	private void Dispose (bool disposing)
	/// 	{
	/// 		if (ptr != IntPtr.Zero) {
	/// 			Free (ptr);
	/// 			ptr = IntPtr.Zero;
	/// 		}
	/// 	}
	/// 	
	/// 	[DllImport ("somelib")]
	/// 	private static extern void Free (IntPtr ptr);
	/// 	
	/// 	private IntPtr ptr;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class GoodClass : IDisposable {
	/// 	~GoodClass ()
	/// 	{
	/// 		Dispose (false);
	/// 	}
	/// 	
	/// 	public void Dispose ()
	/// 	{
	/// 		Dispose (true);
	/// 		GC.SuppressFinalize (this);
	/// 	}
	/// 	
	/// 	private void Dispose (bool disposing)
	/// 	{
	/// 		if (ptr != IntPtr.Zero) {
	/// 			Free (ptr);
	/// 			ptr = IntPtr.Zero;
	/// 		}
	/// 	}
	/// 	
	/// 	[DllImport ("somelib")]
	/// 	private static extern void Free (IntPtr ptr);
	/// 	
	/// 	private IntPtr ptr;
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named IDisposableWithDestructorWithoutSuppressFinalizeRule</remarks>

	[Problem ("The type has a finalizer and implements IDisposable, but Dispose does not call System.GC.SuppressFinalize.")]
	[Solution ("Add a call to GC.SuppressFinalize inside your Dispose method.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseSuppressFinalizeOnIDisposableTypeWithFinalizerRule : Rule, ITypeRule {

		private bool Recurse (MethodDefinition method, int level)
		{
			// some methods have no body (e.g. p/invokes, icalls)
			if ((method == null) || !method.HasBody)
				return false;

			// don't iterate the IL unless we know there are some call[virt] inside them
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return false;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					// are we calling GC.SuppressFinalize ?
					MethodReference callee = (ins.Operand as MethodReference);
					if (callee.IsNamed ("System", "GC", "SuppressFinalize")) {
						return true;
					} else if (level < 3) {
						if (Recurse (callee.Resolve (), level + 1))
							return true;
					}
					break;
				}
			}
			return false;
		}

		private void CheckDispose (MethodDefinition dispose)
		{
			if ((dispose != null) && !Recurse (dispose, 0)) {
				Runner.Report (dispose, Severity.High, Confidence.Normal);
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums, interfaces and delegates
			if (type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// rule applies to types that implements System.IDisposable
			if (!type.Implements ("System", "IDisposable"))
				return RuleResult.DoesNotApply;

			// and provide a finalizer
			if (type.GetMethod (MethodSignatures.Finalize) == null)
				return RuleResult.DoesNotApply;

			// rule applies!

			// look if GC.SuppressFinalize is being called in the Dispose methods
			// or one of the method it calls

			CheckDispose (type.GetMethod (MethodSignatures.Dispose));
			CheckDispose (type.GetMethod (MethodSignatures.DisposeExplicit));

			return Runner.CurrentRuleResult;
		}
	}
}
