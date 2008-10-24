//
// Gendarme.Rules.Concurrency.DoNotUseMethodImplOptionsSynchronizedRule
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule checks if some methods are decorated with <c>[MethodImpl(MethodImplOptions.Synchronized)]</c>.
	/// The runtime synchronize those methods automatically using a <c>lock(this)</c> for
	/// instance methods or a <c>lock(typeof(X))</c> for static methods. This can cause
	/// deadlocks if misused inside your own code or if the method is visible outside the
	/// assembly since anyone else, outside your control, could be using a <c>lock</c> on 
	/// an instance or the type and cause a deadlock. The best locking is to create your own, 
	/// private, instance <c>System.Object</c> and <c>lock</c> on it.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [MethodImpl (MethodImplOptions.Synchronized)]
	/// public void SychronizedMethod ()
	/// {
	///	producer++;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class ClassWithALocker {
	/// 	object locker = new object ();
	///	int producer = 0;
	/// 
	///	public void MethodLockingLocker ()
	///	{
	///		lock (locker) {
	///			producer++;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method is decorated with [MethodImpl(MethodImplOptions.Synchronized)].")]
	[Solution ("Remove the attribute and provide your own lock private to your code.")]
	public class DoNotUseMethodImplOptionsSynchronizedRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsSynchronized)
				return RuleResult.Success;

			// special case since the compiler generate add/remove on events using Synchronized
			if (method.IsAddOn || method.IsRemoveOn)
				return RuleResult.DoesNotApply;

			// base severity on whether the method is visible or not
			// if not then the potential problem is limited to the assembly
			Severity severity = method.IsVisible () ? Severity.High : Severity.Medium;
			Runner.Report (method, severity, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
