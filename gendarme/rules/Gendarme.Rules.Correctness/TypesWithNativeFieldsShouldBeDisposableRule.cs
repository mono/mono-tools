//
// Gendarme.Rules.Correctness.TypesWithNativeFieldsShouldBeDisposableRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

	/// <summary>
	/// This rule will fire if a type contains <c>IntPtr</c>, <c>UIntPtr</c>, or 
	/// <c>HandleRef</c> fields but does not implement <c>System.IDisposable</c>.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// public class DoesNotImplementIDisposable {
	///	IntPtr field;
	/// }
	/// 
	/// abstract public class AbstractDispose : IDisposable {
	///	IntPtr field;
	///	
	///	// the field should be disposed in the type that declares it
	///	public abstract void Dispose ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Dispose : IDisposable {
	///	IDisposable field;
	///	
	///	public void Dispose ()
	///	{
	///		UnmanagedFree (field);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains native field(s) but doesn't implement IDisposable.")]
	[Solution ("Implement IDisposable and free the native field(s) in the Dispose method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class TypesWithNativeFieldsShouldBeDisposableRule : TypesShouldBeDisposableBaseRule {

		static OpCodeBitmask StoreFieldBitmask = new OpCodeBitmask (0x0, 0x400000000000000, 0x80000000, 0x0);

		protected override string AbstractTypeMessage { 
			get { return "Field is native. Type should implement a non-abstract Dispose() method"; }
		}

		protected override string TypeMessage { 
			get { return "Field is native. Type should implement a Dispose() method";  }
		}

		protected override string AbstractDisposeMessage { 
			get { return "Some fields are native pointers. Making this method abstract shifts the reponsability of disposing those fields to the inheritors of this class."; }
		}

		protected override void CheckMethod (MethodDefinition method, bool abstractWarning)
		{
			if ((method == null) || !method.HasBody)
				return;

			OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
			// method must have a CALL[VIRT] and either STFLD or STELEM_REF
			if (!bitmask.Intersect (OpCodeBitmask.Calls) || !bitmask.Intersect (StoreFieldBitmask))
				return;

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = (ins.Operand as MethodReference);
				if (mr == null || mr.DeclaringType.IsNative ())
					continue;

				FieldDefinition field = null;
				Instruction next = ins.Next;
				if (next.Is (Code.Stfld)) {
					field = next.Operand as FieldDefinition;
				} else if (next.Is (Code.Stobj) || next.Is (Code.Stind_I)) {
					Instruction origin = next.TraceBack (method);
					if (origin.Is (Code.Ldelema)) {
						origin = origin.TraceBack (method);
						if (origin != null)
							field = origin.Operand as FieldDefinition;
					}
				}

				if (field != null && FieldCandidates.Contains (field)) {
					Runner.Report (field, Severity.High, Confidence.High,
						abstractWarning ? AbstractTypeMessage : TypeMessage);
				}
			}
		}

		protected override bool FieldTypeIsCandidate (TypeDefinition type)
		{
			return ((type != null) && type.IsNative ());
		}
	}
}
