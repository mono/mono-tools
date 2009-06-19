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
	
	internal class AllocationsStore : ProfileStore {

		ulong total_bytes;
		
		class ClassNode : Node {
		
			List<Node> children;
			LoadedClass instance;
			
			public ClassNode (ProfileStore store, Node parent, LoadedClass instance) : base (store, parent)
			{
				this.instance = instance;
			}
			
			public override List<Node> Children {
				get {
					if (children == null) {
						children = new List<Node> ();
						foreach (LoadedClass.AllocationsPerMethod child in instance.Methods)
							children.Add (new MethodNode (Store, this, child));
					}
					return children;
				}
			}
			
			public override string Name {
				get { return instance.Name; }
			}
			
			public override ulong Value {
				get { return instance.AllocatedBytes; }
			}
		}
		
		class MethodNode : Node {
			
			List<Node> children = new List<Node> ();
			LoadedClass.AllocationsPerMethod instance;
			
			public MethodNode (ProfileStore store, Node parent, LoadedClass.AllocationsPerMethod instance) : base (store, parent)
			{
				this.instance = instance;
			}
			
			public override List<Node> Children {
				get { return children; }
			}

			public override string Name {
				get { return instance.Method.Class.Name + "." + instance.Method.Name; }
			}
			
			public override ulong Value {
				get { return instance.AllocatedBytes; }
			}
		}	
		
		public AllocationsStore (ProfilerEventHandler data, DisplayOptions options) : base (data, options)
		{
			if (data == null || (data.Flags & ProfilerFlags.CLASS_EVENTS) == 0)
				return;

			nodes = new List<Node> ();
			LoadedClass[] classes = data.LoadedElements.Classes;
			Array.Sort (classes, LoadedClass.CompareByAllocatedBytes);
			Array.Reverse (classes);
			foreach (LoadedClass c in classes) {
				if (c.AllocatedBytes > 0) {
					total_bytes += c.AllocatedBytes;
					nodes.Add (new ClassNode (this, null, c));
				}
			}
		}
		
		public override void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value val)
		{
			Node node = (Node) iter;
			if (column == 0)
				val = new GLib.Value (node.Name);
			else {
				double percent = (double) node.Value / (double) total_bytes * 100.0;
				val = new GLib.Value (String.Format ("{0,5:F2}% ({1} bytes)", percent, node.Value));
			}
		}
	}
}
