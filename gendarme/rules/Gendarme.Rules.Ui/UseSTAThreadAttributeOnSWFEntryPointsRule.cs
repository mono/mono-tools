//
// Gendarme.Rules.UI.UseSTAThreadAttributeOnSWFEntryPointsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2008 Daniel Abramov
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

using Mono.Cecil;

namespace Gendarme.Rules.UI {

	/// <summary>
	/// This rule checks executable assemblies, i.e. *.exe's, that reference 
	/// System.Windows.Forms to 
	/// ensure that their entry point is decorated with <c>[System.STAThread]</c> attribute 
	/// and is not decorated with <c>[System.MTAThread]</c> attribute to ensure that Windows 
	/// Forms work properly.
	/// </summary>
	/// <example>
	/// Bad example #1 (no attributes):
	/// <code>
	/// public class WindowsFormsEntryPoint {
	///	static void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example #2 (MTAThread)
	/// <code>
	/// public class WindowsFormsEntryPoint {
	///	[MTAThread]
	///	static void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example #1 (STAThread):
	/// <code>
	/// public class WindowsFormsEntryPoint {
	///     [STAThread]
	///     static void Main ()
	///     {
	///     }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example #2 (not Windows Forms):
	/// <code>
	/// public class ConsoleAppEntryPoint {
	///	static void Main ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The System.Windows.Forms application's entry-point (Main) is missing an [STAThread] attribute.")]
	[Solution ("Add a [STAThread] attribute to your application's Main method.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2232:MarkWindowsFormsEntryPointsWithStaThread")]
	public class UseSTAThreadAttributeOnSWFEntryPointsRule : Rule, IAssemblyRule {

		private const string SystemWindowsForms = "System.Windows.Forms";

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			MethodDefinition entry_point = assembly.EntryPoint;
			
			// rule applies only if the assembly has an entry point
			if (entry_point == null)
				return RuleResult.DoesNotApply;

			bool referencesSWF = false;
			foreach (AssemblyNameReference assRef in assembly.MainModule.AssemblyReferences) {
				if (assRef.Name == SystemWindowsForms) { // SWF referenced
					referencesSWF = true;
					break;
				}
			}

			// rule applies only if the assembly reference System.Windows.Forms.dll
			if (!referencesSWF)
				return RuleResult.DoesNotApply;

			bool hasSTA = entry_point.HasAttribute ("System", "STAThreadAttribute");
			bool hasMTA = entry_point.HasAttribute ("System", "MTAThreadAttribute");

			// success if only [STAThread] attribute is present
			if (hasSTA && !hasMTA)
				return RuleResult.Success;

			string text = String.Empty;
			if (!hasSTA && hasMTA)
				text = "In order for Windows Forms to work properly, replace [System.MTAThread] attribute with [System.STAThread] on the entry point.";
			else if (hasSTA && hasMTA)
				text = "In order for Windows Forms to work properly, remove [System.MTAThread] attribute from the entry point, leaving [System.STAThread] there.";
			else if (!hasSTA && !hasMTA)
				text = "In order for Windows Forms to work properly, place [System.STAThread] attribute upon the entry point.";

			// note: assembly rule reporting a method defect
			Runner.Report (entry_point, Severity.High, Confidence.Total, text);
			return RuleResult.Failure;
		}
	}
}
