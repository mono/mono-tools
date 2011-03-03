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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule checks if you're using <c>lock</c> on the current instance (<c>this</c>) or
	/// on a <c>Type</c>. This can cause
	/// problems because anyone can acquire a lock on the instance or type. And if another
	/// thread does acquire a lock then deadlocks become a very real possibility. The preferred way to
	/// handle this is to create a private <c>System.Object</c> instance field and <c>lock</c> that. This
	/// greatly reduces the scope of the code which may acquire the lock which makes it much easier
	/// to ensure that the locking is done correctly.
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

	[Problem ("This method uses lock(this) or lock(typeof(X)) which makes it very difficult to ensure that the locking is done correctly.")]
	[Solution ("Instead lock a private object so that you have better control of when the locking is done.")]
	public class DoNotLockOnThisOrTypesRule : LockAnalyzerRule {

		private const string LockThis = "Monitor.Enter(this) or lock(this) in C#";
		private const string LockType = "Monitor.Enter(typeof({0})) or lock(typeof({0})) in C#";

		public override void Analyze (MethodDefinition method, MethodReference enter, Instruction ins)
		{
			Instruction locker = ins.TraceBack (method);
			if (locker.OpCode.Code == Code.Dup)
				locker = locker.TraceBack (method);

			string msg = CheckLocker (method, locker);
			if (msg.Length > 0)
				Runner.Report (method, ins, Severity.High, Confidence.High, msg);
		}

		private static string CheckLocker (MethodDefinition method, Instruction ins)
		{
			string msg = String.Empty;

			switch (ins.OpCode.Code) {
			case Code.Ldarg_0:
				if (!method.IsStatic)
					msg = LockThis;
				break;
			case Code.Ldarg:
			case Code.Ldarg_S:
				if (!method.IsStatic) {
					ParameterDefinition pd = (ins.Operand as ParameterDefinition);
					if ((pd == null) || (pd.Index == 0))
						msg = LockThis;
				}
				break;
			case Code.Call:
			case Code.Callvirt:
				MethodReference mr = (ins.Operand as MethodReference);
				if (!mr.ReturnType.IsNamed ("System", "Type"))
					return String.Empty;

				if ((mr.Name == "GetTypeFromHandle") && (mr.DeclaringType.Name == "Type")) {
					// ldtoken
					msg = String.Format (CultureInfo.InvariantCulture, LockType, (ins.Previous.Operand as TypeReference).Name);
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
					return String.Empty;

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
