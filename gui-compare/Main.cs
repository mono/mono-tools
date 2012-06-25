//
// Main.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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
using Gtk;
using System.IO;

namespace GuiCompare
{
	class MainClass
	{
		
		public static void Main (string[] args)
		{
			Application.Init ();
			
 			try {
				InfoManager.Init ();
			} catch (Exception e) {
				Dialog d = new Dialog ("Error", null, DialogFlags.Modal, new object [] {
					"OK", ResponseType.Ok });
				d.VBox.Add (new Label ("There was a problem while trying to initialize the InfoManager\n\n" + e.ToString ()));
				d.VBox.ShowAll ();
				d.Run ();
				return;
			}
			
			string profile_path = null;
			if (args.Length != 0 && args[0].StartsWith ("--profile-path="))
				profile_path = args[0].Substring (15);
			
			MainWindow win = new MainWindow (profile_path);
			win.Show ();
			if (args.Length == 2 && File.Exists (args [0]) && File.Exists (args [1])){
				win.ComparePaths (args [0], args [1]);
			}
			Application.Run ();
		}
	}
}
