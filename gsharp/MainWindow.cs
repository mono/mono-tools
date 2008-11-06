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
using System.Collections;
using Gtk;
using Mono.Attach;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Mono.CSharp.Gui
{
	public partial class MainWindow : Gtk.Window
	{
		Shell shell;
		bool panevisible;
		
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

		delegate string ReadLiner (bool primary);

		public void LoadStartupFiles ()
		{
			string dir = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
				"gsharp");
			if (!System.IO.Directory.Exists (dir))
				return;

			LoadFiles (System.IO.Directory.GetFiles (dir), true);
		}

		public void LoadFiles (IEnumerable list, bool silent)
		{
			ArrayList sources = new ArrayList ();
			ArrayList libraries = new ArrayList ();
				
			foreach (string file in list){
				string l = file.ToLower ();
					
				if (l.EndsWith (".cs"))
					sources.Add (file);
				else if (l.EndsWith (".dll"))
					libraries.Add (file);
			}
				
			foreach (string file in libraries){
				Evaluator.LoadAssembly (file);
			}
			
			foreach (string file in sources){
				try {
					using (System.IO.StreamReader r = System.IO.File.OpenText (file)){
						ReadEvalPrintLoopWith (p => r.ReadLine ());
					}
				} catch {
					if (!silent)
						throw;
				}
			}
		}
	
		string Evaluate (string input)
		{
			bool result_set;
			object result;

			try {
				input = Evaluator.Evaluate (input, out result, out result_set);
			} catch (Exception e){
				Console.WriteLine (e);
				return null;
			}
			
			return input;
		}

		void ReadEvalPrintLoopWith (ReadLiner readline)
		{
			string expr = null;
			while (true){
				string input = readline (expr == null);
				if (input == null)
					return;

				if (input == "")
					continue;

				expr = expr == null ? input : expr + "\n" + input;
				
				expr = Evaluate (expr);
			} 
		}

		static string settings;
		
		static void InitSettings ()
		{
			string dir = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "gsharp");

			try {
				System.IO.Directory.CreateDirectory (dir);
			} catch {
				return;
			}
			settings = System.IO.Path.Combine (dir, "gsharp.xml");
		}

		static MainWindow ()
		{
			InitSettings ();
		}

		public void LoadSettings (Shell shell)
		{
			if (settings == null){
				shell.LoadHistory (null);
				return;
			}

			XDocument doc;
			try {
				doc = XDocument.Load (settings);
			} catch {
				shell.LoadHistory (null);
				return;
			}

			int width = -1;
			int height = -1;
			int position = -1;
			
			try {
				foreach (var e in doc.Root.Elements ()){
					switch (e.Name.LocalName){
					case "history":
						shell.LoadHistory (e.Elements ());
						break;
					case "Width":
						width = Int32.Parse (e.Value);
						break;
					case "Height":
						height = Int32.Parse (e.Value);
						break;
					case "PaneVisible":
						panevisible = Int32.Parse (e.Value) == 1;
						break;
					case "PanePosition":
						position = Int32.Parse (e.Value);
						break;
					}
				}
			} catch {
			}

			width = System.Math.Max (width, 200);
			height = System.Math.Max (height, 120);
			
			Resize (width, height);
			if (position >= 0)
				hpaned.Position = position;
		}

		public void SaveSettings ()
		{
			if (settings == null)
				return;

			try {
				var doc = new XDocument (
					new XElement ("Root", 
						      shell.SaveHistory (),
						      new XElement ("Width", Allocation.Width),
						      new XElement ("Height", Allocation.Height),
						      new XElement ("PaneVisible", PaneVisible ? 1 : 0),
						      new XElement ("PanePosition", hpaned.Position)));
				doc.Save (settings, SaveOptions.DisableFormatting);
			} catch (Exception e ) {
				Console.WriteLine ("oops" + e);
			}
		}

		public bool PaneVisible {
			get {
				return notebook1.Page == 0;
			}
		}
		
		//
		// Keeps track of the helper Gtk Pane used for toying with Gtk# widgets
		//
		void SetDisplayPane (bool visible)
		{
			if (!(PaneVisible ^ visible))
				return;
				
			if (visible){
				notebook1.Page = 0;
				sw.Reparent (paned_container);
			} else {
				notebook1.Page = 1;
				sw.Reparent (standalone_container);
			}
		}

		public Container PaneContainer {
			get {
				return eventbox;
			}
		}
		
		public MainWindow() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			Title = "C# Shell";

			// This is the page with no pane
			notebook1.Page = 1;
			gtkpane.Toggled += delegate {
				SetDisplayPane (gtkpane.Active);
			};
			
			shell = new Shell ();
			LoadSettings (shell);

			gtkpane.Active = panevisible;

			shell.QuitRequested += OnQuitActionActivated;
			
			shell.ShowAll ();
			
			sw.Add (shell);
			Focus = shell;
		}

		protected virtual void OnQuitActionActivated (object sender, System.EventArgs e)
		{
			SaveSettings ();
			
			if (MainClass.Attached){
				this.Destroy ();
			} else {
				Application.Quit ();
			}
		}

		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			if (!MainClass.Attached)
				Application.Quit ();
		}
		
	}
}
