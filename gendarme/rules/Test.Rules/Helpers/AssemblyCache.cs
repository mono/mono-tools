//
// Test.Rules.Helpers.AssemblyCache
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Test.Rules.Helpers {
	
	/// <summary>
	/// Performs caching and mapping of AssemblyDefinitions.
	/// </summary>
	internal static class AssemblyCache {
		
		private static Dictionary<Assembly, AssemblyDefinition> cachedAssemblies = 
			new Dictionary<Assembly, AssemblyDefinition> ();

		/// <summary>
		/// Gets an AssemblyDefinition for Reflection.Assembly or creates one.
		/// </summary>
		public static AssemblyDefinition GetDefinition (Assembly asm)
		{
			AssemblyDefinition def;
			if (cachedAssemblies.TryGetValue (asm, out def))
				return def;

			def = AssemblyDefinition.ReadAssembly (
				asm.Location,
				new ReaderParameters { AssemblyResolver = AssemblyResolver.Resolver });
			def.MainModule.LoadDebuggingSymbols ();
			cachedAssemblies.Add (asm, def);
			return def;
		}
	}
}
