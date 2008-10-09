// MainWindow.cs
//Authors: Miguel de Icaza (miguel@novell.com)
//
// Copyright (c) 2008 Novell, Inc.
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
//
//

using System;
using Gtk;
using Mono.Attach;
using System.Runtime.InteropServices;

namespace Mono.CSharp.Gui
{
	public partial class MainWindow : Gtk.Window
	{
		Shell shell;
		
		
		protected virtual void OnAttachToProcessActionActivated (object sender, System.EventArgs e)
		{
			ProcessSelector p = new ProcessSelector ();
			int c = p.Run ();
			if (c == (int) ResponseType.Ok){
				VirtualMachine vm = new VirtualMachine (p.PID);

				vm.Attach (typeof (MainWindow).Assembly.Location, "--agent");
			}
			
			p.Destroy ();
		}

		
		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			Title = "C# Shell";

			shell = new Shell ();
			shell.ShowAll ();
			
			sw.Add (shell);
			Focus = shell;
		}

		protected virtual void OnQuitActionActivated (object sender, System.EventArgs e)
		{
			if (MainClass.Attached){
				this.Destroy ();
			} else {
				Application.Quit ();
			}
		}
		
	}
}
