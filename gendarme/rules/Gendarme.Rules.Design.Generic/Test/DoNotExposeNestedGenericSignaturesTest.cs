//
// Unit Tests for DoNotExposeNestedGenericSignaturesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class DoNotExposeNestedGenericSignaturesTest : MethodRuleTestFixture<DoNotExposeNestedGenericSignaturesRule> {

		public class IntegerCollection : List<int> {
		}

		public class Generic<T> {

			public ICollection NonGenericReturnValue ()
			{
				return null;
			}

			public ICollection<string> GenericReturnValue ()
			{
				return null;
			}

			public ICollection<KeyValuePair<T, T>> NestedReturnValue ()
			{
				return null;
			}

			public void NonGenericParameter (ICollection value)
			{
			}

			public void GenericParameter (ICollection<int> value)
			{
			}

			public void NestedParameter (KeyValuePair<T, ICollection<int>> value)
			{
			}

			public void UnnestedParameter (KeyValuePair<T, IntegerCollection> value)
			{
			}

			public void TwoNestedParameters (KeyValuePair<T, ICollection<int>> value,
				int max, int min, ICollection<KeyValuePair<T,string>> template)
			{
			}

			protected ICollection<KeyValuePair<T, T>> All (KeyValuePair<T, ICollection<int>> value,
				int max, int min, ICollection<KeyValuePair<T, string>> template)
			{
				return null;
			}
		}

		[Test]
		public void ReturnValue ()
		{
			AssertRuleSuccess<Generic<int>> ("NonGenericReturnValue");
			AssertRuleSuccess<Generic<int>> ("GenericReturnValue");
			AssertRuleFailure<Generic<int>> ("NestedReturnValue", 1);
		}

		[Test]
		public void Parameters ()
		{
			AssertRuleSuccess<Generic<int>> ("NonGenericParameter");
			AssertRuleSuccess<Generic<int>> ("GenericParameter");
			AssertRuleFailure<Generic<int>> ("NestedParameter", 1);
			AssertRuleSuccess<Generic<int>> ("UnnestedParameter");
			AssertRuleFailure<Generic<int>> ("TwoNestedParameters", 2);
		}

		[Test]
		public void Both ()
		{
			AssertRuleFailure<Generic<int>> ("All", 3);
		}

		// adapted from XmlResultWriter

		public XElement CreateDefects ()
		{
			var query = from n in Runner.Defects
				    group n by n.Rule into a
//gmcs bug//			    orderby a.Key.Name
				    select new {
					    Rule = a.Key,
					    Value = from o in a
						    group o by o.Target into r
						    orderby (r.Key == null ? String.Empty : a.Key.Name)
						    select new {
							    Target = r.Key,
							    Value = r
						    }
				    };

			return new XElement ("results",
				from value in query
				select new XElement ("rule",
					CreateRuleDetails (value.Rule),
					from v2 in value.Value
					select new XElement ("target",
						CreateTargetDetails (v2.Target),
						from Defect defect in v2.Value
						select CreateDefect (defect))));
		}

		static XObject [] CreateRuleDetails (IRule rule)
		{
			return null;
		}

		static XObject [] CreateTargetDetails (IMetadataTokenProvider target)
		{
			return null;
		}

		static XElement CreateDefect (Defect defect)
		{
			return null;
		}

		[Test]
		public void Linq ()
		{
			AssertRuleSuccess<DoNotExposeNestedGenericSignaturesTest> ("CreateDefects");
		}

		public IEnumerable<int?> NullableReturnValue ()
		{
			return null;
		}

		public void NullableParameter (ICollection<DateTime?> value)
		{
		}

		[Test]
		public void Nullable ()
		{
			AssertRuleSuccess<DoNotExposeNestedGenericSignaturesTest> ("NullableReturnValue");
			AssertRuleSuccess<DoNotExposeNestedGenericSignaturesTest> ("NullableParameter");
		}
	}
}
