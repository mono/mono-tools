//
// Gendarme.Rules.Performance.AvoidLocalDataStoreSlotRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
	/// This rule warns if a method use <c>LocalDataStoreSlot</c> to store or
	/// retrieve data from Thread or Context Local Storage. The faster alternative
	/// is to use <c>[ThreadStatic]</c> or <c>[ContextStatic]</c> attributes to avoid
	/// extra calls and typecasts. 
	/// Also <c>[ThreadStatic]</c> is available on Silverlight while the
	/// <c>LocalDataStoreSlot</c> API are not.
	/// </summary>
	/// <example>
	/// Bad example (Thread Local Storage):
	/// <code>
	/// static void SetSharedKey (byte[] key)
	/// {
	///	LocalDataStoreSlot lds = Thread.AllocateNamedDataSlot ("shared-key");
	///	lds.SetData (key.Clone ());
	/// }
	/// 
	/// public byte[] SignData (byte[] data)
	/// {
	///	LocalDataStoreSlot lds = Thread.GetNamedDataSlot ("shared-key");
	///	using (HMACSHA1 hmac = new HMACSHA1 (Thread.GetData (lds) as byte[])) {
	///		return hmac.ComputeHash (data);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (Thread Local Storage):
	/// <code>
	/// [ThreadStatic]
	/// static byte[] shared_key;
	/// 
	/// static void SetSharedKey (byte[] key)
	/// {
	///	shared_key = (byte[]) key.Clone ();
	/// }
	/// 
	/// public byte[] SignData (byte[] data)
	/// {
	///	using (HMACSHA1 hmac = new HMACSHA1 (shared_key)) {
	///		return hmac.ComputeHash (data);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("LocalDataStoreSlot is slower than the [ThreadStatic] or [ContextStatic] alternatives.")]
	[Solution ("Change your code to use [ThreadStatic] or [ContextStatic] attributes.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidLocalDataStoreSlotRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				// if the module does not reference System.LocalDataStoreSlot
				// then no method inside it will be using it
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System", "LocalDataStoreSlot");
					}));
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction current in method.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference mr = (current.Operand as MethodReference);
					if (mr == null)
						continue;

					TypeReference type = mr.DeclaringType;
					switch (type.Namespace) {
					case "System.Threading":
					case "System.Runtime.Remoting.Contexts":
						break;
					default:
						continue;
					}

					switch (type.Name) {
					case "Thread":
					case "Context":
						break;
					default:
						continue;
					}

					switch (mr.Name) {
					case "AllocateDataSlot":
					case "AllocateNamedDataSlot":
					case "FreeNamedDataSlot":
					case "GetNamedDataSlot":
					case "GetData":
					case "SetData":
						break;
					default:
						continue;
					}

					Runner.Report (method, current, Severity.High, Confidence.Total);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

