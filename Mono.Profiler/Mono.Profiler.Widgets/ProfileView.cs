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
using Mono.Profiler;

namespace Mono.Profiler.Widgets {
	
	[System.ComponentModel.ToolboxItem (true)]
	public class ProfileView : Gtk.ScrolledWindow {
		
		string path;
		ProfileType type;
		
		public string LogFile {
			get { return path; }
			set {
				path = value;
				SyncLogFileReader rdr = new SyncLogFileReader (path);
				ProfilerEventHandler data = new ProfilerEventHandler ();
				data.LoadedElements.RecordHeapSnapshots = false;
				while (!rdr.HasEnded) {
					BlockData current = null;
					try {
						current = rdr.ReadBlock ();
						current.Decode (data, rdr);
					} catch (DecodingException e) {
						Console.Error.WriteLine ("Stopping decoding after a DecodingException in block of code {0}, length {1}, file offset {2}, block offset {3}: {4}", e.FailingData.Code, e.FailingData.Length, e.FailingData.FileOffset, e.OffsetInBlock, e.Message);
						break;
					}
				}
				Gtk.Widget view;
				if (type == ProfileType.Allocations) 
					view = new AllocationsView (data);
				else
					view = new CallsView (data);
				view.ShowAll ();
				View = view;
			}
		}
		
		public ProfileType ProfileType {
			get { return type; }
			set { type = value; }
		}
		
		Gtk.Widget View {
			get { return Child; }
			set {
				if (Child != null)
					Remove (Child);
				if (value != null)
					Add (value);
			}
		}
	}
}
