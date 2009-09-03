// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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


using System;
using System.Text;
using Gtk;

namespace Mono.Profiler.Widgets {
	
	public class ProfileSetupDialog : Dialog {
		
		ProfileConfiguration config;
		
		public ProfileSetupDialog (Gtk.Window parent) : base ("Profile Options", parent, DialogFlags.DestroyWithParent, Stock.Cancel, ResponseType.Cancel, Stock.Execute, ResponseType.Accept)
		{
			config = new ProfileConfiguration ();
			HBox box = new HBox (false, 6);
			box.PackStart (new Label ("Assembly:"), false, false, 0);
			FileChooserButton assembly_button = new FileChooserButton ("Select Assembly", FileChooserAction.Open);
			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.exe");
			assembly_button.Filter = filter;
			assembly_button.SelectionChanged += delegate { 
				config.AssemblyPath = assembly_button.Filename;
				SetResponseSensitive (ResponseType.Accept, !String.IsNullOrEmpty (assembly_button.Filename));
			};
			box.PackStart (assembly_button, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 3);
			box = new HBox (false, 6);
			box.PackStart (new Label ("Type:"), false, false, 0);
			ComboBox type_combo = ComboBox.NewText ();
			type_combo.AppendText ("Allocations");
			type_combo.AppendText ("Calls/Instrumented");
			type_combo.AppendText ("Statistical");
			type_combo.Active = 2;
			type_combo.Changed += delegate { config.Mode = (ProfileMode) (1 << type_combo.Active); };
			box.PackStart (type_combo, false, false, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 3);
			box = new HBox (false, 6);
			CheckButton start_enabled_chkbtn = new CheckButton ("Enabled at Startup");
			start_enabled_chkbtn.Active = true;
			start_enabled_chkbtn.Toggled += delegate { config.StartEnabled = start_enabled_chkbtn.Active; };
			box.PackStart (start_enabled_chkbtn, false, false, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 3);
			SetResponseSensitive (ResponseType.Accept, false);
		}

		public ProfileConfiguration Config {
			get { return config; }
		}
	}
}
