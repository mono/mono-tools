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
using Mono.Profiler;

namespace Mono.Profiler.Widgets {
	
	internal class CallsStore : ProfileStore {

		class CallsNode : Node {
		
			List<Node> children;
			StackTrace frame;
			
			public CallsNode (ProfileStore store, Node parent, StackTrace frame) : base (store, parent)
			{
				this.frame = frame;
			}
			
			public override List<Node> Children {
				get {
					if (children == null) {
						children = new List<Node> ();
						bool filter = Store.Options.Filters.Contains (frame.TopMethod.Class.Assembly.BaseName);
						foreach (StackTrace child in frame.CalledFrames) {
							if (filter)
								RecursiveAddFilteredChildren (child);
							else
								children.Add (new CallsNode (Store, this, child));
						}
					}
					return children;
				}
			}
			
			void RecursiveAddFilteredChildren (StackTrace trace)
			{
				if (Store.Options.Filters.Contains (trace.TopMethod.Class.Assembly.BaseName))
					foreach (StackTrace child in trace.CalledFrames)
						RecursiveAddFilteredChildren (child);
				else
					children.Add (new CallsNode (Store, this, trace));
			}
			
			public override string Name {
				get { return frame.TopMethod.Class.Name + "." + frame.TopMethod.Name; }
			}
			
			public override ulong Value {
				get { return frame.Clicks; }
			}
		}
		
		ulong total_clicks;

		public CallsStore (ProfilerEventHandler data, DisplayOptions options) : base (data, options)
		{
			if (data == null || (data.Flags & ProfilerFlags.METHOD_EVENTS) == 0)
				return;
			
			nodes = new List<Node> ();
			foreach (StackTrace frame in data.RootFrames) {
				total_clicks += frame.TopMethod.Clicks;
				nodes.Add (new CallsNode (this, null, frame));
			}
		}

		public override void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value val)
		{
			Node node = (Node) iter;
			if (column == 0)
				val = new GLib.Value (node.Name);
			else if (column == 1) {
				double percent = (double) node.Value / (double) total_clicks * 100.0;
				val = new GLib.Value (String.Format ("{0,5:F2}% ({1:F6}s)", percent, ProfileData.ClicksToSeconds (node.Value)));
			}
		}		
	}
}
