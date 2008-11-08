// TypeView.cs
//Authors: ${Author}
//
// Copyright (c) 2008 [copyright holders]
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
using System.IO;
using System.Collections.Generic;
using Gtk;
using Gdk;

namespace Mono.CSharp.Gui
{
	[System.ComponentModel.ToolboxItem(true)]
	public class TypeView : TextView
	{
		MainWindow container;
		
		public TypeView(MainWindow container) : base ()
		{
			WrapMode = Gtk.WrapMode.None;
			CreateTags ();

			Pango.FontDescription font_description = new Pango.FontDescription();
			font_description.Family = "Monospace";
			ModifyFont(font_description);
			this.container = container;
		}

		double  press_x = -1, press_y = -1;
		
		void CreateTags ()
		{
			TextTag keyword = new TextTag ("keyword") {
				Weight = Pango.Weight.Bold
			};

			TextTag deftag = new TextTag ("default"){
			};

			Buffer.TagTable.Add (keyword);
			Buffer.TagTable.Add (deftag);
		}

		string current_tag;

		Dictionary<Type,TextTag> typetags = new Dictionary<Type,TextTag> ();
		static int typec;
		
		string GetTypeTag (Type t)
		{
			if (typetags.ContainsKey (t)){
				return typetags [t].Name;
			}
			
			TextTag typetag = new TextTag ("t" + (typec++)){
				Foreground = "blue",
				Weight = Pango.Weight.Bold
			};
			
			typetag.TextEvent += delegate (object o, TextEventArgs e){
				EventButton eb = e.Event as EventButton;
				
				if (eb != null && eb.Button == 1){
					if (eb.Type == EventType.ButtonPress){
						press_x = eb.X;
						press_y = eb.Y;
					} else if (eb.Type == EventType.ButtonRelease){
						if (press_x != -1){
							int dx = System.Math.Abs ((int)(eb.X - press_x));
							int dy = System.Math.Abs ((int)(eb.Y - press_y));

							if (dx < 8 && dy < 8){
								container.Describe (t);
							}
						}
						press_x = -1;
					}
				}
			};

			Buffer.TagTable.Add (typetag);

			return typetag.Name;
		}
		
		public void ShowType (Type t)
		{
			Buffer.Clear ();
			
			current_tag = "default";

			GuiStream guistream = new GuiStream (null, (x,y) => {
				TextIter end = Buffer.EndIter;
				Buffer.InsertWithTagsByName (ref end, y, current_tag);
			});

			StreamWriter writer = new StreamWriter (guistream);
			Outline outliner = new Outline (t, writer, true, false, false);

			outliner.OutlineNotificationEvent += delegate (int kind, object value) {
				switch (kind){
				case 0:
					current_tag = "default";
					break;
				case 1:
					current_tag = GetTypeTag ((Type) value);
					break;
				}
			};
			outliner.OutlineType ();
			writer.Flush ();
		}
	}
}
