//
// minvoke.cs
//
// (C) 2009 Novell, Inc. (http://www.novell.com)
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
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MInvoke {

	class ImportKey {
		public string module_name;
		public string entry_point;

		public ImportKey (string moduleName, string entryPointName)
		{
			module_name = moduleName.ToLower();
			entry_point = entryPointName;
		}

		public override int GetHashCode ()
		{
			return module_name.GetHashCode() ^ entry_point.GetHashCode();
		}

		public override bool Equals (object o)
		{
			ImportKey key = o as ImportKey;
			if (key == null)
				return false;
			return key.module_name == module_name && key.entry_point == entry_point;
		}
	}

	class DllImportMap : Dictionary<ImportKey, MethodDefinition> { }

	class DllImports : Dictionary<ImportKey, MethodDefinition> { }

	static class CecilRocks {

		public static IEnumerable<TypeDefinition> GetAllTypes (this ModuleDefinition self)
		{
			return self.Types.SelectMany (t => t.GetAllTypes ());
		}

		static IEnumerable<TypeDefinition> GetAllTypes (this TypeDefinition self)
		{
			yield return self;

			if (!self.HasNestedTypes)
				yield break;

			foreach (var type in self.NestedTypes.SelectMany (t => t.GetAllTypes ()))
				yield return type;
		}
	}

	public class MInvoke {
		private static CustomAttribute GetMapDllImportAttribute (ICustomAttributeProvider provider)
		{
			foreach (CustomAttribute ca in provider.CustomAttributes) {
				TypeDefinition resolved = ca.AttributeType.Resolve ();

				if (resolved != null) {
					if (resolved.Name == "MapDllImport") {
// 						Console.WriteLine ("{0} has a MapDllImportAttribute ({1})", provider,
// 								   ca.ConstructorParameters[0]);
						return ca;
					}
				}
			}
			return null;
		}

		private static bool IsFinalizer (MethodDefinition method)
		{
			if (method.Name != "Finalize")
				return false;

			if (!method.IsVirtual)
				return false;

			if (method.Parameters.Count != 0)
				return false;

			return true;
		}
		
		private static DllImportMap BuildMap (string map_assembly)
		{
			DllImportMap map = new DllImportMap ();

			var assembly = AssemblyDefinition.ReadAssembly (map_assembly);
			foreach (TypeDefinition t in assembly.MainModule.GetAllTypes ()) {
				if (t.Name == "<Module>")
					continue;

				if (t.IsSpecialName || t.IsRuntimeSpecialName)
					continue;

				foreach (MethodDefinition md in t.Methods) {
					if (md.IsSpecialName)
						continue;

					if (IsFinalizer (md))
						continue;

					CustomAttribute attr = GetMapDllImportAttribute (md);
					if (attr == null)
						continue;

					ImportKey key = new ImportKey (attr.ConstructorArguments [0].Value.ToString(), md.Name);
					map.Add (key, md);
				}
			}

			return map;
		}

		private static DllImports CollectDllImports (AssemblyDefinition assembly)
		{
			DllImports imports = new DllImports ();

			foreach (TypeDefinition t in assembly.MainModule.GetAllTypes ()) {
				if (t.Name == "<Module>")
					continue;

				if (t.IsSpecialName || t.IsRuntimeSpecialName)
					continue;

				foreach (MethodDefinition md in t.Methods) {
					if (md.IsSpecialName)
						continue;

					if (IsFinalizer (md))
						continue;

					PInvokeInfo pinfo = md.PInvokeInfo;
					if (pinfo == null)
						continue;

// 					Console.WriteLine ("{0} is a pinvoke, hashcode = {1}", md, md.GetHashCode());

					ImportKey key = new ImportKey (pinfo.Module.Name, pinfo.EntryPoint);
					if (imports.ContainsKey (key))
						Console.WriteLine ("WARNING: pinvoke {0}/{1} shows up more than once in input assembly", key.module_name, key.entry_point);
					else
						imports.Add (key, md);
				}
			}

			return imports;
		}

		private static void Retarget (AssemblyDefinition assembly, DllImportMap map, DllImports imports)
		{
			foreach (TypeDefinition t in assembly.MainModule.Types) {
				if (t.Name == "<Module>")
					continue;

				Retarget (t, map, imports);
			}
		}

		private static void Retarget (TypeDefinition type, DllImportMap map, DllImports imports)
		{
			foreach (var nested in type.NestedTypes)
				Retarget (nested, map, imports);

			foreach (MethodDefinition md in type.Methods) {
				if (!md.HasBody)
					continue;
	
				for (int i = 0; i < md.Body.Instructions.Count; i ++) {
					Instruction ins = md.Body.Instructions[i];

					if (ins.OpCode == OpCodes.Call) {
						MethodDefinition method_operand = ins.Operand as MethodDefinition;
						if (method_operand == null)
							continue;

						PInvokeInfo pinfo = method_operand.PInvokeInfo;
						if (pinfo == null)
							continue;

						ImportKey key = new ImportKey (pinfo.Module.Name, pinfo.EntryPoint);
						if (imports.ContainsKey (key)) {
							//Console.WriteLine ("{0} is a pinvoke, {1}/{2}", method_operand, pinfo.EntryPoint, pinfo.Module.Name);
							if (map.ContainsKey (key)) {

								Console.WriteLine ("retargeting reference to method method {0}/{1}", key.module_name, key.entry_point);

								var il = md.Body.GetILProcessor ();
								MethodDefinition mapped_method = map[key];
								MethodReference mapped_ref;

								mapped_ref = type.Module.Import(mapped_method);

								Instruction callMethod = il.Create(OpCodes.Call, mapped_ref);

								il.Replace (ins, callMethod);
							}
							else {
								Console.WriteLine ("WARNING: no map entry for method {0}/{1}", key.module_name, key.entry_point);
							}
						}
					}
				}
			}
		}

		public static void Main (string[] args) {
			if (args.Length != 3) {
				Console.Error.WriteLine ("usage:  minvoke.exe MapAssemblyName InputAssemblyName OutputAssemblyName");
				Environment.Exit (1);
			}
			string map_assembly_name = args[0];
			string input_assembly_name = args[1];
			string output_assembly_name = args[2];

			Console.WriteLine ("building list of MapDllImports from map assembly {0}", map_assembly_name);

			DllImportMap map = BuildMap (map_assembly_name);

			Console.WriteLine ("building list of DllImports in input assembly {0}", input_assembly_name);

			var input_assembly = AssemblyDefinition.ReadAssembly (input_assembly_name);

			DllImports imports = CollectDllImports (input_assembly);

			Console.WriteLine ("retargeting assembly {0} -> {1}", input_assembly_name, output_assembly_name);

			Retarget (input_assembly, map, imports);

			input_assembly.Write (output_assembly_name);
		}
	}
}