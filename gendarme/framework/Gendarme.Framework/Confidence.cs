// 
// Gendarme.Framework.Confidence
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
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

namespace Gendarme.Framework {

	/// <summary>
	/// The level of confidence about the rule results. The more 
	/// confidence the less likely the rule will return false positives.
	/// </summary>
	[Serializable]
	public enum Confidence {
		/// <summary>
		/// The rule is 100% certain of its result. 
		/// There should never be false positives for Total.
		/// </summary>
		Total,
		/// <summary>
		/// The rule is near 100% certain of its result.
		/// A few false-positives are possible.
		/// </summary>
		High,
		/// <summary>
		/// The rule has found a potential defect but cannot be certain of the result.
		/// Some false positive are to be expected in the results.
		/// </summary>
		Normal,
		/// <summary>
		/// The rule doesn't have enough information to be certain about the defect.
		/// Many of the results are likely to be false positives. 
		/// By default some runners wont display results if the confidence on the defect is low.
		/// </summary>
		Low
	}
}
