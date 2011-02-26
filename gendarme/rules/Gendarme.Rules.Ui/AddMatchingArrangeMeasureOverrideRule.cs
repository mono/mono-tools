// 
// Gendarme.Rules.Ui.AddMatchingArrangeMeasureOverrideRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.UI {

	/// <summary>
	/// An object that inherits from System.Windows.FrameworkElement and provides either 
	/// an ArrangeOverride or MeasureOverride method should also provide the other.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class BadClass : System.Windows.FrameworkElement {
	///	protected override Size MeasureOverride (Size availableSize)
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class GoodClass : System.Windows.FrameworkElement {
	/// 	protected override Size MeasureOverride (Size availableSize)
	/// 	{
	/// 	}
	/// 	protected override Size ArrangeOverride (Size finalSize)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	[Problem ("A FrameworkElement provides either an ArrangeOverride or MeasureOverride method, but not the other.")]
	[Solution ("Add the counterpart ArrangeOverride or MeasureOverride method.")]
	public class AddMatchingArrangeMeasureOverrideRule : Rule, ITypeRule {
		private const string Size = "System.Windows.Size";
		private static string [] parameters = { Size };
		private static Func<MethodReference, bool> checkOverride = method => method.IsOverride ();
		private static MethodSignature arrangeSignature = new MethodSignature ("ArrangeOverride",
									Size, parameters, checkOverride);
		private static MethodSignature measureSignature = new MethodSignature ("MeasureOverride",
									Size, parameters, checkOverride);

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference System.Windows.Size, 
			// then it will not be using the overrides
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				string assembly_name = e.CurrentAssembly.Name.Name;
				Active = ((assembly_name == "WindowsBase" || assembly_name == "System.Windows") ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Windows", "Size");
					})
				);
			};
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || !type.HasMethods || !type.Inherits ("System.Windows", "FrameworkElement"))
				return RuleResult.DoesNotApply;
			var arrangeOverride = type.GetMethod (arrangeSignature);
			var measureOverride = type.GetMethod (measureSignature);
			// The class doesn't have either method.
			if (arrangeOverride == null && measureOverride == null)
				return RuleResult.DoesNotApply;
			// Only ArrangeOverride provided.
			if (arrangeOverride != null && measureOverride == null)
				Runner.Report (arrangeOverride, Severity.High, Confidence.Total);
			// Only MeasureOverride provided.
			if (measureOverride != null && arrangeOverride == null)
				Runner.Report (measureOverride, Severity.High, Confidence.Total);

			return Runner.CurrentRuleResult;
		}
	}


}
