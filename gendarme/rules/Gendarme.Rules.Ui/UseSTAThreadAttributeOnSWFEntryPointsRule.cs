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


using Gendarme.Framework;
using Gendarme.Framework.Rocks;

using Mono.Cecil;

namespace Gendarme.Rules.Ui {

	public class UseSTAThreadAttributeOnSWFEntryPointsRule : IAssemblyRule {
		private const string SystemWindowsForms = "System.Windows.Forms";

		private const string STAThread = "System.STAThreadAttribute";
		private const string MTAThread = "System.MTAThreadAttribute";

		public MessageCollection CheckAssembly (AssemblyDefinition assembly, Runner runner)
		{
			if (assembly.EntryPoint == null)
				return runner.RuleSuccess;

			bool referencesSWF = false;
			foreach (AssemblyNameReference assRef in assembly.MainModule.AssemblyReferences) {
				if (assRef.Name == SystemWindowsForms) { // SWF referenced
					referencesSWF = true;
					break;
				}
			}

			if (!referencesSWF)
				return runner.RuleSuccess;

			MethodDefinition entryPoint = assembly.EntryPoint;
			bool hasSTA = entryPoint.HasAttribute (STAThread);
			bool hasMTA = entryPoint.HasAttribute (MTAThread);

			if (hasSTA && !hasMTA)
				return runner.RuleSuccess;

			string text = string.Empty;
			if (!hasSTA && hasMTA)
				text = "In order for Windows Forms to work properly, replace [System.MTAThread] attribute with [System.STAThread] on the entry point.";
			else if (hasSTA && hasMTA)
				text = "In order for Windows Forms to work properly, remove [System.MTAThread] attribute from the entry point, leaving [System.STAThread] there.";
			else if (!hasSTA && !hasMTA)
				text = "In order for Windows Forms to work properly, place [System.STAThread] attribute upon the entry point.";

			Location loc = new Location (entryPoint);
			Message msg = new Message (text, loc, MessageType.Error);
			return new MessageCollection (msg);
		}
	}
}
