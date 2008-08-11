// Author:
// Massimiliano Mantione (massi@ximian.com)
//
// (C) 2008 Novell, Inc  http://www.novell.com
//

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
using System.IO;
using Gtk;

namespace Mono.Profiler
{
	class HeapSnapshotViewerMain
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine ("Please specify one input file");
				return;
			}

			SeekableLogFileReader reader = null;

			try {
				reader = new SeekableLogFileReader (args [0]);
			} catch (IOException e){
				Console.Error.WriteLine ("It was not possible to open the file {0}", args [0]);
				return;
			}
			
			foreach (SeekableLogFileReader.Block block in reader.Blocks) {
				Console.WriteLine ("Found block {0} at offset {1} (length {2}, time from start {3})", block.Code, block.FileOffset, block.Length, block.TimeFromStart);
			}
			
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			
			win.HeapExplorer.Model = new HeapExplorerTreeModel (reader);
			win.HeapExplorer.Model.Initialize ();
			
			Application.Run ();
		}
	}
}