/***************************************************************************
 *  CSharpShell.cs, based on the BooShell.cs from Banshee.
 *
 *  Copyright (C) 2006-2008 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>.
 *             Miguel de Icaza (miguel@gnome.org).
 * 
 *  Based on ShellTextView.boo in the MonoDevelop Boo Binding
 *  originally authored by  Peter Johanson <latexer@gentoo.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Gtk;
using Mono.CSharp;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Linq;

namespace Mono.CSharp.Gui
{
	[ToolboxItem (true)]
	public class Shell : TextView
	{        
		TextMark end_of_last_processing;
		string expr;
		Evaluator evaluator;
		Report report;

		List<string> history = new List<string> ();
		int history_cursor;

		MainWindow container;
		public MainWindow Container { get { return container; }}
		
		public Shell(MainWindow container) : base()
		{
			this.container = container;
			WrapMode = WrapMode.Word;
			CreateTags ();

			Pango.FontDescription font_description = new Pango.FontDescription();
			font_description.Family = "Monospace";
			ModifyFont(font_description);
			
			TextIter end = Buffer.EndIter;
			Buffer.InsertWithTagsByName (ref end, "Mono C# Shell, type 'help;' for help\n\nEnter statements or expressions below.\n", "Comment");
			ShowPrompt (false);
			
			report = new Report (new ConsoleReportPrinter ());
			evaluator = new Evaluator (new CompilerSettings (), report);
			evaluator.DescribeTypeExpressions = true;
			
			evaluator.InteractiveBaseClass = typeof (InteractiveGraphicsBase);
			evaluator.Run ("LoadAssembly (\"System.Drawing\");");
			evaluator.Run ("using System; using System.Linq; using System.Collections; using System.Collections.Generic; using System.Drawing;");

			if (!MainClass.Debug){
				GuiStream error_stream = new GuiStream ("Error", (x, y) => Output (x, y));
				StreamWriter gui_output = new StreamWriter (error_stream);
				gui_output.AutoFlush = true;
				Console.SetError (gui_output);

				GuiStream stdout_stream = new GuiStream ("Stdout", (x, y) => Output (x, y));
				gui_output = new StreamWriter (stdout_stream);
				gui_output.AutoFlush = true;
				Console.SetOut (gui_output);
			}
		}

		void CreateTags ()
		{
			TextTag freeze_tag = new TextTag("Freezer") {
				Editable = false
			};
			Buffer.TagTable.Add(freeze_tag);

			TextTag prompt_tag = new TextTag("Prompt") {
				Foreground = "blue",
				//Background = "#f8f8f8",
				Weight = Pango.Weight.Bold
			};
			Buffer.TagTable.Add(prompt_tag);
            
			TextTag prompt_continuation_tag = new TextTag("PromptContinuation") {
				Foreground = "orange",
				//Background = "#f8f8f8",
				Weight = Pango.Weight.Bold
			};
			Buffer.TagTable.Add(prompt_continuation_tag);
            
			TextTag error_tag = new TextTag("Error") {
				Foreground = "red"
			};
			Buffer.TagTable.Add(error_tag);
            
			TextTag stdout_tag = new TextTag("Stdout") {
				Foreground = "#006600"
			};
			Buffer.TagTable.Add(stdout_tag);

			TextTag comment = new TextTag ("Comment") {
				Foreground = "#3f7f5f"
			};
			Buffer.TagTable.Add (comment);
		}

		public event EventHandler QuitRequested;
		
		//
		// Returns true if the line is complete, so that the line can be entered
		// into the history, false if this was partial
		//
		public bool Evaluate (string s)
		{
			string res = null;
			object result;
			bool result_set;
			StringWriter errorwriter = new StringWriter ();
			
			var old_printer = report.SetPrinter (new StreamReportPrinter (errorwriter));
			
			try {
				res = evaluator.Evaluate (s, out result, out result_set);
			} catch (Exception e){
				expr = null;
				ShowError (e.ToString ());
				ShowPrompt (true, false);
				return true;
			} finally {
				report.SetPrinter (old_printer);
			}

			// Partial input
			if (res != null){
				ShowPrompt (false, true);
				return false;
			}
			string error = errorwriter.ToString ();
			if (error.Length > 0){
				ShowError (error);
				ShowPrompt (false, false);
			} else {
				if (result_set){
					ShowResult (result);
					ShowPrompt (true, false);
				} else
					ShowPrompt (false, false);
			}
			expr = null;
			
			return true;
		}

		string GetCurrentExpression ()
		{
			string input = InputLine;

			return expr == null ? input : expr + "\n" + input;
		}

		[Conditional("DEBUG_HISTORY")]
		void DumpHistory ()
		{
			for (int i = 0; i < history.Count; i++)
				Console.WriteLine ("{0}  {1}: {2}",
						   i == history_cursor ? "==>" : "   ",
						   i, history [i]);
			Console.WriteLine ("--- {0} --- ", history_cursor);
		}
		
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if(Cursor.Compare (InputLineBegin) < 0) {
				Buffer.MoveMark(Buffer.SelectionBound, InputLineEnd);
				Buffer.MoveMark(Buffer.InsertMark, InputLineEnd);
			}

			switch (evnt.Key){
			case Gdk.Key.Return:
			case Gdk.Key.KP_Enter:
				string input_line = InputLine;
				history [history.Count-1] = input_line;
				history_cursor = history.Count;
				
				string expr_copy = expr = GetCurrentExpression ();

				// Insert a new line before we evaluate.
				TextIter end = Buffer.EndIter;
				Buffer.InsertWithTagsByName (ref end, "\n", "Stdout");

				if (Evaluate (expr)){
					if (expr_copy != input_line){
						history.Add (expr_copy);
						history_cursor = history.Count;
					}
				}
				history.Add ("");
				DumpHistory ();
				if (InteractiveBase.QuitRequested && QuitRequested != null)
					QuitRequested (this, EventArgs.Empty);
				return true;
				
			case Gdk.Key.Up:
				if (history_cursor == 0){
					DumpHistory ();
					return true;
				}
				string input = InputLine;
				if (!String.IsNullOrEmpty (input)){
					DumpHistory ();
					history [history_cursor] = input;
				}
				history_cursor--;
				InputLine = (string) history [history_cursor];
				DumpHistory ();
				return true;

			case Gdk.Key.Down:
				if (history_cursor+1 >= history.Count){
					DumpHistory ();
					return true;
				}
				history_cursor++;
				InputLine = (string) history [history_cursor];
				DumpHistory ();
				return true;
				
			case Gdk.Key.Left:
				if(Cursor.Compare(InputLineBegin) <= 0) {
					return true;
				}
				break;
				
			case Gdk.Key.Home:
				Buffer.MoveMark(Buffer.InsertMark, InputLineBegin);
				if((evnt.State & Gdk.ModifierType.ShiftMask) != Gdk.ModifierType.ShiftMask) {
					Buffer.MoveMark(Buffer.SelectionBound, InputLineBegin);
				}
				return true;

			case Gdk.Key.Tab:
				string saved_text = InputLine;
				string prefix;
				string [] completions = evaluator.GetCompletions (LineUntilCursor, out prefix);
				if (completions == null)
					return true;

				if (completions.Length == 1){
					TextIter cursor = Cursor;
					Buffer.Insert (ref cursor, completions [0]);
					return true;
				}
					
				Console.WriteLine ();
				foreach (var s in completions){
					Console.Write (prefix);
					Console.Write (s);
					Console.Write (" ");
				}
				// Insert a new line before we evaluate.
				end = Buffer.EndIter;
				Buffer.InsertWithTagsByName (ref end, "\n", "Stdout");
				ShowPrompt (false);
				InputLine = saved_text;
#if false
				Gtk.TextIter start = Cursor;
				if (prefix.Length != 0)
					MoveVisually (ref start, -prefix.Length);
				int x, y;
				GdkWindow.GetOrigin (out x, out y);
				var r = GetIterLocation (start);
				x += r.X;
				y += r.Y;
				var w = new Gtk.Window (WindowType.Popup);
				w.SetUposition (x, y);
				w.SetUsize (100, 100);
				foreach (var s in completions){
					Console.WriteLine ("{0}[{1}]", prefix, s);
				}
				w.ShowAll ();
				Console.WriteLine ("Position: x={0} y={1}", x + r.X, y +r.Y);
#endif
				return true;
				
			default:
				break;
			}
			
			return base.OnKeyPressEvent(evnt);
		}
        
		public void ShowPrompt(bool newline)
		{
			ShowPrompt (newline, false);
		}
        
		private void ShowPrompt (bool newline, bool continuation)
		{
			TextIter end_iter = Buffer.EndIter;
            
			if(newline) {
				Buffer.Insert(ref end_iter, "\n");
			}

			string prompt = continuation ? InteractiveBase.ContinuationPrompt : InteractiveBase.Prompt;
			Buffer.Insert(ref end_iter, prompt);
            
			Buffer.PlaceCursor(Buffer.EndIter);
			ScrollMarkOnscreen(Buffer.InsertMark);
            
			end_of_last_processing = Buffer.CreateMark(null, Buffer.EndIter, true);
			Buffer.ApplyTag(Buffer.TagTable.Lookup("Freezer"), Buffer.StartIter, InputLineBegin);
            
			TextIter prompt_start_iter = InputLineBegin;
			prompt_start_iter.LineIndex -= prompt.Length;
            
			TextIter prompt_end_iter = InputLineBegin;
			prompt_end_iter.LineIndex -= 1;
            
			Buffer.ApplyTag(Buffer.TagTable.Lookup(continuation ? "PromptContinuation" : "Prompt"), 
					prompt_start_iter, prompt_end_iter);
		}

		public void Output (string kind, string s)
		{
			TextIter end = Buffer.EndIter;
			Buffer.InsertWithTagsByName (ref end, s, kind);
		}
		
		public void ShowResult (object res)
		{
			TextIter end = Buffer.EndIter;

			var handlers = new List<InteractiveGraphicsBase.TransformHandler> (InteractiveGraphicsBase.type_handlers);

			//object original = res;
			bool retry;
			do {
				retry = false;
				foreach (var render_handler in handlers){
					object transformed = render_handler (res);
					if (transformed == null || transformed == res)
						continue;
					
					if (transformed is Gtk.Widget){
						Gtk.Widget w = (Gtk.Widget) transformed;
						TextChildAnchor anchor = Buffer.CreateChildAnchor (ref end);
						w.Show ();
						AddChildAtAnchor (w, anchor);
						return;
					} else {
						res = transformed;
						handlers.Remove (render_handler);
						retry = true;
						break;
					}
				}
			} while (retry && handlers.Count > 0);

			StringWriter pretty = new StringWriter ();
			try {
				PrettyPrint (pretty, res);
			} catch (Exception e) {
				Console.WriteLine (e);
			}
			Buffer.InsertWithTagsByName (ref end, pretty.ToString (), "Stdout");
		}

		public void ShowError (string err)
		{
			TextIter end = Buffer.EndIter;

			Buffer.InsertWithTagsByName (ref end, err, "Error");
		}
		
		TextIter InputLineBegin {
			get { return Buffer.GetIterAtMark(end_of_last_processing); }
		}
        
		TextIter InputLineEnd {
			get { return Buffer.EndIter; }
		}
        
		TextIter Cursor {
			get { return Buffer.GetIterAtMark(Buffer.InsertMark); }
		}
		
		public Evaluator Evaluator {
			get {
				return evaluator;
			}
		}		

		string InputLine {
			get { return Buffer.GetText(InputLineBegin, InputLineEnd, false); }
			set {
				TextIter start = InputLineBegin;
				TextIter end = InputLineEnd;
				Buffer.Delete(ref start, ref end);
				start = InputLineBegin;
				Buffer.Insert(ref start, value);
				ScrollMarkOnscreen(Buffer.InsertMark);
			}
		}

		string LineUntilCursor {
			get {
				return Buffer.GetText (InputLineBegin, Cursor, false);
			}
		}
		
		static void p (TextWriter output, string s)
		{
			output.Write (s);
		}

		static string EscapeString (string s)
		{
			return s.Replace ("\"", "\\\"");
		}

		static void EscapeChar (TextWriter output, char c)
		{
			if (c == '\''){
				output.Write ("'\\''");
				return;
			}
			if (c > 32){
				output.Write ("'{0}'", c);
				return;
			}
			switch (c){
			case '\a':
				output.Write ("'\\a'");
				break;

			case '\b':
				output.Write ("'\\b'");
				break;
				
			case '\n':
				output.Write ("'\\n'");
				break;
				
			case '\v':
				output.Write ("'\\v'");
				break;
				
			case '\r':
				output.Write ("'\\r'");
				break;
				
			case '\f':
				output.Write ("'\\f'");
				break;
				
			case '\t':
				output.Write ("'\\t");
				break;

			default:
				output.Write ("'\\x{0:x}", (int) c);
				break;
			}
		}

		internal XElement SaveHistory ()
		{
			var doc = new XElement (
				"history",
				from x in history
				select new XElement ("item", x));

			return doc;
		}

		internal void LoadHistory (IEnumerable<XElement> items)
		{
			if (items == null){
				history.Add ("");
				return;
			}

			foreach (var e in items){
				string s = e.Value;
				if (s == null || s.Length == 0)
					continue;
				
				history.Add (s);
			}
			history.Add ("");
			history_cursor = history.Count-1;
		}
		
		internal static void PrettyPrint (TextWriter output, object result)
		{
			if (result == null){
				p (output, "null");
				return;
			}
			
			if (result is Array){
				Array a = (Array) result;
				
				p (output, "{ ");
				int top = a.GetUpperBound (0);
				for (int i = a.GetLowerBound (0); i <= top; i++){
					PrettyPrint (output, a.GetValue (i));
					if (i != top)
						p (output, ", ");
				}
				p (output, " }");
			} else if (result is bool){
				if ((bool) result)
					p (output, "true");
				else
					p (output, "false");
			} else if (result is string){
				p (output, String.Format ("\"{0}\"", EscapeString ((string)result)));
			} else if (result is IDictionary){
				IDictionary dict = (IDictionary) result;
				int top = dict.Count, count = 0;
				
				p (output, "{");
				foreach (DictionaryEntry entry in dict){
					count++;
					p (output, "{ ");
					try {
						PrettyPrint (output, entry.Key);
					} catch {
						p (output, "<error>");
					}
					p (output, ", ");
					try {
						PrettyPrint (output, entry.Value);
					} catch {
						p (output, "<error>");
					}
					if (count != top)
						p (output, " }, ");
					else
						p (output, " }");
				}
				p (output, "}");
			} else if (result is IEnumerable) {
				int i = 0;
				p (output, "{ ");
				try {
					foreach (object item in (IEnumerable) result) {
						if (i++ != 0)
							p (output, ", ");
						
						PrettyPrint (output, item);
					}
				} catch {
					p (output, "<error>");
				}
				p (output, " }");
			} else if (result is char){
				EscapeChar (output, (char) result);
			} else {
				try {
					p (output, result.ToString ());
				} catch {
					p (output, "<error>");
				}
			}
		}

	}

	public class GuiStream : Stream {
		string kind;
		Action<string,string> callback;
		
		public GuiStream (string k, Action<string, string> cb)
		{
			kind = k;
			callback = cb;
		}
		
		public override bool CanRead { get { return false; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return true; } }


		public override long Length { get { return 0; } } 
		public override long Position { get { return 0; } set {} }
		public override void Flush () { }
		public override int Read  ([In,Out] byte[] buffer, int offset, int count) { return -1; }

		public override long Seek (long offset, SeekOrigin origin) { return 0; }

		public override void SetLength (long value) { }

		public override void Write (byte[] buffer, int offset, int count) {
			callback (kind, Encoding.UTF8.GetString (buffer, offset, count));
		}
	}
}
