//
// EventSlots.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using Monodoc;

namespace Monodoc.Utils
{
	class IndexMaker
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1 || args.Length != 2) {
				Usage ();
				return;
			}

			string action = args[0];
			var root = args.Length == 1 ? RootTree.LoadTree () : RootTree.LoadTree (args[1]);

			switch (action) {
			case "tree":
				RootTree.MakeIndex (root);
				break;
			case "search":
				RootTree.MakeSearchIndex (root);
				break;
			default:
				Console.WriteLine ("Sorry, {0} isn't a supported action.", action);
				break;
			}
		}

		static void Usage ()
		{
			Console.WriteLine (@"Usage: mono make-index.exe action [documentation-root]

	action: can be either 'tree' to generate a quick index of a documentation tree or 'search' to generate a more complete Lucene index
	documentation-root: path to the documentation root, by default it's $libdir/monodoc");
		}
	}
}