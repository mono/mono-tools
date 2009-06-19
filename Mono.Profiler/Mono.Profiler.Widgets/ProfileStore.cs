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
	
	internal abstract class ProfileStore : GLib.Object, TreeModelImplementor {

		ProfilerEventHandler data;
		DisplayOptions options;
		protected List<Node> nodes;

		protected ProfileStore (ProfilerEventHandler data, DisplayOptions options)
		{
			this.data = data;
			this.options = options;
		}

		public override void Dispose ()
		{
			foreach (Node n in nodes)
				n.Dispose ();
			base.Dispose ();
		}

		public DisplayOptions Options {
			get { return options; }
		}
				
		public ProfilerEventHandler ProfileData {
			get { return data; }
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
		
		public abstract void GetValue (TreeIter iter, int column, ref GLib.Value val);
		
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
