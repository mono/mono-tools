// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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


using System;
using System.Text;
using System.Xml.Serialization;

namespace Mono.Profiler.Widgets {
	
	[Flags]
	public enum ProfileMode {
		None = 0,
		Allocations = 1 << 0,
		Instrumented = 1 << 1,
		Statistical = 1 << 2,
		HeapSummary = 1 << 3,
		HeapSnaphot = 1 << 4,
	}

	public class ProfileConfiguration {

		public ProfileConfiguration () {}

		string assembly_path;
		[XmlAttribute]
		public string AssemblyPath {
			get { return assembly_path; }
			set { assembly_path = value; }
		}

		ProfileMode mode = ProfileMode.Statistical;
		[XmlAttribute]
		public ProfileMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		bool start_enabled = true;
		[XmlAttribute]
		public bool StartEnabled {
			get { return start_enabled; }
			set { start_enabled = value; }
		}

		public string ToArgs ()
		{
			StringBuilder sb = new StringBuilder ();
			if ((Mode & ProfileMode.Allocations) != 0)
				sb.Append ("alloc");

			if ((Mode & ProfileMode.Instrumented) != 0) {
				if (sb.Length > 0)
					sb.Append (",");
				sb.Append ("calls");
			}

			if ((Mode & ProfileMode.Statistical) != 0) {
				if (sb.Length > 0)
					sb.Append (",");
				sb.Append ("s=128");
			}

			if (!StartEnabled)
				sb.Append (",sd");

			return sb.ToString ();
		}

		public override string ToString ()
		{
			return System.IO.Path.GetFileName (AssemblyPath) + ":" + Mode + ", StartEnabled=" + StartEnabled;
		}

		public override bool Equals (object o)
		{
			if (o is ProfileConfiguration) {
				ProfileConfiguration c = o as ProfileConfiguration;
				return AssemblyPath == c.AssemblyPath && Mode == c.Mode && StartEnabled == c.StartEnabled;
			} else
				return false;
		}

		public override int GetHashCode ()
		{
			return AssemblyPath.GetHashCode () ^ Mode.GetHashCode () ^ StartEnabled.GetHashCode ();
		}
	}
}

