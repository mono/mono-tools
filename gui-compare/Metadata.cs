
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public abstract class CompNamed {
		public CompNamed ()
		{
		}

		public CompNamed (string name)
		{
			this.name = name;
		}

		public string Name {
			set { name = value; }
			get { return name; }
		}

		string name;
	}

	public abstract class CompAssembly : CompNamed {
		public CompAssembly (string name)
			: base (name)
		{
		}

		public abstract List<CompNamespace> GetNamespaces();
	}

	public abstract class CompNamespace : CompNamed {
		public CompNamespace (string name)
			: base (name)
		{
		}

		public abstract List<CompClass> GetClasses();
		public abstract List<CompInterface> GetInterfaces ();
		public abstract List<CompClass> GetStructs ();
	}

	public interface ICompAttributeContainer
	{
// 		public List<CompAttribute> GetAttributes ();
	}

	public abstract class CompInterface : CompNamed, ICompAttributeContainer {
		public CompInterface (string name)
			: base (name)
		{
		}
	}

	public abstract class CompClass : CompNamed, ICompAttributeContainer {
		public CompClass (string name)
			: base (name)
		{
		}

		public abstract List<CompInterface> GetInterfaces ();
		public abstract List<CompMethod>    GetConstructors();
		public abstract List<CompMethod>    GetMethods();
// 		public abstract List<CompProperty>  GetProperties();
// 		public abstract List<CompField>     GetFields();
// 		public abstract List<CompEvent>     GetEvents();

		public abstract List<CompClass>  GetNestedClasses();
	}

	public abstract class CompMethod : CompNamed, ICompAttributeContainer {
		public CompMethod (string name)
			: base (name)
		{
		}
	}
}
