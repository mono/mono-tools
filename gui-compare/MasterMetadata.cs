using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public class MasterAssembly : CompAssembly {
		public MasterAssembly (string path)
			: base (path)
		{
			masterinfo = XMLAssembly.CreateFromFile (path);
		}

		public override List<CompNamespace> GetNamespaces ()
		{
			List<CompNamespace> namespaces = new List<CompNamespace>();
			foreach (XMLNamespace ns in masterinfo.namespaces)
				namespaces.Add (new MasterNamespace (ns));

			namespaces.Sort(delegate (CompNamespace x, CompNamespace y) { return String.Compare (x.Name, y.Name); });

			return namespaces;
		}

		XMLAssembly masterinfo;
	}

	public class MasterNamespace : CompNamespace {
		public MasterNamespace (XMLNamespace ns)
			: base (ns.name)
		{
			this.ns = ns;
		}

		public override List<CompClass> GetClasses ()
		{
			List<CompClass> classes = new List<CompClass>();
			foreach (XMLClass cls in ns.types)
				classes.Add (new MasterClass (cls));

			classes.Sort(delegate (CompClass x, CompClass y) { return String.Compare (x.Name, y.Name); });

			return classes;
		}

		XMLNamespace ns;
	}

	public class MasterClass : CompClass {
		public MasterClass (XMLClass cls)
			: base (cls.name)
		{
		}

		public override List<CompMethod> GetMethods()
		{
			throw new NotImplementedException ();
		}

		public override List<CompMethod> GetConstructors()
		{
			throw new NotImplementedException ();
		}
	}
}
