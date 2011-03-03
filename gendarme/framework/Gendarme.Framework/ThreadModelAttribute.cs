//
// Gendarme.Framework.ThreadModelAttribute class
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// 	(C) 2009 Jesse Jones
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

// Note that these types are extended version of what we recommend
// that users use. See DecorateThreadsRule documentation for a smaller version
namespace Gendarme.Framework {

	/// <summary>Used with <see cref = "ThreadModelAttribute"/>.</summary>
	[Serializable]
	public enum ThreadModel {
		/// <summary>The code may run safely only under the main thread.</summary>
		/// <remarks>This is the default for code in the assemblies being checked.</remarks>
		MainThread = 0x0000,
		
		/// <summary>The code may run under a single arbitrary thread.</summary>
		SingleThread = 0x0001,
		
		/// <summary>The code may run under multiple threads, but only if the 
		/// execution is serialized (e.g. by user level locking).</summary>
		Serializable = 0x0002,
		
		/// <summary>The code may run under multiple threads concurrently without user 
		/// locking.</summary>
		/// <remarks>This is the default for code in the System/Mono namespaces.</remarks>
		Concurrent = 0x0003,
		
		/// <summary>Or this with the above for the rare cases where the code cannot be
		/// shown to be correct using a static analysis.</summary>
		AllowEveryCaller = 0x0008,
	}
	
	/// <summary>Used to precisely specify the threading semantics of code.</summary>
	[Serializable]
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
	AttributeTargets.Interface | AttributeTargets.Delegate |
	AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property,
	AllowMultiple = false, Inherited = false)]
	public sealed class ThreadModelAttribute : Attribute, IEquatable<ThreadModelAttribute> {

		public ThreadModelAttribute (ThreadModel model)
		{
			ThreadModel value = model & ~ThreadModel.AllowEveryCaller;
			if ((value < ThreadModel.MainThread) || (value > ThreadModel.Concurrent))
				throw new ArgumentException (model.ToString () + " is not a valid ThreadModel value.");
			
			Model = model & (ThreadModel) 0x0007;
			AllowsEveryCaller = (model & ThreadModel.AllowEveryCaller) != 0;
		}
				
		public ThreadModel Model { get; set; }
		
		public bool AllowsEveryCaller { get; set; }
		
		#region Overrides and Operators
		public override string ToString ()
		{
			if (AllowsEveryCaller)
				return String.Format (CultureInfo.InvariantCulture, "{0} | AllowEveryCaller", Model);
				
			return Model.ToString ();
		}
		
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			
			ThreadModelAttribute rhs = obj as ThreadModelAttribute;
			return this == rhs;
		}
		
		public bool Equals (ThreadModelAttribute other)
		{
			return this == other;
		}
		
		public static bool operator== (ThreadModelAttribute lhs, ThreadModelAttribute rhs)
		{
			if (object.ReferenceEquals (lhs, rhs))
				return true;
			
			if ((object) lhs == null || (object) rhs == null)
				return false;
			
			if (lhs.Model != rhs.Model)
				return false;
			
			if (lhs.AllowsEveryCaller != rhs.AllowsEveryCaller)
				return false;
			
			return true;
		}
		
		public static bool operator!= (ThreadModelAttribute lhs, ThreadModelAttribute rhs)
		{
			return !(lhs == rhs);
		}
		
		public override int GetHashCode ()
		{
			int hash = 0;
			
			unchecked {
				hash += Model.GetHashCode ();
				hash += AllowsEveryCaller.GetHashCode ();
			}
			
			return hash;
		}
		#endregion
	}
}
