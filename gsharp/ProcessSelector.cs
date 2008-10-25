// ProcessSelector.cs
//Authors: ${Author}
//
// Copyright (c) 2008 [copyright holders]
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
//

using System;
using System.Diagnostics;
using Gtk;
using Mono.Unix.Native;

namespace Mono.CSharp.Gui
{
	
	public partial class ProcessSelector : Gtk.Dialog
	{
		TreeStore procstore;
		int pid = -1;
		
		public ProcessSelector()
		{
			this.Build();

			CellRendererText t = new CellRendererText ();
			treeview.AppendColumn ("Process", t, "text", 0);

			treeview.Selection.Changed += delegate {
				TreeSelection ts = treeview.Selection;
				TreeIter iter;
				TreeModel mod;
				if (ts.GetSelected (out mod, out iter)) {
					string proc = mod.GetValue (iter, 0) as string;
					int r;
					
					if (Int32.TryParse (proc.Substring (0, proc.IndexOf ('/')), out r))
						pid = r;
				}
			};
			
			procstore = new TreeStore (typeof (String));
			UpdateTreeView ();			
			treeview.Model = procstore;
			
			UpdateTreeView ();
		}

		void UpdateTreeView ()
		{
			var a = new PerformanceCounterCategory ("Mono Memory");

			procstore.Clear ();
			string thispid = Syscall.getpid ().ToString ();
			
			foreach (string p in a.GetInstanceNames ()){
				if (p.StartsWith (thispid, StringComparison.Ordinal))
					continue;
				
				procstore.AppendValues (p);
			}
		}

		protected virtual void OnClose (object sender, System.EventArgs e)
		{
		}

		public int PID {
			get {
				return pid;
			}
		}
	}
}
