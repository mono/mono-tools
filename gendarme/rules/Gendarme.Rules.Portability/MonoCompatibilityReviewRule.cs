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
	/// This rule uses MoMA definition files to analyze assemblies and warns if called methods are:
	/// <list type="bullet">
	/// <item>
	/// <description>marked with a <c>[MonoTODO]</c> attribute;</description>
	/// </item>
	/// <item>
	/// <description>throw a <c>NotImplementedException</c>; or</description>
	/// </item>
	/// <item>
	/// <description>do not exist in the current version of Mono::</description>
	/// </item>
	/// </list>
	/// The rule looks for the definitions in <c>~/.local/share/Gendarme/definitions.zip</c>. 
	/// If the file is missing it will try to download the latest version.
	/// </summary>
	/// <remarks>This rule does not replace MoMA (which is obviously the easiest solution for Windows developers). It's goal is to help analyze multiple portability issues from the Linux side (where Gendarme is most likely being used)</remarks>

	[Problem ("The method has some known limitations when used with the Mono:: runtime.")]
	[Solution ("Check if this code is critical to your application. Also make sure your definition files are up to date.")]
	[EngineDependency (typeof (OpCodeEngine))]
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
			string value;

			if (!dict.TryGetValue (callee, out value))
				return;

			string message = String.Format (error, callee, value);
			// confidence is Normal since we can't be sure if MoMA data is up to date
			Runner.Report (method, ins, severity, Confidence.Normal, message);
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
			}

			return Runner.CurrentRuleResult;
		}
	}
}
