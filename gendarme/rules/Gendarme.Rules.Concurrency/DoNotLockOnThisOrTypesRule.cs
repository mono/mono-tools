//
// Gendarme.Rules.Concurrency.DoNotLockOnThisOrTypesRule
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

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule checks if you're use <c>lock</c> on the current instance (<c>this</c>) or
	/// on a <c>Type</c>. Doing so means potential concurrency troubles. If you are locking 
	/// <c>this</c> anyone else, outside your code/control, could be using a <c>lock</c> on 
	/// your instance causing a deadlock. Locking on types is also bad since there is 
	/// only one instance of each <c>Type</c>. Again anyone else, outside your code/control,
	/// could be locking on it. The best locking is to create your own, private, instance
	/// <c>System.Object</c> and <c>lock</c> on it.
	/// </summary>
	/// <example>
	/// Bad example (this):
	/// <code>
	/// public void MethodLockingOnThis ()
	/// {
	/// 	lock (this) {
	///		producer++;
	///     }	
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (type):
	/// <code>
	/// public void MethodLockingOnType ()
	/// {
	/// 	lock (this.GetType ()) {
	///		producer++;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class ClassWithALocker {
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

	[Problem ("This method use a lock(this) or lock(typeof(X)) construct which is often used incorrectly.")]
	[Solution ("To be safe from outside always lock on something that is totally private to your code.")]
	public class DoNotLockOnThisOrTypesRule : LockAnalyzerRule {

		private const string LockThis = "Monitor.Enter(this) or lock(this) in C#";
		private const string LockType = "Monitor.Enter(typeof({0})) or lock(typeof({0})) in C#";

		public override void Analyze (MethodDefinition method, Instruction ins)
		{
			Instruction locker = ins.TraceBack (method);
			if (locker.OpCode.Code == Code.Dup)
				locker = locker.TraceBack (method);

			string msg = CheckLocker (method, locker);
			if (msg != null)
				Runner.Report (method, ins, Severity.High, Confidence.High, msg);
		}

		private static string CheckLocker (MethodDefinition method, Instruction ins)
		{
			string msg = null;

			switch (ins.OpCode.Code) {
			case Code.Ldarg_0:
				msg = LockThis;
				break;
			case Code.Ldarg:
			case Code.Ldarg_S:
				ParameterDefinition pd = (ins.Operand as ParameterDefinition);
				if ((pd == null) || (pd.Sequence != 0))
					msg = LockThis;
				break;
			case Code.Call:
			case Code.Callvirt:
				MethodReference mr = (ins.Operand as MethodReference);
				if (mr.ReturnType.ReturnType.FullName != "System.Type")
					return null;

				if ((mr.Name == "GetTypeFromHandle") && (mr.DeclaringType.Name == "Type")) {
					// ldtoken
					msg = String.Format (LockType, (ins.Previous.Operand as TypeReference).Name);
				} else {
					msg = mr.ToString ();
				}
				break;
			default:
				// [g]mcs commonly do a stloc.x ldloc.x just before 
				// (a) an ldarg.0 (for instance methods); or
				// (b) an ldtoken (for static methods)
				// and this throws off TraceBack
				Instruction locker = StoreLoadLocal (method, ins);
				if (locker == null)
					return null;

				return CheckLocker (method, locker);
			}
			return msg;
		}

		private static Instruction StoreLoadLocal (MethodDefinition method, Instruction ins)
		{
			// check for a STLOC followed by a LDLOC
			if (!ins.IsLoadLocal () || !ins.Previous.IsStoreLocal ())
				return null;
			// make sure it's about the same local variable
			if (ins.GetVariable (method) != ins.Previous.GetVariable (method))
				return null;
			return ins.Previous.Previous;
		}
	}
}
