/***************************************************************************
 *  CSharpShell.cs, based on the BooShell.cs from Banshee.
 *
 *  Copyright (C) 2006-2008 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org>.
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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Gtk;
using Mono.CSharp;
using System.ComponentModel;

namespace Mono.CSharp.Gui
{
	[ToolboxItem (true)]
	public class Shell : TextView
	{        
		TextMark end_of_last_processing;
		string expr = null;
		
		public Shell() : base()
		{
			WrapMode = WrapMode.Word;
			CreateTags ();

			Pango.FontDescription font_description = new Pango.FontDescription();
			font_description.Family = "Monospace";
			ModifyFont(font_description);
			
			TextIter end = Buffer.EndIter;
			Buffer.InsertWithTagsByName (ref end, "Mono C# Shell, type 'help;' for help\n\nEnter statements or expressions below.\n", "Comment");
			ShowPrompt (false);

			Evaluator.InteractiveBaseClass = typeof (InteractiveGraphicsBase);
			Evaluator.Run ("using System; using System.Linq; using System.Collections; using System.Collections.Generic; using System.Drawing;");
			Evaluator.Run ("LoadAssembly (\"System.Drawing\");");
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

		//
		// Returns true if the line is complete, so that the line can be entered
		// into the history, false if this was partial
		//
		bool Evaluate (string s)
		{
			string res = null;
			object result;
			bool result_set;
			StringWriter errorwriter = new StringWriter ();

			Report.Stderr = errorwriter;
			
			try {
				res = Evaluator.Evaluate (s, out result, out result_set);
			} catch (Exception e){
				expr = null;
				ShowError (e.ToString ());
				ShowPrompt (true, false);
				return true;
			}

			// Partial input
			if (res != null){
				ShowPrompt (true, true);
				return false;
			}
			string error = errorwriter.ToString ();
			if (error.Length > 0){
				ShowError (error);
			} else {
				if (result_set)
					ShowResult (result);
			}
			ShowPrompt (true, false);
			expr = null;
			
			return true;
		}

		void HistoryUpdateLine ()
		{
			Console.WriteLine ("ADDED: {0}", GetCurrentExpression ());
			expr = null;
		}

		string GetCurrentExpression ()
		{
			string input = InputLine;

			return expr == null ? input : expr + "\n" + input;
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
				string copy;
				copy = expr = GetCurrentExpression ();
					
				if (Evaluate (expr)){
				}

				return true;
				
			case Gdk.Key.Up:
				
				return true;

			case Gdk.Key.Down:
				
				return true;
				
			case Gdk.Key.Left:
				if(Cursor.Compare(InputLineBegin) <= 0) {
					return true;
				}
				break;
				
			case Gdk.Key.Home:
				Buffer.MoveMark(Buffer.InsertMark, InputLineBegin);
				if((evnt.State & Gdk.ModifierType.ShiftMask) == evnt.State) {
					Buffer.MoveMark(Buffer.SelectionBound, InputLineBegin);
				}
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

		public void ShowResult (object res)
		{
			TextIter end = Buffer.EndIter;

			Buffer.InsertWithTagsByName (ref end, "\n", "Stdout");

			System.Drawing.Bitmap bitmap = res as System.Drawing.Bitmap;
			if (bitmap != null){
				TextChildAnchor anchor = Buffer.CreateChildAnchor (ref end);
				BitmapWidget bw = new BitmapWidget (bitmap);
				bw.Show ();
					
				AddChildAtAnchor (bw, anchor);
				
				return;
			}
			
			StringWriter pretty = new StringWriter ();
			PrettyPrint (pretty, res);
			Buffer.InsertWithTagsByName (ref end, pretty.ToString (), "Stdout");
		}

		public void ShowError (string err)
		{
			TextIter end = Buffer.EndIter;

			Buffer.InsertWithTagsByName (ref end, "\n" + err, "Error");
		}
		
		#if false
			public void SetResult (InterpreterResult result)
			{
				if(!IsRealized) {
					return;
				}
            
				TextIter end_iter = Buffer.EndIter;
            
				StringBuilder builder = new StringBuilder();
				if(result.Errors.Count > 0) {
					foreach(string error in result.Errors) {
						builder.Append(error + "\n");
					}
				} else if(result.Message == null) {
					ShowPrompt (true);
					return;
				} else {
					builder.Append(result.Message);
				}
            
				string str_result = builder.ToString().Trim();
				Buffer.Insert(ref end_iter, "\n" + str_result);
            
				TextIter start_iter = end_iter;
				start_iter.Offset -= str_result.Length;
				Buffer.ApplyTag(Buffer.TagTable.Lookup(result.Errors.Count > 0 ? "Error" : "Stdout"), 
						start_iter, end_iter);
            
				if(script_lines != null) {
					ShowPrompt (true);
				}
			}
		#endif
			private TextIter InputLineBegin {
			get { return Buffer.GetIterAtMark(end_of_last_processing); }
		}
        
		private TextIter InputLineEnd {
			get { return Buffer.EndIter; }
		}
        
		private TextIter Cursor {
			get { return Buffer.GetIterAtMark(Buffer.InsertMark); }
		}

		private string InputLine {
			get { return Buffer.GetText(InputLineBegin, InputLineEnd, false); }
			set {
				TextIter start = InputLineBegin;
				TextIter end = InputLineEnd;
				Buffer.Delete(ref start, ref end);
				start = InputLineBegin;
				Buffer.Insert(ref start, value);
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
					PrettyPrint (output, entry.Key);
					p (output, ", ");
					PrettyPrint (output, entry.Value);
					if (count != top)
						p (output, " }, ");
					else
						p (output, " }");
				}
				p (output, "}");
			} else if (result is IEnumerable) {
				int i = 0;
				p (output, "{ ");
				foreach (object item in (IEnumerable) result) {
					if (i++ != 0)
						p (output, ", ");

					PrettyPrint (output, item);
				}
				p (output, " }");
			} else if (result is char){
				EscapeChar (output, (char) result);
			} else {
				p (output, result.ToString ());
			}
		}

	}
}
