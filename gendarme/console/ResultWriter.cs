//
// ResultWriter base class
//
// Authors:
//	Christian Birkl <christian.birkl@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006 Christian Birkl
// Copyright (C) 2006, 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;

namespace Gendarme {

	abstract public class ResultWriter : IDisposable {

		private bool disposed;

		protected ResultWriter (IRunner runner, string fileName)
		{
			Runner = runner;
			FileName = fileName;
		}

		[ThreadModel (ThreadModel.SingleThread)]
		~ResultWriter ()
		{
			if (!disposed)
				Dispose (false);
		}

		protected IRunner Runner {
			get;
			private set;
		}

		protected string FileName {
			get;
			set;
		}

		protected virtual void Start ()
		{
		}

		protected virtual void Write ()
		{
		}

		protected virtual void Finish ()
		{
		}

		public void Report ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().Name);
			
			Start ();
			Write ();
			Finish ();
		}

		public void Dispose ()
		{
			if (!disposed) {
				Dispose (true);
				GC.SuppressFinalize (this);
				
				disposed = true;
			}
		}

		[ThreadModel (ThreadModel.SingleThread)]
		protected abstract void Dispose (bool disposing);
	}
}
