// 
// Gendarme.Framework.Engine
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Framework {

	// FIXME: do a IEngine interface once this is stable
	abstract public class Engine {

		protected Engine ()
		{
		}

		protected EngineController Controller {
			get;
			private set;
		}

		/// <summary>
		/// Override to attach to some of the controller events
		/// </summary>
		/// <param name="controller"></param>
		public virtual void Initialize (EngineController controller)
		{
			Controller = controller;
		}

		/// <summary>
		/// Note: TearDown can be called without a call to Engine.Register or 
		/// Engine.Initialize
		/// </summary>
		public virtual void TearDown ()
		{
			Controller = null;
		}
	}
}
