
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

		public ComparisonStatus status;
		public ComparisonNodeType type;

		public ComparisonNode parent;

		public string name;

		public int Present;
		public int PresentTotal;
		public int Missing;
		public int MissingTotal;
		public int Todo;
		public int TodoTotal;

		public int Extra;
		public int ExtraTotal;
		public int Warning;
		public int WarningTotal;
		public int ErrorTotal;

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
