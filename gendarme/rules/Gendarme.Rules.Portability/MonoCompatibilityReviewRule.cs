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
using System.Collections.Generic;
using System.IO;
using System.Net;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

using ICSharpCode.SharpZipLib.Zip;

using MoMA.Analyzer.MoMAWebService;

namespace Gendarme.Rules.Portability {

	[Problem ("The method has some known limitations when used with the Mono:: runtime.")]
	[Solution ("Check if this code is critical to your application. Also make sure your definition files are up to date.")]
	public class MonoCompatibilityReviewRule : Rule, IMethodRule {

		private const string NotImplementedMessage = "{0} is not implemented.{1}";
		private const string MissingMessage = "{0} is missing from Mono.{1}";
		private const string TodoMessage = "{0} is marked with a [MonoTODO] attribute: {1}.";

		private Dictionary<string, string> NotImplementedInternal; //value is unused
		private Dictionary<string, string> MissingInternal; //value is unused
		private Dictionary<string, string> TodoInternal; //value = TODO Description

		public Dictionary<string, string> NotImplemented {
			get { return NotImplementedInternal; }
		}

		public Dictionary<string, string> Missing {
			get { return MissingInternal; }
		}

		public Dictionary<string, string> ToDo {
			get { return TodoInternal; }
		}
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			string localAppDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			string definitionsFolder = Path.Combine (localAppDataFolder, "Gendarme");
			string definitionsFile = Path.Combine (definitionsFolder, "definitions.zip");

			if (!File.Exists (definitionsFile)) {
				if (!Directory.Exists (definitionsFolder))
					Directory.CreateDirectory (definitionsFolder);

				if (!Download (definitionsFile)) {
					Active = false;
					return;
				}
			}

			using (FileStream fs = File.OpenRead (definitionsFile)) {
				using (ZipInputStream zs = new ZipInputStream (fs)) {
					ZipEntry ze;
					while ((ze = zs.GetNextEntry ()) != null) {
						switch (ze.Name) {
						case "exception.txt":
							NotImplementedInternal = Read (new StreamReader (zs), false);
							break;
						case "missing.txt":
								MissingInternal = Read (new StreamReader (zs), false);
							break;
						case "monotodo.txt":
							TodoInternal = Read (new StreamReader (zs), true);
							break;
						default:
							break;
						}
					}
				}
			}

			// rule is active only if we have, at least one of, the MoMA files
			Active = ((NotImplemented != null) || (Missing != null) || (ToDo != null));
		}

		private bool Download (string definitionsFile)
		{
			// try to download files from the net
			try {
				string definitionsUri;
				using (MoMASubmit ws = new MoMASubmit ()) {
					definitionsUri = ws.GetLatestDefinitionsVersion ().Split ('|') [2];
				}

				using (WebClient wc = new WebClient ()) {
					wc.DownloadFile (new Uri (definitionsUri), definitionsFile);
				}
			}
			catch (WebException e) {
				if (Runner.VerbosityLevel > 0)
					Console.Error.WriteLine (e);
				return false;
			}
			return true;
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

		private void Check (Dictionary<string, string> dict, MethodDefinition method, Instruction ins, string error, Severity severity)
		{
			string callee = ins.Operand.ToString ();
			if (!dict.ContainsKey (callee))
				return;

			string message = String.Format (error, callee, dict [callee]);
			// confidence is Normal since we can't be sure if MoMA data is up to date
			Runner.Report (method, ins, severity, Confidence.Normal, message);
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// rule applies

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
				case Code.Newobj:
				case Code.Initobj:
				case Code.Ldftn:
				case Code.Ldvirtftn:
					// calling not implemented method is very likely not to work == High
					if (NotImplemented != null) {
						Check (NotImplemented, method, ins, NotImplementedMessage, Severity.High);
					}

					// calling missing methods can't work == Critical
					if (Missing != null) {
						Check (Missing, method, ins, MissingMessage, Severity.Critical);
					}

					// calling todo methods migh work with some limitations == Medium
					if (ToDo != null) {
						Check (ToDo, method, ins, TodoMessage, Severity.Medium);
					}
					break;
				default:
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
