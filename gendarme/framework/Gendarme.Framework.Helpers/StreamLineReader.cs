//
// StreamLineReader - A StringReader-like class that avoid creating string
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Framework.Helpers {

	// note: inheriting from StreamReader was not possible since we cannot
	// override EndOfStream and ensure integrity with other Read ops
	public class StreamLineReader : IDisposable {

		StreamReader sr;
		char [] buff;
		int n;
		int max;

		public StreamLineReader (string fileName)
		{
			sr = new StreamReader (fileName);
			Initialize ();
		}

		public StreamLineReader (Stream stream)
		{
			sr = new StreamReader (stream);
			Initialize ();
		}

		void Initialize ()
		{
			buff = new char [4096];
			max = n = buff.Length;
		}

		public bool EndOfStream {
			get { return (n == max) && sr.EndOfStream; }
		}

		public int ReadLine (char [] buffer, int index, int count)
		{
			if (Disposed)
				throw new ObjectDisposedException ("StreamLineReader");
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// ordered to avoid possible integer overflow
			if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			int len = 0;
			while (len < count) {
				if (n == max) {
					max = sr.ReadBlock (buff, 0, buff.Length);
					n = 0;
				}
				char c = buff [n++];
				switch (c) {
				case '\r':
					continue;
				case '\n':
					Array.Clear (buffer, len, buffer.Length - len);
					return len;
				default:
					buffer [index++] = c;
					len++;
					break;
				}
			}
			return len;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			try {
				if (!Disposed)
					sr.Dispose ();
			}
			finally {
				Disposed = true;
			}
		}

		protected bool Disposed { get; private set; }
	}
}

