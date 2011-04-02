//
// Unit tests for AvoidAlwaysNullFieldRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Jesse Jones
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Threading;

using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Maintainability {

	[TestFixture]
	public sealed class AvoidAlwaysNullFieldTest : TypeRuleTestFixture<AvoidAlwaysNullFieldRule> {

		private sealed class Good1 {
			private string name = "hey";	// initialized
			
			public void Write ()
			{
				Console.WriteLine (name);
			}
		}

		private class Good2 {
			protected string name;	// not private
			
			public void Write ()
			{
				if (name != null)
					Console.WriteLine (name);
			}
		}

		private sealed class Good3 {
			private string name;	// not used (AvoidUnusedPrivateField will handle this case)
			
			public void Write ()
			{
				Console.WriteLine ("hey");
			}
		}

		private sealed class Good4 {
			private IntPtr zero;	// not a reference type
			
			public void Write ()
			{
				Console.WriteLine (zero);
			}
		}

		private sealed class Good5 {
			private static string name = "hey";	// initialized
			
			public void Write ()
			{
				Console.WriteLine (name);
			}
		}

		private sealed class Good6 {
			private string name;
			
			public Good6 ()
			{
				IndirectSet (ref name);	// address taken
			}
			
			public void Write ()
			{
				Console.WriteLine (name);
			}
			
			public void IndirectSet (ref string n)
			{
				n = "hey";
			}
		}

		private class Bad1 {
			private string name;	
			
			public void Write ()
			{
				if (name != null)
					Console.WriteLine (name);
			}
		}

		private class Bad2 {
			private string name;	
			
			public Bad2 ()
			{
				name = null;
			}
			
			public void Write ()
			{
				if (name != null)
					Console.WriteLine (name);
			}
		}

		private class Bad3 {
			private static string name;	
			
			public Bad3 ()
			{
				name = null;
			}
			
			public void Write ()
			{
				if (name != null)
					Console.WriteLine (name);
			}
		}

		private class Bad4 {
			private string name1;	
			private string name2;	
			
			public Bad4 ()
			{
				name1 = null;	// explictly set to null, but not used
				name2 = "hey";
			}
			
			public void Write ()
			{
				Console.WriteLine (name2);
			}
		}

		private class TernaryIf {
			private string name;

			public TernaryIf (bool flag)
			{
				name = flag ? "hmm" : null;	// we don't handle this properly
			}
			
			public void Write ()
			{
				if (name != null)
					Console.WriteLine (name);
			}
		}

		[Test]
		public void Simple ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);	// no fields
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleSuccess (SimpleTypes.Structure);	// a few public fields
		}

		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();	
			AssertRuleSuccess<Good2> ();	
			AssertRuleSuccess<Good3> ();	
			AssertRuleSuccess<Good4> ();	
			AssertRuleSuccess<Good5> ();	
			AssertRuleSuccess<Good6> ();	

			AssertRuleFailure<Bad1> (1);
			AssertRuleFailure<Bad2> (1);
			AssertRuleFailure<Bad3> (1);
			AssertRuleFailure<Bad4> (1);
		}

		class Bug667692a {

			private string [] bar = null;

			public void SetBar (IEnumerable<string> boo)
			{
				bar = boo == null ? null : boo.ToArray ();
			}
		}

		class Bug667692b {

			private string [] bar = null;

			public void SetBar (IEnumerable<string> boo)
			{
				bar = boo != null ? boo.ToArray () : null;
			}
		}

		[Test]
		public void MultipleAssignment ()
		{
			AssertRuleSuccess<TernaryIf> ();
			AssertRuleSuccess<Bug667692a> ();
			AssertRuleSuccess<Bug667692b> ();
		}

		// this will create an anonymous method
		class AnonymousDelegatesAllFields {

			string file;

			void Parse ()
			{
				ThreadPool.QueueUserWorkItem (delegate {
					file = ""; 
				});
			}

			void Show ()
			{
				Console.WriteLine (file);
			}
		}

		// this will create a nested type with the anonymous method
		class AnonymousDelegatesFieldsAndLocals {

			string file;

			void Parse ()
			{
				string local;

				ThreadPool.QueueUserWorkItem (delegate {
					file = "";
					local = file;
				});
			}

			void Show ()
			{
				Console.WriteLine (file);
			}
		}

		[Test]
		public void AnonymousDelegates ()
		{
			AssertRuleSuccess<AnonymousDelegatesAllFields> ();
			AssertRuleSuccess<AnonymousDelegatesFieldsAndLocals> ();
		}
	}
}
