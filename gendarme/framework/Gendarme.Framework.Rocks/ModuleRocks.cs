//
// Gendarme.Framework.Rocks.ModuleRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		/// Load, if available, the debugging symbols associated with the module. This first
		/// try to load a MDB file (symbols from the Mono:: runtime) and then, if not present 
		/// and running on MS.NET, try to load a PDB file (symbols from MS runtime).
		/// </summary>
		/// <param name="self"></param>
		public static void LoadDebuggingSymbols (this ModuleDefinition self)
		{
			if (self == null)
				return;

			// don't create a new reader if the symbols are already loaded
			if (self.HasSymbols)
				return;

			string image_name = self.FullyQualifiedName;
			string symbol_name = image_name + ".mdb";
			Type reader_type = null;

			// we can always load Mono symbols (whatever the runtime we're using)
			// so we start by looking for it's debugging symbol file
			if (File.Exists (symbol_name)) {
				// "always" if we can find Mono.Cecil.Mdb
				reader_type = Type.GetType ("Mono.Cecil.Mdb.MdbReaderProvider, Mono.Cecil.Mdb, Version=0.9.4.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
				// load the assembly from the current folder if
				// it is here, or fallback to the gac
			}
			
			// if we could not load Mono's symbols then we try, if not running on Mono,
			// to load MS symbols (PDB files)
			if ((reader_type == null) && !RunningOnMono) {
				// assume we're running on MS.NET
				symbol_name = Path.ChangeExtension (image_name, ".pdb");
				if (File.Exists (symbol_name)) {
					reader_type = Type.GetType ("Mono.Cecil.Pdb.PdbReaderProvider, Mono.Cecil.Pdb");
				}
			}

			// no symbols are available to load
			if (reader_type == null)
				return;

			ISymbolReaderProvider provider = (ISymbolReaderProvider) Activator.CreateInstance (reader_type);
			try {
				self.ReadSymbols (provider.GetSymbolReader (self, image_name));
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
			catch (FormatException) {
				// Mono.Cecil.Mdb wrap MonoSymbolFileException inside a FormatException
				// This makes it possible to catch such exception without a reference to the
				// Mono.CompilerServices.SymbolWriter.dll assembly
			}
			catch (InvalidOperationException) {
				// this happens if the PDB is out of sync with the actual DLL (w/new PdbCciReader)
			}
			// in any case (of failure to load symbols) Gendarme can continue its analysis (but some rules
			// can be affected). The HasDebuggingInformation extension method let them adjust themselves
		}

		static TypeDefinition[] Empty = new TypeDefinition [0];

		/// <summary>
		/// Return an IEnumerable that allows a single loop (like a foreach) to
		/// traverse all types that are defined in a module.
		/// </summary>
		/// <param name="self">The ModuleDefinition on which the extension method can be called.</param>
		/// <returns>An IEnumerable to traverse every types of the module</returns>
		public static IEnumerable<TypeDefinition> GetAllTypes (this ModuleDefinition self)
		{
			if (self == null)
				return Empty;
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

		static Dictionary<ModuleDefinition, IEnumerable<MemberReference>> member_ref_cache = new Dictionary<ModuleDefinition, IEnumerable<MemberReference>> ();

		/// <summary>
		/// Check if any MemberReference, referenced by the current ModuleDefinition, satisfies the
		/// specified predicate.
		/// </summary>
		/// <param name="self">The ModuleDefinition on which the extension method can be called.</param>
		/// <param name="predicate">The condition to execute on a provided MemberReference</param>
		/// <returns>True if 'predicate' returns true for any MemberReference in the module's referenced types.</returns>
		/// <remarks>Cecil's GetMemberReferences method will allocate a new array each time it is called.
		/// This extension method will cache the IEnumerable, on the first use, to reduce memory consumption.</remarks>
		public static bool AnyMemberReference (this ModuleDefinition self, Func<MemberReference, bool> predicate)
		{
			if (self == null)
				return false;

			// since ModuleDefinition.GetMemberReferences allocates an array (always identical if the
			// assembly is opened "read-only", like Gendarme does) we'll cache and retrieve the array
			IEnumerable<MemberReference> refs;
			if (!member_ref_cache.TryGetValue (self, out refs)) {
				refs = self.GetMemberReferences ();
				member_ref_cache.Add (self, refs);
			}

			return refs.Any (predicate);
		}

		static Dictionary<ModuleDefinition, IEnumerable<TypeReference>> type_ref_cache = new Dictionary<ModuleDefinition, IEnumerable<TypeReference>> ();

		/// <summary>
		/// Check if any TypeReference, referenced by the current ModuleDefinition, satisfies the
		/// specified predicate.
		/// </summary>
		/// <param name="self">The ModuleDefinition on which the extension method can be called.</param>
		/// <param name="predicate">The condition to execute on a provided TypeReference</param>
		/// <returns>True if 'predicate' returns true for any TypeReference in the module's referenced types.</returns>
		/// <remarks>Cecil's GetTypeReferences method will allocate a new array each time it is called.
		/// This extension method will cache the IEnumerable, on the first use, to reduce memory consumption.</remarks>
		public static bool AnyTypeReference (this ModuleDefinition self, Func<TypeReference, bool> predicate)
		{
			if (self == null)
				return false;

			// since ModuleDefinition.GetTypeReferences allocates an array (always identical if the
			// assembly is opened "read-only", like Gendarme does) we'll cache and retrieve the array
			IEnumerable<TypeReference> refs;
			if (!type_ref_cache.TryGetValue (self, out refs)) {
				refs = self.GetTypeReferences ();
				type_ref_cache.Add (self, refs);
			}

			return refs.Any (predicate);
		}
	}
}
