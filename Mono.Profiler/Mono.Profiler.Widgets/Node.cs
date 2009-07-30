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
using System.Runtime.InteropServices;

namespace Mono.Profiler.Widgets {
	
	internal abstract class Node {
		
		public static Comparison<Node> DescendingValue = delegate (Node a, Node b) {
			return b.Value.CompareTo (a.Value);
		};

		ProfileStore store;
		Node parent;
		GCHandle gch;
		
		public Node (ProfileStore store, Node parent)
		{
			this.store = store;
			this.parent = parent;
			gch = GCHandle.Alloc (this, GCHandleType.Weak);
		}
		
		public abstract List<Node> Children { get; }
		
		public abstract string Name { get; }
		
		public abstract ulong Value { get; }

		public Node Parent {
			get { return parent; }
		}

		protected ProfileStore Store {
			get { return store; }
		}
		
		public void Dispose ()
		{
			foreach (Node n in Children)
				n.Dispose ();
			gch.Free ();
		}
		
		public static explicit operator Node (Gtk.TreeIter iter)
		{
			if (iter.UserData == IntPtr.Zero)
				return null;
			
			GCHandle gch = (GCHandle) iter.UserData;
			return gch.Target as Node;
		}

		public static explicit operator Gtk.TreeIter (Node node)
		{
			if (node == null)
				return Gtk.TreeIter.Zero;
			
			Gtk.TreeIter result = Gtk.TreeIter.Zero;
			result.UserData = (IntPtr) node.gch;
			return result;
		}
	}		
}
