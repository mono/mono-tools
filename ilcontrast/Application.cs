// Application.cs : Main application class
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, 
// publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.


namespace IlContrast {

	using System;
	using System.IO;
	using Gtk;
	using ICSharpCode.SharpZipLib.Tar;

	public class Application {

		public static int Main (string[] args)
		{
			string base_path = null;
			string target_path = null;

			foreach (string arg in args) {
				if (arg.StartsWith ("--base="))
					base_path = arg.Substring (7);
				else if (arg.StartsWith ("--target="))
					target_path = arg.Substring (9);
			}

			ComparisonInfo info = null;
			if (args.Length > 0) {
				if (base_path == null || target_path == null) {
					Console.Error.WriteLine ("Usage: ilcontrast [--base=<path> --target=<path>]");
					return 1;
				}
				info = new ComparisonInfo (base_path, target_path);
			}
				
			Gtk.Application.Init();
			
			if (!Directory.Exists (DataPath)) {
				Directory.CreateDirectory (DataPath);
				TarArchive tar = TarArchive.CreateInputTarArchive (System.Reflection.Assembly.GetCallingAssembly ().GetManifestResourceStream ("deploy.tar"));
				tar.ExtractContents (DataPath);
			}

			IlContrast.Window win = new IlContrast.Window (info);
			win.Show ();
			Gtk.Application.Run ();
			return 0;
		}

		public static string DataPath {
			get {
				string data_path = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				return Path.Combine (data_path, "ilcontrast");
			}
		}

		public static string DeployPath {
			get {
				return Path.Combine (DataPath, "deploy");
			}
		}
	}
}

