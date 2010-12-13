// 
// Gendarme.Rules.NUnit.NUnitRocks
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

using Mono.Cecil;

namespace Gendarme.Rules.NUnit {

	/// <summary>
	/// NUnitRocks contains extensions methods for NUnit-related methods and types.
	/// </summary>
	public static class NUnitRocks {
		
		/// <summary>
		/// Checks if the method is a valid unit test (has corresponding attribute).
		/// </summary>
		/// <param name="self">The MethodDefenition on which the extension method can be called.</param>
		/// <returns>True if method is a unit test, false otherwise.</returns>
		public static bool IsTest (this MethodDefinition self)
		{
			if (self.HasCustomAttributes)
				foreach (CustomAttribute attribute in self.CustomAttributes)
					if (attribute.AttributeType.FullName == "NUnit.Framework.TestAttribute" ||
						attribute.AttributeType.FullName == "NUnit.Framework.TestCaseAttribute" ||
						attribute.AttributeType.FullName == "NUnit.Framework.TestCaseSourceAttribute")
						return true;
			return false;
		}
	}
}
