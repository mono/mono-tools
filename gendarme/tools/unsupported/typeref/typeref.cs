using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

namespace Gendarme.Tools {

	class TypeRef {

		static List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition> ();
		static List<string> TypeNames = new List<string> ();

		static void Main (string [] args)
		{
			TypeNames.AddRange (args [0].Split (','));

			for (int i = 1; i < args.Length; i++) {
				Assemblies.Add (AssemblyDefinition.ReadAssembly (args [i]));
			}

			if (TypeNames.Count == 1) {
				Console.WriteLine ("Looking for type: {0}", TypeNames [0]);
			} else {
				Console.WriteLine ("Looking for types:");
				foreach (string name in TypeNames)
					Console.WriteLine ("\t{0}", name);
			}
			Console.WriteLine ();

			int inside = 0;
			int total = 0;
			foreach (AssemblyDefinition assembly in Assemblies) {
				foreach (ModuleDefinition module in assembly.Modules) {
					bool all = TypeNames.All (name => (module.HasTypeReference (name)));
					if (all)
						inside++;
					total++;
					Console.WriteLine ("{0} : {1}",	all ? "[YES]" : "[no] ", assembly.Name.FullName);
				}
			}
			Console.WriteLine ("TOTAL : {0} out of {1} assemblies", inside, total);
		}
	}
}

