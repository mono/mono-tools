
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public abstract class CompAssembly {
		public CompAssembly (string name)
		{
			this.name = name;
		}

		public string Name {
			get { return name; }
		}

		public abstract List<CompNamespace> GetNamespaces();

		string name;
	}

	public abstract class CompNamespace {
		string name;

		public CompNamespace (string name)
		{
			this.name = name;
		}

		public string Name {
			get { return name; }
		}

		public abstract List<CompClass> GetClasses();
	}

	public abstract class CompAttributeContainer
	{
// 		public List<CompAttribute> GetAttributes ();
	}

	public abstract class CompClass : CompAttributeContainer {
		string name;

		public CompClass (string name)
		{
			this.name = name;
		}

		public string Name {
			get { return name; }
		}

		public abstract List<CompMethod>   GetConstructors();
		public abstract List<CompMethod>   GetMethods();
// 		public abstract List<CompProperty> GetProperties();
// 		public abstract List<CompField>    GetFields();
// 		public abstract List<CompEvent>    GetEvents();

		public abstract List<CompClass>  GetNestedClasses();
	}

	public abstract class CompMethod : CompAttributeContainer {
		public CompMethod (string name)
		{
			this.name = name;
		}

		public string Name {
			get { return name; }
		}

		string name;
	}
}
