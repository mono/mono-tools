//
// Gendarme.Framework.Rocks.ModuleRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Helpers;

namespace Gendarme.Framework.Rocks {

	// add ModuleDefinition extensions methods here
	// only if:
	// * you supply minimal documentation for them (xml)
	// * you supply unit tests for them
	// * they are required somewhere to simplify, even indirectly, the rules
	//   (i.e. don't bloat the framework in case of x, y or z in the future)

	/// <summary>
	/// ModuleRocks contains extensions methods for ModuleDefinition
	/// and the related collection classes.
	/// </summary>
	public static class ModuleRocks {

		static bool running_on_mono;

		static ModuleRocks ()
		{
			running_on_mono = typeof (object).Assembly.GetType ("System.MonoType", false) != null;
		}

		static bool RunningOnMono {
			get { return running_on_mono; }
		}

		/// <summary>
		/// Check is debugging symbols are available for this module.
		/// </summary>
		/// <param name="self"></param>
		/// <returns>True if the symbol file for this module is available, False otherwise</returns>
		public static bool HasDebuggingInformation (this ModuleDefinition self)
		{
			return (self as IAnnotationProvider).Annotations.Contains ("symbols");
		}

		/// <summary>
		/// Load, if available, the debugging symbols associated with the module. This first
		/// try to load a MDB file (symbols from the Mono:: runtime) and then, if not present 
		/// and running on MS.NET, try to load a PDB file (symbols from MS runtime).
		/// </summary>
		/// <param name="self"></param>
		public static void LoadDebuggingSymbols (this ModuleDefinition self)
		{
			// don't create a new reader if the symbols are already loaded
			IAnnotationProvider provider = (self as IAnnotationProvider);
			if (provider.Annotations.Contains ("symbols"))
				return;

			string image_name = self.Image.FileInformation.FullName;
			string symbol_name = image_name + ".mdb";
			Type reader_type = null;

			// we can always load Mono symbols (whatever the runtime we're using)
			// so we start by looking for it's debugging symbol file
			if (File.Exists (symbol_name)) {
				// "always" if we can find Mono.Cecil.Mdb
				reader_type = Type.GetType ("Mono.Cecil.Mdb.MdbFactory, Mono.Cecil.Mdb, Version=0.2.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
				// load the assembly from the current folder if
				// it is here, or fallback to the gac
			}
			
			// if we could not load Mono's symbols then we try, if not running on Mono,
			// to load MS symbols (PDB files)
			if ((reader_type == null) && !RunningOnMono) {
				// assume we're running on MS.NET
				symbol_name = Path.ChangeExtension (image_name, ".pdb");
				if (File.Exists (symbol_name)) {
					reader_type = Type.GetType ("Mono.Cecil.Pdb.PdbFactory, Mono.Cecil.Pdb");
				}
			}

			// no symbols are available to load
			if (reader_type == null)
				return;

			ISymbolStoreFactory factory = (ISymbolStoreFactory) Activator.CreateInstance (reader_type);
			try {
				self.LoadSymbols (factory.CreateReader (self, image_name));
				provider.Annotations.Add ("symbols", symbol_name);
			}
			catch (FileNotFoundException) {
				// this happens if a MDB file is missing 	 
			}
			catch (TypeLoadException) {
				// this happens if a Mono.Cecil.Mdb.dll is not found
			}
			catch (COMException) {
				// this happens if a PDB file is missing
			}
		}
	}
}
