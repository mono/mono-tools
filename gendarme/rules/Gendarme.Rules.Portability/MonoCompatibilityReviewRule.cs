//
// Gendarme.Rules.Portability.MonoCompatibilityReviewRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2007 Andreas Noever
// Copyright (C) 2009, 2011 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using ICSharpCode.SharpZipLib.Zip;

using MoMA.Analyzer.MoMAWebService;

namespace Gendarme.Rules.Portability {

	/// <summary>
	/// This rule will fire if one of the assemblies being checked contains a call to a .NET
	/// method which is either not implemented on Mono or partially implemented. It does
	/// this by downloading a MoMA definitions file under <c>~/.local/share/Gendarme/</c> (on UNIX)
	/// or <c>C:\Documents and Settings\{username}\Local Settings\Application Data\Gendarme</c> 
	/// (on Windows) and checking for calls to the methods therein. The rule will work without 
	/// MoMA but if it does fire it may be useful to download and run MoMA.
	///
	/// By default the rule will use the latest local version available. This can be overriden to use a 
	/// specific, local, version if you want to review compatibility against a specific Mono version.
	/// You can also manually remove them, now and then, to ensure you are using the latest version.
	/// Also upgrading Gendarme will try to download a newer version of the definitions files.
	/// </summary>

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
		
		private Version version;
		private string definitions_folder;

		public HashSet<string> NotImplemented {
			get { return NotImplementedInternal; }
		}

		public HashSet<string> Missing {
			get { return MissingInternal; }
		}

		public Dictionary<string, string> ToDo {
			get { return TodoInternal; }
		}

		private string DefinitionsFolder {
			get {
				if (definitions_folder == null) {
					string localAppDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
					definitions_folder = Path.Combine (localAppDataFolder, "Gendarme");
					if (!Directory.Exists (definitions_folder))
						Directory.CreateDirectory (definitions_folder);
				}
				return definitions_folder;
			}
		}

		/// <summary>
		/// The version of Mono against which you wish to review compatibility.
		/// You need to have this version of the definitions file downloaded in order to use it.
		/// This is useful if you want to upgrade Gendarme but still want to test compatibility
		/// against an older version of Mono.
		/// </summary>
		[Description ("The version of Mono against which you wish to review compatibility.")]
		public string Version {
			get {
				if (version == null)
					return String.Empty;
				return version.ToString ();
			}
			set {
				version = new Version (value);
				string file = GetFileName (version);
				if (!File.Exists (file)) {
					version = null;
					throw new FileNotFoundException ("Cannot find definitions for the requested version.", file);
				}
			}
		}

		private string GetFileName (Version v)
		{
			return Path.Combine (DefinitionsFolder, String.Format (CultureInfo.InvariantCulture, "definitions-{0}.zip", v));
		}

		private Version FindLastestLocalVersion ()
		{
			string [] def_files = Directory.GetFiles (DefinitionsFolder, "definitions-*.zip");
			if (def_files.Length > 1)
				Array.Sort<string> (def_files);
			if (def_files.Length == 0)
				return new Version ();

			try {
				string latest = def_files [def_files.Length - 1];
				int s = latest.LastIndexOf ("definitions-", StringComparison.Ordinal) + 12;
				return new Version (latest.Substring (s, latest.Length - s - 4)); // remove .zip
			}
			catch (FormatException) {
				return new Version ();
			}
		}

		private string SelectDefinitionsFile ()
		{
			Version def_version = version;

			// nothing specified ? 
			if (def_version == null) {
				// then we'll use the latest local version available
				def_version = FindLastestLocalVersion ();
				// if Gendarme version is newer than the definitions then there's likely something new available
				if (typeof (IRule).Assembly.GetName ().Version > def_version) {
					// however we don't want to download a (potentially) unexisting file each time we execute 
					// Gendarme (e.g. a development release, like 2.5.x.x) so we limit this to once per week
					FileInfo fi = new FileInfo (GetFileName (def_version));
					if (!fi.Exists || (fi.CreationTimeUtc < DateTime.UtcNow.AddDays (-7)))
						def_version = DownloadLatestDefinitions ();
				}
			}

			return GetFileName (def_version);
		}

		private void LoadDefinitions (string filename)
		{
			if (!File.Exists (filename))
				return;

			using (FileStream fs = File.OpenRead (filename)) 
			using (ZipInputStream zs = new ZipInputStream (fs))
			using (StreamLineReader sr = new StreamLineReader (zs)) {
				ZipEntry ze;
				while ((ze = zs.GetNextEntry ()) != null) {
					switch (ze.Name) {
					case "exception.txt":
						NotImplementedInternal = Read (sr);
						break;
					case "missing.txt":
						MissingInternal = Read (sr);
						break;
					case "monotodo.txt":
						TodoInternal = ReadWithComments (sr);
						break;
					default:
						break;
					}
				}
			}
		}

		static byte [] ecma_key = new byte [] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89  };
		static byte [] msfinal_key = new byte [] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a };
		static byte [] winfx_key = new byte [] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0xe4, 0x35 };
		static byte [] silverlight_key = new byte [] { 0x7c, 0xec, 0x85, 0xd7, 0xbe, 0xa7, 0x79, 0x8e };
		static Version coreclr_runtime = new Version (2, 0, 5, 0);

		static bool ComparePublicKeyToken (byte [] pkt1, byte [] pkt2)
		{
			for (int i = 0; i < 8; i++) {
				if (pkt1 [i] != pkt2 [i])
					return false;
			}
			return true;
		}

		static bool Filter (AssemblyNameReference anr)
		{
			// if the scope is the current assembly, then AssemblyNameReference will be null
			if (anr == null)
				return false;

			// MoMA tracks only assemblies that have "well known" public key tokens
			byte [] pkt = anr.PublicKeyToken;
			if ((pkt == null) || (pkt.Length != 8))
				return false;

			switch (pkt [0]) {
			case 0xb7:
				// candidate for b77a5c561934e089 which is the ECMA key
				return ComparePublicKeyToken (ecma_key, pkt);
			case 0xb0:
				// candidate for b03f5f7f11d50a3a which is the 'msfinal' key
				return ComparePublicKeyToken (msfinal_key, pkt);
			case 0x31:
				// candidate for 31bf3856ad364e35 which is 'winfx' key - 
				// but some Silverlight assemblies, Microsoft.VisualBasic.dll and 
				// System.ServiceModel.dll, use it too
				return (ComparePublicKeyToken (winfx_key, pkt) && (anr.Version != coreclr_runtime));
			case 0x7c:
				// candidate for 7cec85d7bea7798e which is used by Silverlight
				// MoMA does not track Silverlight compatibility
				return !ComparePublicKeyToken (silverlight_key, pkt);
			default:
				return false;
			}
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// get the specified or latest definition file available locally *or*
			// download it if none is present or if gendarme is more recent than the file
			LoadDefinitions (SelectDefinitionsFile ());

			// rule is active only if we have, at least one of, the MoMA files
			Active = ((NotImplemented != null) || (Missing != null) || (ToDo != null));

			// MoMA does not support all frameworks, e.g. Silverlight
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				foreach (AssemblyNameReference anr in e.CurrentModule.AssemblyReferences) {
					if (Filter (anr)) {
						Active = true;
						return;
					}
				}
				Active = false;
			};
		}

		private Version DownloadLatestDefinitions ()
		{
			Version v = null;
			// try to download files from the net
			try {
				string definitionsUri;
				using (MoMASubmit ws = new MoMASubmit ()) {
					string lastest_def = ws.GetLatestDefinitionsVersion ();
					int s = lastest_def.LastIndexOf ('/') + 1;
					int e = lastest_def.IndexOf ('-', s);
					v = new Version (lastest_def.Substring (s, e - s));
					definitionsUri = lastest_def.Split ('|') [2];
				}

				using (WebClient wc = new WebClient ()) {
					string filename = GetFileName (v);
					wc.DownloadFile (new Uri (definitionsUri), filename);
				}
			}
			catch (WebException e) {
				if (Runner.VerbosityLevel > 0)
					Console.Error.WriteLine (e);
			}
			return v;
		}

		static char [] buffer = new char [4096];

		private static Dictionary<string, string> ReadWithComments (StreamLineReader reader)
		{
			Dictionary<string, string> dict = new Dictionary<string, string> ();
			while (!reader.EndOfStream) {
				int length = reader.ReadLine (buffer, 0, buffer.Length);
				int pos = Array.IndexOf (buffer, '-');
				string key = new string (buffer, 0, pos);
				string comment = (buffer [length - 1] == '-') ? null :
					new string (buffer, pos + 1, length - pos - 1);
				dict.Add (key, comment);
			}
			return dict;
		}

		private static HashSet<string> Read (StreamLineReader reader)
		{
			HashSet<string> set = new HashSet<string> ();
			while (!reader.EndOfStream) {
				int length = reader.ReadLine (buffer, 0, buffer.Length);
				set.Add (new string (buffer, 0, length));
			}
			return set;
		}

		// this correspond to Call, Callvirt, Newobj, Initobj, Ldftn, Ldvirtftn
		private static OpCodeBitmask mask = new OpCodeBitmask (0x8000000000, 0x4400000000000, 0x0, 0x40060);
		
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

				// filter calls to assemblies that MoMA likely does not include (e.g. your own code)
				MethodReference mr = (ins.Operand as MethodReference);
				if ((mr == null) || !Filter (mr.DeclaringType.Scope as AssemblyNameReference))
					continue;

				// MethodReference.ToString is costly so we do it once for the three checks
				string callee = mr.GetFullName ();

				// calling not implemented method is very likely not to work == High
				if ((NotImplemented != null) && NotImplementedInternal.Contains (callee)) {
					string message = String.Format (CultureInfo.InvariantCulture, NotImplementedMessage, callee);
					// confidence is Normal since we can't be sure if MoMA data is up to date
					Runner.Report (method, ins, Severity.High, Confidence.Normal, message);
				}

				// calling missing methods can't work == Critical
				if ((Missing != null) && Missing.Contains (callee)) {
					string message = String.Format (CultureInfo.InvariantCulture, MissingMessage, callee);
					Runner.Report (method, ins, Severity.Critical, Confidence.Normal, message);
				}

				// calling todo methods migh work with some limitations == Medium
				if (ToDo != null) {
					string value;
					if (ToDo.TryGetValue (callee, out value)) {
						string message = String.Format (CultureInfo.InvariantCulture, TodoMessage, callee, value);
						Runner.Report (method, ins,  Severity.Medium, Confidence.Normal, message);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

