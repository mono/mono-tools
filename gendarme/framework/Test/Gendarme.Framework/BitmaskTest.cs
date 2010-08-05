// 
// Unit tests for Bitmask<T>
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

using Gendarme.Framework;

using NUnit.Framework;

namespace Test.Framework {

	[TestFixture]
	public class BitmaskTest {

		[Test]
		public void BitmaskSeverity ()
		{
			Bitmask<Severity> x = new Bitmask<Severity> ();
			Assert.IsFalse (x.Get (Severity.Audit), "Empty/Audit");
			Assert.AreEqual (0, x.GetHashCode (), "Empty/GetHashCode");
			Assert.AreEqual ("0", x.ToString (), "Empty/ToString");
	
			x.Set (Severity.Audit);
			Assert.IsTrue (x.Get (Severity.Audit), "Set/Audit");
			Assert.AreEqual (16, x.GetHashCode (), "GetHashCode");
			Assert.AreEqual ("10000", x.ToString (), "ToString");
			Assert.IsTrue (x.Equals (x), "Equals(self)");
			Assert.IsTrue (x.Equals ((object) x), "Equals((object)self)");
			Assert.IsFalse (x.Equals (Severity.Audit), "Equals");

			x.Clear (Severity.Audit);
			Assert.IsFalse (x.Get (Severity.Audit), "Clear/Audit");
		}

		[Test]
		public void Count ()
		{
			Bitmask<long> x = new Bitmask<long> ();
			Assert.AreEqual (0, x.Count (), "0");
			x.SetAll ();
			Assert.AreEqual (64, x.Count (), "64");
			for (int i = 63; i >= 0; i--) {
				x.Clear (i);
				Assert.AreEqual (i, x.Count (), i.ToString ());
			}
		}

		[Test]
		public void Intersect ()
		{
			Bitmask<long> x = new Bitmask<long> ();
			Assert.IsTrue (x.Intersect (null), "null"); // special case since they are equals
			Assert.IsFalse (x.Intersect (x), "self");

			Bitmask<long> all = new Bitmask<long> ();
			all.SetAll ();
			Assert.IsFalse (x.Intersect (all), "x N all");
			Assert.IsFalse (all.Intersect (x), "all N x");

			x.Set (0);
			Assert.IsTrue (x.Intersect (all), "1 N all");
			Assert.IsTrue (all.Intersect (x), "all N 1");

			Assert.IsTrue (x.Intersect (x), "self 1");
		}

		[Test]
		public void IsSubsetOf ()
		{
			Bitmask<long> x = new Bitmask<long> ();
			Assert.IsFalse (x.IsSubsetOf (null), "null");
			Assert.IsTrue (x.IsSubsetOf (x), "self");

			Bitmask<long> all = new Bitmask<long> ();
			all.SetAll ();
			Assert.IsTrue (x.IsSubsetOf (all), "x < all");
			Assert.IsFalse (all.IsSubsetOf (x), "all < x");

			x.Set (0);
			Assert.IsTrue (x.IsSubsetOf (all), "1 < all");
			Assert.IsFalse (all.IsSubsetOf (x), "all < 1");

			Assert.IsTrue (x.IsSubsetOf (x), "self 1");
		}

		[Test]
		public void SetClearAll ()
		{
			Bitmask<long> x = new Bitmask<long> ();
			for (int i = 0; i < 64; i++) {
				Assert.IsFalse (x.Get (i), "Default#" + i.ToString ());
			}
			x.SetAll ();
			for (int i = 0; i < 64; i++) {
				Assert.IsTrue (x.Get (i), "SetAll#" + i.ToString ());
			}
			x.ClearAll ();
			for (int i = 0; i < 64; i++) {
				Assert.IsFalse (x.Get (i), "ClearAll#" + i.ToString ());
			}
		}

		[Test]
		public void SetUp ()
		{
			Bitmask<long> x = new Bitmask<long> (true);
			for (int i = 0; i < 64; i++) {
				Assert.IsTrue (x.Get (i), "true#" + i.ToString ());
			}
			x.ClearAll ();
			x.SetUp (16);
			for (int i = 0; i < 16; i++) {
				Assert.IsFalse (x.Get (i), "SetUp#" + i.ToString ());
			}
			for (int i = 16; i < 64; i++) {
				Assert.IsTrue (x.Get (i), "SetUp#" + i.ToString ());
			}
		}

		[Test]
		public void SetDown ()
		{
			Bitmask<long> x = new Bitmask<long> (false);
			for (int i = 0; i < 64; i++) {
				Assert.IsFalse (x.Get (i), "false#" + i.ToString ());
			}
			x.SetDown (48);
			for (int i = 0; i <= 48; i++) {
				Assert.IsTrue (x.Get (i), "SetDown#" + i.ToString ());
			}
			for (int i = 49; i < 64; i++) {
				Assert.IsFalse (x.Get (i), "SetDown#" + i.ToString ());
			}
		}
	}
}
