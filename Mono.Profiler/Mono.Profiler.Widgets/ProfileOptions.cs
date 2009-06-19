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
using System.Collections.Generic;

namespace Mono.Profiler.Widgets {
	
	public class DisplayOptions	{

		static string[] system_libs = new string[] {
			"mscorlib", "System", "System.Xml",
			"glib-sharp", "pango-sharp", "atk-sharp", "gdk-sharp", "gtk-sharp", "glade-sharp", "gtk-dotnet"
		};

		List<string> filters = new List<string> ();
		internal List<string> Filters {
			get {
				List<string> result = new List<string> ();
				result.AddRange ((string[])filters.ToArray ());
				if (!show_system_nodes)
					result.AddRange (system_libs);
				return result;
			}
		}

		bool show_system_nodes;
		public bool ShowSystemNodes {
			get { return show_system_nodes; }
			set {
				if (show_system_nodes == value)
					return;
				show_system_nodes = value;
				OnChanged ();
			}
		}
		
		public event EventHandler Changed;
		
		void OnChanged ()
		{
			if (Changed == null)
				return;
			Changed (this, EventArgs.Empty);
		}
	}
}
