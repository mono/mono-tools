// 
// Test.Rules.Ui.AddMatchingArrangeMeasureOverrideTest
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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
using System.Reflection;
using System.Windows;

using Mono.Cecil;
using Gendarme.Rules.UI;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

// Mono 2.8 does not provide the PresentationFramework.dll assembly so we fake what we need to test (when compiled with xMCS)
#if __MonoCS__
namespace System.Windows {
	public class FrameworkElement {
		protected virtual Size ArrangeOverride (Size finalSize)
		{
			return new Size ();
		}
		protected virtual Size MeasureOverride (Size availableSize)
		{
			return new Size ();
		}
	}
}
#endif

namespace Test.Rules.Ui {

	public class BasicFrameworkElement : FrameworkElement {
	}
	public class GoodFrameworkElement : FrameworkElement {
		protected override Size ArrangeOverride (Size finalSize)
		{
			return base.ArrangeOverride (finalSize);
		}
		protected override Size MeasureOverride (Size availableSize)
		{
			return base.MeasureOverride (availableSize);
		}
	}
	public class BadArrangeOnlyElement : FrameworkElement {
		protected override Size ArrangeOverride (Size finalSize)
		{
			return base.ArrangeOverride (finalSize);
		}
	}
	public class BadMeasureOnlyElement : FrameworkElement {
		protected override Size MeasureOverride (Size availableSize)
		{
			return base.MeasureOverride (availableSize);
		}
	}

	[TestFixture]
	public class AddMatchingArrangeMeasureOverrideTest : TypeRuleTestFixture<AddMatchingArrangeMeasureOverrideRule> {
		[Test]
		public void DoesNotApply ()
		{
			// Doesn't inhert from FrameworkElement
			AssertRuleDoesNotApply (SimpleTypes.Class);

			// Aren't classes
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);

			// Doesn't implement either method.
			AssertRuleDoesNotApply<BasicFrameworkElement> ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodFrameworkElement> ();
		}

		[Test]
		public void Bad ()
		{
			// Only overrides ArrangeOverride method.
			AssertRuleFailure<BadArrangeOnlyElement> (1);

			// Only overrides MeasureOverride method.
			AssertRuleFailure<BadMeasureOnlyElement> (1);
		}
	}
}
