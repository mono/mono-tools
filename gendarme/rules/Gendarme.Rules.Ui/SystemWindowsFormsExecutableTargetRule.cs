//
// Gendarme.Rules.Ui.SystemWindowsFormsExecutableTargetRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.UI {

	/// <summary>
	/// An executable assembly, i.e. an .exe, refers to the System.Windows.Forms assembly 
	/// but isn't compiled using <c>-target:winexe</c>. A console window will be created 
	/// and shown under Windows (MS runtime) when the application is executed which is
	/// probably not desirable for a winforms application.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <c>gmcs swf.cs -pkg:dotnet</c>
	/// </example>
	/// <example>
	/// Good example:
	/// <c>gmcs swf.cs -pkg:dotnet -target:winexe</c>
	/// </example>

	[Problem ("The assembly refers to the 'System.Windows.Forms.dll' assembly but isn't compiled using /target:winexe. A console window will be shown under Windows.")]
	// The base class has the solution text.
	public class SystemWindowsFormsExecutableTargetRule : ExecutableTargetRule {

		protected override string AssemblyName {
			get { return "System.Windows.Forms"; }
		}

		protected override byte[] GetAssemblyPublicKeyToken ()
		{
			return new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
		}
	}
}
