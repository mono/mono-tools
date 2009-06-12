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
using Gtk;
using Mono.Profiler;

namespace Mono.Profiler.Widgets {
	
	internal class CallsStore : GLib.Object, TreeModelImplementor {

		class Node {
		
			Node parent;
			List<Node> children;
			StackTrace frame;
			GCHandle gch;
			
			public Node (Node parent, StackTrace frame)
			{
				this.parent = parent;
				this.frame = frame;
				gch = GCHandle.Alloc (this, GCHandleType.Weak);
			}
			
			public List<Node> Children {
				get {
					if (children == null) {
						children = new List<Node> ();
						foreach (StackTrace child in frame.CalledFrames)
							children.Add (new Node (this, child));
					}
					return children;
				}
			}
			
			public ulong Clicks {
				get { return frame.Clicks; }
			}
			
			public LoadedMethod Method {
				get { return frame.TopMethod; }
			}
			
			public Node Parent {
				get { return parent; }
			}
			
			public void Dispose ()
			{
				foreach (Node n in Children)
					n.Dispose ();
				gch.Free ();
			}
			
			public static explicit operator Node (TreeIter iter)
			{
				if (iter.UserData == IntPtr.Zero)
					return null;
				
				GCHandle gch = (GCHandle) iter.UserData;
				return gch.Target as Node;
			}

			public static explicit operator TreeIter (Node node)
			{
				if (node == null)
					return TreeIter.Zero;
				
				TreeIter result = TreeIter.Zero;
				result.UserData = (IntPtr) node.gch;
				return result;
			}
		}
		
		ProfilerEventHandler data;
		ulong total_clicks;
		List<Node> nodes;

		public CallsStore (ProfilerEventHandler data)
		{
			this.data = data;
			if (data == null || (data.Flags & ProfilerFlags.METHOD_EVENTS) == 0)
				return;
			
			nodes = new List<Node> ();
			foreach (StackTrace frame in data.RootFrames) {
				total_clicks += frame.TopMethod.Clicks;
				nodes.Add (new Node (null, frame));
			}
		}

		public override void Dispose ()
		{
			foreach (Node n in nodes)
				n.Dispose ();
			base.Dispose ();
		}
		
		public TreeModelFlags Flags {
			get { return TreeModelFlags.ItersPersist; }
		}
		
		public int NColumns {
			get { return 2; }
		}
		
		public GLib.GType GetColumnType (int idx)
		{
			if (idx > 1)
				return GLib.GType.Invalid;
			else 
				return GLib.GType.String;
		}
		
		public bool GetIter (out TreeIter iter, TreePath path)
		{
			iter = TreeIter.Zero;
			if (path.Indices.Length == 0 || nodes.Count <= path.Indices [0])
				return false;

			Node node = nodes [path.Indices [0]];
			for (int i = 1; i < path.Indices.Length; i++) {
				if (node.Children.Count <= path.Indices [i])
					return false;
				node = node.Children [path.Indices [i]];
			}
			iter = (TreeIter) node;
			return true;
		}
		
		public TreePath GetPath (TreeIter iter)
		{
			Node node = (Node) iter;
			TreePath result = new TreePath ();
			if (node == null)
				return result;
			
			while (node.Parent != null) {
				result.PrependIndex (node.Parent.Children.IndexOf (node));
				node = node.Parent;
			}
			result.PrependIndex (nodes.IndexOf (node));
			return result;
		}
		
		public void GetValue (TreeIter iter, int column, ref GLib.Value val)
		{
			Node node = (Node) iter;
			LoadedMethod m = node.Method;
			if (column == 0)
				val = new GLib.Value (m.Class.Name + "." + m.Name);
			else if (column == 1)
				val = new GLib.Value (String.Format ("{0,5:F2}% ({1:F6}s)", ((((double)m.Clicks) / total_clicks) * 100), data.ClicksToSeconds (m.Clicks)));
		}
		
		public bool IterChildren (out TreeIter iter, TreeIter parent)
		{
			iter = TreeIter.Zero;
			Node parent_node = (Node) parent;
			if (parent_node == null) {
				if (nodes.Count == 0)
					return false;
				iter = (TreeIter) nodes [0];
			} else {
				if (parent_node.Children.Count == 0)
					return false;
				iter = (TreeIter) parent_node.Children [0];
			}
			return true;
		}
		
		public bool IterHasChild (TreeIter iter)
		{
			Node node = (Node) iter;
			if (node == null)
				return nodes.Count > 0;
			else
				return node.Children.Count > 0;
		}
		
		public int IterNChildren (TreeIter iter)
		{
			Node node = (Node) iter;
			if (node == null)
				return nodes.Count;
			else
				return node.Children.Count;
		}

		public bool IterNext (ref TreeIter iter)
		{
			Node node = (Node) iter;
			iter = TreeIter.Zero;
			if (node == null)
				return false;

			List<Node> siblings = node.Parent == null ? nodes : node.Parent.Children;
			int idx = siblings.IndexOf (node);
			if (idx < 0 || ++idx >= siblings.Count)
				return false;
			iter = (TreeIter) siblings [idx];
			return true;
		}
		
		public bool IterNthChild (out TreeIter iter, TreeIter parent, int n)
		{
			Node parent_node = (Node) parent;
			List<Node> siblings = parent_node == null ? nodes : parent_node.Children;
			if (siblings.Count == 0 || siblings.Count <= n) {
				iter = TreeIter.Zero;
				return false;
			} else {
				iter = (TreeIter) siblings [n];
				return true;
			}
		}
		
		public bool IterParent (out TreeIter parent, TreeIter iter)
		{
			Node node = (Node) iter;
			if (node == null) {
				parent = TreeIter.Zero;
				return false;
			} else {
				parent = (TreeIter) node.Parent;
				return true;
			}
		}
		
		public void RefNode (TreeIter iter)
		{
		}
		
		public void UnrefNode (TreeIter iter)
		{
		}
	}
}
