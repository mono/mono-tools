//
// Gendarme.Rules.Portability.MonoCompatibilityReviewRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;

using MoMA.Analyzer.MoMAWebService;

namespace Gendarme.Rules.Portability {

	public class MonoCompatibilityReviewRule : IMethodRule {

		private System.Net.WebException DownloadExceptionInternal;
		private Dictionary<string, string> NotImplementedInternal; //value is unused
		private Dictionary<string, string> MissingInternal; //value is unused
		private Dictionary<string, string> TodoInternal; //value = TODO Description

		public System.Net.WebException DownloadException {
			get { return DownloadExceptionInternal; }
			set { DownloadExceptionInternal = value; }
		}

		public Dictionary<string, string> NotImplemented {
			get { return NotImplementedInternal; }
			set { NotImplementedInternal = value; }
		}

		public Dictionary<string, string> Missing {
			get { return MissingInternal; }
			set { MissingInternal = value; }
		}

		public Dictionary<string, string> Todo {
			get { return TodoInternal; }
			set { TodoInternal = value; }
		}
		
		public MonoCompatibilityReviewRule ()
		{
			string localAppDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			string definitionsFolder = Path.Combine (localAppDataFolder, "Gendarme");
			string definitionsFile = Path.Combine (definitionsFolder, "definitions.zip");

			if (!File.Exists (definitionsFile)) {
				if (!Directory.Exists (definitionsFolder))
					Directory.CreateDirectory (definitionsFolder);

				try {
					//try to download files from the net
					MoMASubmit ws = new MoMASubmit ();
					string definitionsUri = ws.GetLatestDefinitionsVersion ().Split ('|') [2];
					ws.Dispose ();

					System.Net.WebClient wc = new System.Net.WebClient ();
					wc.DownloadFile (new Uri (definitionsUri), definitionsFile);
				}
				catch (System.Net.WebException e) {
					DownloadException = e;
					return;
				}
			}

			ZipInputStream zs = new ZipInputStream (File.OpenRead (definitionsFile));
			ZipEntry ze;
			while ((ze = zs.GetNextEntry ()) != null) {
				switch (ze.Name) {
				case "exception.txt":
					NotImplemented = Read (new StreamReader (zs), false);
					break;
				case "missing.txt":
					Missing = Read (new StreamReader (zs), false);
					break;
				case "monotodo.txt":
					Todo = Read (new StreamReader (zs), true);
					break;
				default:
					break;
				}
			}
		}

		private static Dictionary<string, string> Read (StreamReader reader, bool split)
		{
			Dictionary<string, string> dict = new Dictionary<string, string> ();
			string line;
			while ((line = reader.ReadLine ()) != null) {
				if (split) {
					string [] parts = line.Split ('-');
					dict.Add (parts [0], parts [1]);
				} else {
					dict.Add (line, null);
				}
			}
			return dict;
		}

		private static void Check (Dictionary<string, string> dict, string calledMethod,
				   MethodDefinition method, Instruction ins,
				   ref MessageCollection results, string error, MessageType type)
		{
			if (dict == null)
				return;
			if (!dict.ContainsKey (calledMethod))
				return;

			if (results == null)
				results = new MessageCollection ();

			error = string.Format (error, calledMethod, dict [calledMethod]);
			Location loc = new Location (method, ins.Offset);

			Message msg = new Message (error, loc, type);
			results.Add (msg);
		}
		
		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return runner.RuleSuccess;

			MessageCollection results = null;

			if (DownloadException != null) {
				results = new MessageCollection ();
				results.Add (new Message (string.Format ("Unable to read or download the definitions file: {0}.", DownloadException.Message), null, MessageType.Warning));
				DownloadException = null;
			}

			if (NotImplemented == null && Missing == null && Todo == null)
				return results;

			foreach (Instruction ins in method.Body.Instructions) {

				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
				case Code.Newobj:
				case Code.Initobj:
				case Code.Ldftn:
				case Code.Ldvirtftn:

					string calledMethodString = ins.Operand.ToString ();

					Check (NotImplemented, calledMethodString, method, ins,
					       ref results, "{0} is not implemented.", MessageType.Warning);
					Check (Missing, calledMethodString, method, ins,
					       ref results, "{0} is missing.", MessageType.Error);
					Check (Todo, calledMethodString, method, ins,
					       ref results, "{0} is marked with the MonoTODOAttribute ({1}).", MessageType.Warning);

					break;
				default:
					break;
				}
			}

			return results;
		}
	}
}
