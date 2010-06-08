// 
// Unit tests for ThreadModelAttribute
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using NUnit.Framework;

namespace Test.Framework {

	[TestFixture]
	public class ThreadModelAttributeTest {

		private ThreadModelAttribute BasicCheck (ThreadModel model, bool every)
		{
			string name = model.ToString ();
			ThreadModelAttribute tma = new ThreadModelAttribute (model);
			Assert.AreEqual (model & ~ThreadModel.AllowEveryCaller, tma.Model, name);
			Assert.AreEqual (every, tma.AllowsEveryCaller, name + ".AllowsEveryCaller");
			Assert.AreEqual (every, tma.ToString ().Contains ("AllowEveryCaller"), "ToString()");

			int code = tma.GetHashCode ();
			tma.AllowsEveryCaller = !tma.AllowsEveryCaller;
			Assert.AreNotEqual (code, tma.GetHashCode (), name + "!HashCode");
			tma.AllowsEveryCaller = !tma.AllowsEveryCaller;
			return tma;
		}

		[Test]
		public void MainThread ()
		{
			ThreadModelAttribute tma = BasicCheck (ThreadModel.MainThread, false);
			Assert.IsTrue (tma.Equals (tma), "Equals");
			Assert.IsTrue (tma == new ThreadModelAttribute (ThreadModel.MainThread), "==");

			ThreadModelAttribute tmae = BasicCheck (ThreadModel.MainThread | ThreadModel.AllowEveryCaller, true);
			Assert.IsFalse (tmae.Equals (tma), "!Equals");
			Assert.IsTrue (tma != tmae, "!=");
		}

		[Test]
		public void SingleThread ()
		{
			ThreadModelAttribute tma = BasicCheck (ThreadModel.SingleThread, false);
			Assert.IsTrue (tma.Equals ((object) tma), "Equals");
			Assert.IsFalse (tma == new ThreadModelAttribute (ThreadModel.MainThread), "==");

			ThreadModelAttribute tmae = BasicCheck (ThreadModel.SingleThread | ThreadModel.AllowEveryCaller, true);
			Assert.IsFalse (tmae.Equals ((object) null), "!Equals");
			Assert.IsFalse (tma == tmae, "==");
		}

		[Test]
		public void Serializable ()
		{
			ThreadModelAttribute tma = BasicCheck (ThreadModel.Serializable, false);
			Assert.IsTrue (tma.Equals ((object) tma), "Equals");

			ThreadModelAttribute tmae = BasicCheck (ThreadModel.Serializable | ThreadModel.AllowEveryCaller, true);
			Assert.IsFalse (tmae.Equals ((ThreadModelAttribute) null), "!Equals");
			Assert.IsTrue (tma != null, "!= null");
			Assert.IsTrue (null != tmae, "null !=");
		}

		[Test]
		public void Concurrent ()
		{
			ThreadModelAttribute tma = BasicCheck (ThreadModel.Concurrent, false);
			Assert.IsTrue (tma.Equals (tma), "Equals");

			ThreadModelAttribute tmae = BasicCheck (ThreadModel.Concurrent | ThreadModel.AllowEveryCaller, true);
			Assert.IsFalse (tmae.Equals ((ThreadModelAttribute) null), "!Equals");
			Assert.IsFalse (tma == tmae, "==");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Invalid ()
		{
			new ThreadModelAttribute ((ThreadModel) Int32.MinValue);
		}
	}
}

