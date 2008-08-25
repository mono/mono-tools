//
// Gendarme.Framework.FxCopCompatibilityAttribute class
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

namespace Gendarme.Framework {

	/// <summary>
	/// Attribute to map Gendarme rules with their (quasi)equivalent in FxCop.
	/// It's possible for a Gendarme rule to implement several FxCop rules.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class FxCopCompatibilityAttribute : Attribute {

		/// <summary>
		/// Create an mapping between the Gendarme rule and a FxCop rule
		/// </summary>
		/// <param name="category">FxCop rule category</param>
		/// <param name="checkId">FxCop rule identifier (2 letters, 4 numbers)</param>
		public FxCopCompatibilityAttribute (string category, string checkId)
		{
			Category = category;
			CheckId = checkId;
		}

		/// <summary>
		/// Rule category.
		/// e.g. "Microsoft.Usage"
		/// </summary>
		public string Category { get; internal set; }

		/// <summary>
		/// Rule identifier. The identifier is not guaranteed to be unique without it's category.
		/// e.g. "CA2232:MarkWindowsFormsEntryPointsWithStaThread" (recommanded) or
		/// or simply "CA2232" (harder to maintain)
		/// </summary>
		public string CheckId { get; internal set; }
	}
}
