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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using ICSharpCode.SharpZipLib.Zip;

using MoMA.Analyzer.MoMAWebService;

namespace Gendarme.Rules.Portability {

	/// <summary>
	/// This rule will fire if one of the assemblies being checked contains a call to a .NET
	/// method which is either not implemented on Mono or partially implemented. It does
	/// this by downloading a MoMA definitions.zip file into <c>~/.local/share/Gendarme/definitions.zip</c>
	/// (on Unix) and checking for calls to the methods therein. The rule will work without 
	/// MoMA but if it does fire it may be useful to download and run MoMA.
	/// </summary>
	/// <remarks>
	/// The definitions.zip will not be downloaded if it already exists so it's a good idea to
	/// manually remove it now and then.
	/// </remarks>

	[Problem ("The method is either missing or partially implemented on Mono.")]
	[Solution ("Review and test the code to ensure that it works properly on Mono. Also delete the definitions.zip to ensure that the latest version is downloaded.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class MonoCompatibilityReviewRule : Rule, IMethodRule {

		private const string NotImplementedMessage = "{0} is not implemented.";
		private const string MissingMessage = "{0} is missing from Mono.";
		private const string TodoMessage = "{0} is marked with a [MonoTODO] attribute: {1}.";

		private HashSet<string> NotImplementedInternal; //value is unused
		private HashSet<string> MissingInternal; //value is unused
		private Dictionary<string, string> TodoInternal; //value = TODO Description

		public HashSet<string> NotImplemented {
			get { return NotImplementedInternal; }
		}

		public HashSet<string> Missing {
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
							NotImplementedInternal = Read (new StreamReader (zs));
							break;
						case "missing.txt":
							MissingInternal = Read (new StreamReader (zs));
							break;
						case "monotodo.txt":
							TodoInternal = ReadWithComments (new StreamReader (zs));
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

		private static Dictionary<string, string> ReadWithComments (TextReader reader)
		{
			Dictionary<string, string> dict = new Dictionary<string, string> ();
			string line;
			while ((line = reader.ReadLine ()) != null) {
				int split = line.IndexOf ('-');
				string target = line.Substring (0, split);
				// are there comments ? (many entries don't have any)
				if (split == line.Length - 1) {
					dict.Add (target, null);
				} else {
					dict.Add (target, line.Substring (split + 1));
				}
			}
			return dict;
		}

		private static HashSet<string> Read (TextReader reader)
		{
			HashSet<string> set = new HashSet<string> ();
			string line;
			while ((line = reader.ReadLine ()) != null) {
				set.Add (line);
			}
			return set;
		}

		// this correspond to Call, Calli, Callvirt, Newobj, Initobj, Ldftn, Ldvirtftn
		private static OpCodeBitmask mask = new OpCodeBitmask (0x18000000000, 0x4400000000000, 0x0, 0x40060);
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// check if any instruction refers to methods or types that MoMA could track
			if (!mask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// rule applies

			foreach (Instruction ins in method.Body.Instructions) {
				// look for any instruction that could use something incomplete
				if (!mask.Get (ins.OpCode.Code))
					continue;

				// this (MethodReference.ToString) is costly so we do it once for the three checks
				string callee = ins.Operand.ToString ();

				// calling not implemented method is very likely not to work == High
				if ((NotImplemented != null) && NotImplementedInternal.Contains (callee)) {
					string message = String.Format (NotImplementedMessage, callee);
					// confidence is Normal since we can't be sure if MoMA data is up to date
					Runner.Report (method, ins, Severity.High, Confidence.Normal, message);
				}

				// calling missing methods can't work == Critical
				if ((Missing != null) && Missing.Contains (callee)) {
					string message = String.Format (MissingMessage, callee);
					Runner.Report (method, ins, Severity.Critical, Confidence.Normal, message);
				}

				// calling todo methods migh work with some limitations == Medium
				if (ToDo != null) {
					string value;
					if (ToDo.TryGetValue (callee, out value)) {
						string message = String.Format (TodoMessage, callee, value);
						Runner.Report (method, ins,  Severity.Medium, Confidence.Normal, message);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

