//
// Unit tests for NullDerefRule rule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

using Test.Rules.Helpers;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class NullDerefTest : MethodRuleTestFixture<NullDerefRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public class GoodCases {
			public List<int> CreateListAll (int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else 
					result = new List<int> (c);
			
				result.Add (1);
				
				return result;
			}
	
			public List<int> CreateListIf (int c)	// this was a false positive with FAILURE4
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else if (c > 0)
					result = new List<int> (c);
			
				if (result != null)	
					result.Add (1);
				
				return result;
			}
	
			public List<int> CreateListIf2 (int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else if (c > 0)
					result = new List<int> (c);
			
				if (null != result)	
					result.Add (1);
				
				return result;
			}
	
			public List<int> CreateListThrow (int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else if (c > 0)
					result = new List<int> (c);
			
				if (result == null)	
					throw new ArgumentException ("c can't be negative!");
				
				result.Add (1);
				
				return result;
			}
	
			public List<int> CreateListThrow2 (int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else if (c > 0)
					result = new List<int> (c);
			
				if (null == result)	
					throw new ArgumentException ("c can't be negative!");
				
				result.Add (1);
				
				return result;
			}

			byte AddFiles (string name)		// this fails without the EmptyStack fix
			{
				if (String.IsNullOrEmpty (name))
					return 0;
	
				if (name.StartsWith ("@", StringComparison.OrdinalIgnoreCase)) {
					// note: recursive (can contains @, masks and filenames)
					using (StreamReader sr = File.OpenText (name.Substring (1))) {
						while (sr.Peek () >= 0) {
							AddFiles (sr.ReadLine ());
						}
					}
				} else if (name.IndexOfAny (new char [] { '*', '?' }) >= 0) {
					string dirname = Path.GetDirectoryName (name);
					if (dirname.Length == 0)
						dirname = "."; // assume current directory
					string [] files = Directory.GetFiles (dirname, Path.GetFileName (name));
					foreach (string file in files) {
						AddAssembly (file);
					}
				} else {
					AddAssembly (name);
				}
				return 0;
			}

			void AddAssembly (string filename)
			{
			}

			void Using ()
			{
				using (Stream stream = new FileStream ("gendarme.xsd", FileMode.Create)) {
					using (XmlReader reader = XmlReader.Create ("foo")) {
						while (reader.Read ()){}
					}
				}
			}
			
			void TryGetValue (Dictionary<string, object> d)
			{
				object o;
				if (d.TryGetValue ("hmm", out o))
					Console.WriteLine (o.ToString ());
			}
		}
		
		public class BadCases {
			public List<int> CreateList (int c)
			{
				List<int> result = null;         
				
				if (c == 0)
					result = new List<int> ();
				else if (c > 0)
					Console.WriteLine ("len = {0}", result.Count);
			
				result.Add (1);		
				
				return result;
			}
		}
		
		[Test]
		public void Test ()
		{
//			Runner.VerbosityLevel = 3;
			
//			AssertRuleSuccess<GoodCases> ("AddFiles");
			AssertRuleSuccess<GoodCases> ();
			AssertRuleFailure<BadCases> ("CreateList");
		}
	}
}
