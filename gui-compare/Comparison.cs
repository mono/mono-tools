
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public enum ComparisonStatus {
		None,
		Missing,
		Extra,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (CompType type,
		                       string name)
		{
			this.type = type;
			this.name = name;
			this.children = new List<ComparisonNode>();
			this.messages = new List<string>();
			this.todos = new List<string>();
		}

		public void AddChild (ComparisonNode node)
		{
			children.Add (node);
			node.parent = this;
		}

		public void PropagateCounts ()
		{
			Todo = todos.Count;
			foreach (ComparisonNode n in children) {
				n.PropagateCounts ();
				Extra += n.Extra + (n.status == ComparisonStatus.Extra ? 1 : 0);
				Missing += n.Missing + (n.status == ComparisonStatus.Missing ? 1 : 0);
				Present += n.Present; // XXX
				Todo += n.Todo;
				Warning += n.Warning + (n.status == ComparisonStatus.Error ? 1 : 0);
			}
		}


		public ComparisonStatus status;
		public CompType type;

		public ComparisonNode parent;

		public string name;
		public List<string> messages;
		public List<string> todos;
		
		public int Extra;
		public int Missing;
		public int Present;
		public int Warning;
		public int Todo;

		public List<ComparisonNode> children;
	}
}
