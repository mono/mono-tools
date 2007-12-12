
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
		Todo,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (CompType type,
				       string name)
		{
			this.type = type;
			this.name = name;
			this.children = new List<ComparisonNode>();
		}

		public void AddChild (ComparisonNode node)
		{
			children.Add (node);
			node.parent = this;
		}

		public void PropagateCounts ()
		{
			foreach (ComparisonNode n in children) {
				n.PropagateCounts ();
				Extra += n.Extra + (n.status == ComparisonStatus.Extra ? 1 : 0);
				Missing += n.Missing + (n.status == ComparisonStatus.Missing ? 1 : 0);
				Present += n.Present; // XXX
				Todo += n.Todo + (n.status == ComparisonStatus.Todo ? 1 : 0);
				Warning += n.Warning; // XXX
			}
		}


		public ComparisonStatus status;
		public CompType type;

		public ComparisonNode parent;

		public string name;

		public int Extra;
		public int Missing;
		public int Present;
		public int Todo;
		public int Warning;

		public List<ComparisonNode> children;
	}
}
