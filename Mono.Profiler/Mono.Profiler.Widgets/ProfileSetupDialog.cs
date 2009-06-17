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
using Gtk;

namespace Mono.Profiler.Widgets {
	
	public enum ProfileType {
		
		Allocations,
		Calls,
	}

	public class ProfileSetupDialog : Dialog {
		
		ComboBox type_combo;
		FileChooserButton assembly_button;
		
		public ProfileSetupDialog (Gtk.Window parent) : base ("Profile Options", parent, DialogFlags.DestroyWithParent, Stock.Cancel, ResponseType.Cancel, Stock.Execute, ResponseType.Accept)
		{
			HBox box = new HBox (false, 6);
			box.PackStart (new Label ("Assembly:"), false, false, 0);
			assembly_button = new FileChooserButton ("Select Assembly", FileChooserAction.Open);
			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.exe");
			assembly_button.Filter = filter;
			box.PackStart (assembly_button, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 0);
			box = new HBox (false, 6);
			box.PackStart (new Label ("Type:"), false, false, 0);
			type_combo = ComboBox.NewText ();
			type_combo.AppendText ("Allocations");
			type_combo.AppendText ("Calls");
			type_combo.Active = 1;
			box.PackStart (type_combo, false, false, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 0);
		}
		
		public string AssemblyPath {
			get { return assembly_button.Filename; }
		}
		
		public ProfileType ProfileType {
			get { return (ProfileType) type_combo.Active; }
		}
	}
}
