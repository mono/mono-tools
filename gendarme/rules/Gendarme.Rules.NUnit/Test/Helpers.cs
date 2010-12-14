// 
// Test.Rules.NUnit.Helpers
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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

using Test.Rules.Helpers;
using Mono.Cecil;
using NUnit.Framework;


namespace Test.Rules.NUnit {
	/// <summary>
	/// This class contains various helpers fot NUnit unit tests.
	/// </summary>
	public static class Helpers {

		/// <summary>
		/// Adds NUnit.Framework.TestAttribute to the specified ICustomAttributeProvicer
		/// </summary>
		/// <param name="self">ICustomAttributeProvider to which the attribute should be added</param>
		public static void AddTestAttribute (this ICustomAttributeProvider self)
		{
			MethodDefinition ctor = DefinitionLoader.GetMethodDefinition<TestAttribute> (".ctor");
			self.CustomAttributes.Add (new CustomAttribute (ctor));
		}
	}
}
