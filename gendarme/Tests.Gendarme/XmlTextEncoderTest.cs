//
// Unit Test for XmlTextEncoder
//
// Authors:
//	Anthony DeMartini <anthony.demartini@securedecisions.com>
//
// 	(C) 2014 Applied Visions
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gendarme;
using System.Xml;
namespace Tests.Gendarme
{
    [TestClass]
    public class XmlTextEncoderTest
    {
        [TestMethod]
        public void TestEncode ()
        {
            var result = XmlResultWriter.XmlTextEncoder.Encode ("Found string: \"\\x1\\x1\\n.\\x1\\x2\\x4.\\x3\\x2\\x1.\\x1\\x2\\x3.\\x2\\x2\\x1.\\x1\\x2\\x2.\\x3\\x2\\x2.\\x1\\x2\\x2.\\x1\\x2\\x1.\\x1\\x2\\x16.\\x1\\x2\\xb.\\x1\\x2\\x2.\\x2\\x2\\x7.\\x1\\x2\\x1.\\x2\\x2\\x1.\\x5\\x2\\x10.\\x1\\x2\\x5.\\x2\\x2\\x5.\\x1\\x2\\x1.\\x1\\x2\\t.\\x1\\x2\\xc.\\x1\\x2\\xf.\\x1\\x2\\x1.\\x4\\x2\\x5.\\x1\\x2\\x1.\\x1\\x2\\x2.\\x1\\x2\\x3.\\x1\\x2\\x3.\\x2\\x2\\x1.\\x1\\x2\\x2.\\x1\\x2\\x16.\\x1\\x2\\n.\\x1\\x2\\x3.\\x1\\x2\\x2.\\x1\\x2\\x1.\\x1\\x2\\x1.\\x7\\x2&.\\x1\\x2\\x1.\\x5\\x2\\x7.\\x1\\x2\\x14.\\x1\\x2\\x2.\\x1\\x2\\x2.\\x2\\x2\\x1.\\x2\\x2\\x18.\\x1\\x2\\x15.\\x1\\x2\\x2.\\x2\\x2\".");
            Assert.AreEqual ("Found string: &quot;\\x1\\x1\\n.\\x1\\x2\\x4.\\x3\\x2\\x1.\\x1\\x2\\x3.\\x2\\x2\\x1.\\x1\\x2\\x2.\\x3\\x2\\x2.\\x1\\x2\\x2.\\x1\\x2\\x1.\\x1\\x2\\x16.\\x1\\x2\\xb.\\x1\\x2\\x2.\\x2\\x2\\x7.\\x1\\x2\\x1.\\x2\\x2\\x1.\\x5\\x2\\x10.\\x1\\x2\\x5.\\x2\\x2\\x5.\\x1\\x2\\x1.\\x1\\x2\\t.\\x1\\x2\\xc.\\x1\\x2\\xf.\\x1\\x2\\x1.\\x4\\x2\\x5.\\x1\\x2\\x1.\\x1\\x2\\x2.\\x1\\x2\\x3.\\x1\\x2\\x3.\\x2\\x2\\x1.\\x1\\x2\\x2.\\x1\\x2\\x16.\\x1\\x2\\n.\\x1\\x2\\x3.\\x1\\x2\\x2.\\x1\\x2\\x1.\\x1\\x2\\x1.\\x7\\x2&amp;.\\x1\\x2\\x1.\\x5\\x2\\x7.\\x1\\x2\\x14.\\x1\\x2\\x2.\\x1\\x2\\x2.\\x2\\x2\\x1.\\x2\\x2\\x18.\\x1\\x2\\x15.\\x1\\x2\\x2.\\x2\\x2&quot;.", result);
        }
    }
}
