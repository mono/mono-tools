
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public enum ComparisonNodeType {
		Assembly,
		Namespace,
		Attribute,
		Interface,
		Class,
		Struct,
		Enum,
		Method,
		Property,
		Field
	}

	public enum ComparisonStatus {
		None,
		Missing,
		Extra,
		Todo,
		Error
	}

	public class ComparisonNode {
		public ComparisonNode (ComparisonNodeType type,
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
		public ComparisonNodeType type;

		public ComparisonNode parent;

		public string name;

		public int Extra;
		public int Missing;
		public int Present;
		public int Todo;
		public int Warning;

		public List<ComparisonNode> children;
	}

	/* represents the result of comparing a masterinfo to a local assembly */
	public class AssemblyComparison : ComparisonNode {
		public AssemblyComparison (string name)
			: base (ComparisonNodeType.Assembly, name)
		{
		}
	}

	public class NamespaceComparison : ComparisonNode {
		public NamespaceComparison (string name)
			: base (ComparisonNodeType.Namespace, name)
		{
		}
	}

	public class ClassComparison : ComparisonNode {
		public ClassComparison (string name)
			: base (ComparisonNodeType.Class, name)
		{
		}
	}

	public class InterfaceComparison : ComparisonNode {
		public InterfaceComparison (string name)
			: base (ComparisonNodeType.Interface, name)
		{
		}
	}

	public class StructComparison : ComparisonNode {
		public StructComparison (string name)
			: base (ComparisonNodeType.Struct, name)
		{
		}
	}

	public class FieldComparison : ComparisonNode {
		public FieldComparison (string name)
			: base (ComparisonNodeType.Field, name)
		{
		}
	}

	public class MethodComparison : ComparisonNode {
		public MethodComparison (string name)
			: base (ComparisonNodeType.Method, name)
		{
		}
	}

	public class PropertyComparison : ComparisonNode {
		public PropertyComparison (string name)
			: base (ComparisonNodeType.Property, name)
		{
		}
	}

	public class AttributeComparison : ComparisonNode {
		public AttributeComparison (string name)
			: base (ComparisonNodeType.Attribute, name)
		{
		}
	}
}
