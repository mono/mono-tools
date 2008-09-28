// History.cs
//Authors: Miguel de Icaza
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
using System.IO;
using System.Text;

namespace Mono.CSharp.Gui
{
	//
	// taken from mcs/tools/csharp/getline.cs
	//
	// Emulates the bash-like behavior, where edits done to the
	// history are recorded
	//
	class History {
		string [] history;
		int head, tail;
		int cursor, count;
		string histfile;
		
		public History (string app, int size)
		{
			if (size < 1)
				throw new ArgumentException ("size");

			if (app != null){
				string dir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				//Console.WriteLine (dir);
				if (!Directory.Exists (dir)){
					try {
						Directory.CreateDirectory (dir);
					} catch {
						app = null;
					}
				}
				if (app != null)
					histfile = Path.Combine (dir, app) + ".history";
			}
			
			history = new string [size];
			head = tail = cursor = 0;

			if (File.Exists (histfile)){
				using (StreamReader sr = File.OpenText (histfile)){
					string line;
					
					while ((line = sr.ReadLine ()) != null){
						if (line != "")
							Append (line);
					}
				}
			}
		}

		public void Close ()
		{
			if (histfile == null)
				return;

			try {
				using (StreamWriter sw = File.CreateText (histfile)){
					int start = (count == history.Length) ? head : tail;
					for (int i = start; i < start+count; i++){
						int p = i % history.Length;
						sw.WriteLine (history [p]);
					}
				}
			} catch {
				// ignore
			}
		}
		
		//
		// Appends a value to the history
		//
		public void Append (string s)
		{
			//Console.WriteLine ("APPENDING {0} {1}", s, Environment.StackTrace);
			history [head] = s;
			head = (head+1) % history.Length;
			if (head == tail)
				tail = (tail+1 % history.Length);
			if (count != history.Length)
				count++;
		}

		//
		// Updates the current cursor location with the string,
		// to support editing of history items.   For the current
		// line to participate, an Append must be done before.
		//
		public void Update (string s)
		{
			history [cursor] = s;
		}

		public void RemoveLast ()
		{
			head = head-1;
			if (head < 0)
				head = history.Length-1;
		}
		
		public void Accept (string s)
		{
			int t = head-1;
			if (t < 0)
				t = history.Length-1;
			
			history [t] = s;
		}
		
		public bool PreviousAvailable ()
		{
			//Console.WriteLine ("h={0} t={1} cursor={2}", head, tail, cursor);
			if (count == 0 || cursor == tail)
				return false;

			return true;
		}

		public bool NextAvailable ()
		{
			int next = (cursor + 1) % history.Length;
			if (count == 0 || next > head)
				return false;

			return true;
		}
		
		
		//
		// Returns: a string with the previous line contents, or
		// nul if there is no data in the history to move to.
		//
		public string Previous ()
		{
			if (!PreviousAvailable ())
				return null;

			cursor--;
			if (cursor < 0)
				cursor = history.Length - 1;

			return history [cursor];
		}

		public string Next ()
		{
			if (!NextAvailable ())
				return null;

			cursor = (cursor + 1) % history.Length;
			return history [cursor];
		}

		public void CursorToEnd ()
		{
			if (head == tail)
				return;

			cursor = head;
		}

		public void Dump ()
		{
			Console.WriteLine ("Head={0} Tail={1} Cursor={2}", head, tail, cursor);
			for (int i = 0; i < history.Length;i++){
				Console.WriteLine (" {0} {1}: {2}", i == cursor ? "==>" : "   ", i, history[i]);
			}
			//log.Flush ();
		}

		public string SearchBackward (string term)
		{
			for (int i = 1; i < count; i++){
				int slot = cursor-i;
				if (slot < 0)
					slot = history.Length-1;
				if (history [slot] != null && history [slot].IndexOf (term) != -1){
					cursor = slot;
					return history [slot];
				}

				// Will the next hit tail?
				slot--;
				if (slot < 0)
					slot = history.Length-1;
				if (slot == tail)
					break;
			}

			return null;
		}
		
	}
}
		