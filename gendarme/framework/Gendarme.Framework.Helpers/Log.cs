// 
// Gendarme.Framework.Helpers.Log
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Cecil;
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Wrapper around System.Diagnostics.Debug.
	/// </summary>
	/// <remarks>
	/// Instead of adding temporary Console.WriteLines to your rules use this
	/// class instead. This way we can leave the debugging code in the rule
	/// and enable it on a rule by rule basis (using the bin/gendarme.exe.config
	/// file). Usage is like this:
	///
	/// <code>
	/// Log.WriteLine (this, "value: {0}", value);	// this will normally be a rule instance
	/// Log.WriteLine ("DefineAZeroValueRule", "hey");	// should rarely be used
	/// Log.WriteLine ("DefineAZeroValueRule.Details", "hey");	// convention for additional output
	/// </code>
	///
	/// </remarks>
	public static class Log {
		private static Dictionary<string, bool> enabled = new Dictionary<string, bool> ();
#if false
		// Write (T)
		[Conditional ("DEBUG")]
		public static void Write<T> (T category, string format, params object[] args)
		{
			string name = typeof (T).Name;
			if (IsEnabled (name))
				Debug.Write (string.Format (format, args));
		}
		
		// Write (string)
		[Conditional ("DEBUG")]
		public static void Write (string category, string format, params object[] args)
		{
			if (IsEnabled (category))
				Debug.Write (string.Format (format, args));
		}
#endif
		// WriteLine (T)
		[Conditional ("DEBUG")]
		public static void WriteLine<T> (T category)
		{
			WriteLine (typeof (T).Name);
		}
	
		[Conditional ("DEBUG")]
		public static void WriteLine<T> (T category, string format, params object[] args)
		{
			WriteLine (typeof (T).Name, format, args);
		}
		
		[Conditional ("DEBUG")]
		public static void WriteLine<T> (T category, MemberReference member)
		{
			WriteLine (typeof (T).Name, member);
		}
		
		// WriteLine (string)
		[Conditional ("DEBUG")]
		public static void WriteLine (string category)
		{
			if (IsEnabled (category))
				Debug.WriteLine (string.Empty);
		}
	
		[Conditional ("DEBUG")]
		public static void WriteLine (string category, string format, params object[] args)
		{
			if (IsEnabled (category))
				Debug.WriteLine (string.Format (format, args));
		}
		
		[Conditional ("DEBUG")]
		public static void WriteLine (string category, MemberReference member)
		{
			if (IsEnabled (category)) {
				MethodDefinition md = (member as MethodDefinition);
				if (md != null)
					Debug.WriteLine (new MethodPrinter (md).ToString ());
				else
					Debug.WriteLine (member.GetFullName ());
			}
		}
		
		// Misc
		[Conditional ("DEBUG")]
		public static void Indent ()
		{
			Debug.Indent ();
		}
		
		[Conditional ("DEBUG")]
		public static void Unindent ()
		{
			Debug.Unindent ();
		}
				
		public static bool IsEnabled (string category)
		{
			bool enable;
						
			if (!enabled.TryGetValue (category, out enable)) {
				enable = new BooleanSwitch (category, string.Empty).Enabled;
				enabled.Add (category, enable);
			}
						
			return enable;
		}
	}
}
