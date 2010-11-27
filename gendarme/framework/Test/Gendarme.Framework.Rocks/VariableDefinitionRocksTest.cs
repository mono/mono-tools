// 
// Unit tests for VariableDefinitionRocks
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Test.Framework.Rocks {

	[TestFixture]
	public class VariableDefinitionRocksTest {
		
		private TypeDefinition type_def;
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);
			assembly.MainModule.LoadDebuggingSymbols ();
			
			type_def = assembly.MainModule.GetType ("Test.Framework.Rocks.VariableDefinitionRocksTest");
		}
		
		public string path;
		public string host;
		public string cachedLocalPath;
		public string AbsolutePath;
		
		private string Unescape (string s)
		{
			return s;
		}
		
		// This has one compiler generated local with gmcs.
		private string Big ()
		{
			if (cachedLocalPath != null)
				return cachedLocalPath;

			bool windows = (path.Length > 3 && path [1] == ':' &&
					(path [2] == '\\' || path [2] == '/'));

			if (cachedLocalPath != null) {
				string p = Unescape (path);
				bool replace = windows;
				if (replace)
					cachedLocalPath = p.Replace ('/', '\\');
				else
					cachedLocalPath = p;
			} else {
				if (path.Length > 1 && path [1] == ':')
					cachedLocalPath = Unescape (path.Replace (Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

				else if (System.IO.Path.DirectorySeparatorChar == '\\') {
					string h = host;
					if (path.Length > 0) {
						if ((path.Length > 1) || (path [0] != '/')) {
							h += path.Replace ('/', '\\');
						}
					}
					cachedLocalPath = "\\\\" + Unescape (h);
				}  else
					cachedLocalPath = Unescape (path);
			}
			if (cachedLocalPath.Length == 0)
				cachedLocalPath = Path.DirectorySeparatorChar.ToString ();
			return cachedLocalPath;
		}
		
		[Test]
		public void BigTest ()
		{
			MethodDefinition method = type_def.GetMethod ("Big");
			DoTest (method, "windows", "p", "replace", "h");
		}
		
		// This has two compiler generated locals with gmcs.
		private void ForEach (string [] names)
		{
			foreach (string name in names) {
				Console.WriteLine (name);
			}
		}
		
		[Test]
		public void ForEachTest ()
		{
			MethodDefinition method = type_def.GetMethod ("ForEach");
			DoTest (method, "name");
		}
		
		private void DoTest (MethodDefinition method, params string [] userNames)
		{
			int count = 0;
			
			foreach (Instruction ins in method.Body.Instructions) {
				VariableDefinition v = ins.GetVariable (method);
				if (v != null) {
					bool userName = userNames.Any (n => n == v.Name);
					if (userName) {
						Assert.IsFalse (v.IsGeneratedName (), "{0} was reported as a generated name", v.Name);
					} else {
						++count;
						Assert.IsTrue (v.IsGeneratedName (), "{0} was not reported as a generated name", v.Name);
					}
				}
			}
			
			if (count == 0)
				Assert.Fail ("Didn't find any generated locals for VariableDefinitionRocksTest::{0}", method.Name);
		}
	}
}
