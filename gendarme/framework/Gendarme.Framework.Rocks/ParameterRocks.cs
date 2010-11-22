//
// Gendarme.Framework.Rocks.ParameterRocks
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

using Mono.Cecil;

namespace Gendarme.Framework.Rocks {

	public static class ParameterRocks {

		/// <summary>
		/// Check if the parameter represents a list of parameters (<c>params</c> keyword in C#)
		/// </summary>
		/// <param name="self">The ParameterDefinition on which the extension method can be called.</param>
		/// <returns>True if the parameter represents a list of parameters, false otherwise.</returns>
		public static bool IsParams (this ParameterDefinition self)
		{
			if (self == null || !self.HasCustomAttributes)
				return false;
			return self.CustomAttributes.ContainsType ("System.ParamArrayAttribute");
		}

		/// <summary>
		/// Check if the parameter is passed by reference (<c>ref</c> keyword in C#)
		/// </summary>
		/// <param name="self">The ParameterReference on which the extension method can be called.</param>
		/// <returns>True if the parameter is passed by reference, false otherwise.</returns>
		public static bool IsRef (this ParameterReference self)
		{
			if (self == null)
				return false;
			string name = self.ParameterType.Name;
			return (name [name.Length - 1] == '&');
		}

		/// <summary>
		/// Returns the sequence number as found in the metadata
		/// </summary>
		/// <param name="self">The ParameterDefinition on which the extension method can be called.</param>
		/// <returns>The integer value of the sequence number of the parameter.</returns>
		public static int GetSequence (this ParameterDefinition self)
		{
			if (self == null)
				return -1;

			return self.Index + 1;
		}
	}
}
