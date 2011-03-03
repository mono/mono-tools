// 
// Gendarme.Rules.Portability.DoNotHardcodePathsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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

using System;
using System.Collections.Generic;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Portability {

	/// <summary>
	/// This rule checks for strings that contain valid paths, either under Unix or 
	/// Windows file systems. Path literals are often not portable across 
	/// operating systems (e.g. different path separators). To ensure correct cross-platform 
	/// functionality they should be replaced by calls to <c>Path.Combine</c> and/or 
	/// <c>Environment.GetFolderPath</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// void ReadConfig ()
	/// {
	///	using (FileStream fs = File.Open ("~/.local/share/myapp/user.config")) {
	///		// read configuration
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// void ReadConfig ()
	/// {
	///	string config_file = Environment.GetFolderPath (SpecialFolder.LocalApplicationData);
	///	config_file = Path.Combine (Path.Combine (config_file, "myapp"), "user.config");
	///	using (FileStream fs = File.Open (config_file)) {
	///		// read configuration
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This string looks like a path that may become invalid if the code is executed on a different operating system.")]
	[Solution ("Use System.IO.Path and System.Environment to generate paths instead of hardcoding them.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotHardcodePathsRule : Rule, IMethodRule {

		// result cache
		private static Dictionary<string, Confidence?> resultCache =
			new Dictionary<string, Confidence?> ();

		// (back)slash counters
		private int slashes;
		private int backslashes;

		// method body being checked
		private MethodBody method_body;

		// point counter
		private int current_score;

		void AddPoints (int pts)
		{
			current_score += pts;
			// Console.WriteLine ("// added {0} pts", pts);
		}

		Confidence? CheckIfStringIsHardcodedPath (Instruction ldstr, string str)
		{
			// try to filter out false positives:

			// count (back)slashes
			slashes = CountOccurences (str, '/');
			backslashes = CountOccurences (str, '\\');

			// if there's no (back)slashes, string's not going to cause
			// any portability problems
			if (slashes == 0 && backslashes == 0)
				return null;

			// file paths don't contain //
			if (str.Contains ("//"))
				return null;

			// don't check XML strings
			if (str.Contains ("</") || str.Contains ("/>"))
				return null;

			// files paths don't usually have more than one dot (in extension)
			if (CountOccurences (str, '.') > 2)
				return null;


			// handle different cases
			if (CanBeWindowsAbsolutePath (str)) {
				// whoooaaa! most probably we have a windows absolute path here
				AddPoints (5); // add points (5 because '*:\*' is less common)
				backslashes--; // we should't count a backslash in drive letter

				// process a windows path
				ProcessWindowsPath ();

			} else if (CanBeWindowsUNCPath (str)) {
				AddPoints (4); // add points
				backslashes -= 2;

				// go!
				ProcessWindowsPath ();

			} else if (CanBeUnixAbsolutePath (str)) {
				// same for unix
				AddPoints (2); // add points (2 because '/*' is more common)
				slashes--; // we shouldn't count a slash in the beginning

				// process a unix path
				ProcessUnixProbablyAbsolutePath (str);

			} else {
				// since that's not an absolute path, we need to
				// switch between unix/windows path handlers
				// depending on what character is more common
				// ('/' for unix, '\' for windows)

				if (backslashes > slashes)
					ProcessWindowsPath (); // like directory\something\..
				else if (backslashes < slashes)
					ProcessUnixPath (); // like directory/something/..

			}

			// process the extension
			try {
				ProcessExtension (System.IO.Path.GetExtension (str));
			}
			catch (ArgumentException) {
				// catch any invalid path character (more common on windows)
			}

			// try to guess how the string is used
			TryGuessUsage (ldstr);

			// Console.WriteLine ("// total score: {0}", current_score);

			if (current_score > 13)
				return Confidence.Total;

			else if (current_score > 9)
				return Confidence.High;

			else if (current_score > 7)
				return Confidence.Normal;

			else
				return null;
		}

		static bool CanBeWindowsAbsolutePath (string s)
		{
			// true for strings like ?:\*
			// e.g. 'C:\some\path' or 'D:\something.else"
			return s [1] == ':' && s [2] == '\\';
		}

		static bool CanBeWindowsUNCPath (string s)
		{
			// true for Windows UNC paths
			// e.g. \\Server\Directory\File
			return s [0] == '\\' && s [1] == '\\';
		}

		static bool CanBeUnixAbsolutePath (string s)
		{
			// true for strings like /*
			return s [0] == '/';
		}

		void ProcessWindowsPath ()
		{
			// Console.WriteLine ("// process win path");

			// normally, windows paths don't contain slashes 
			if (slashes == 0 && backslashes > 1)
				AddPoints (2);

			// the more backslashes there are
			// the more we are convinced it's a path:

			if (backslashes > 3)
				AddPoints (4);

			else if (backslashes > 1)
				AddPoints (3);

			else if (backslashes == 1)
				AddPoints (2);
		}

		void ProcessUnixProbablyAbsolutePath (string path)
		{
			// check for common prefixes
			if (path.StartsWith ("/bin/", StringComparison.Ordinal) ||
			    path.StartsWith ("/etc/", StringComparison.Ordinal) ||
			    path.StartsWith ("/sbin/", StringComparison.Ordinal) ||
			    path.StartsWith ("/dev/", StringComparison.Ordinal) ||
			    path.StartsWith ("/lib/", StringComparison.Ordinal) ||
			    path.StartsWith ("/usr/", StringComparison.Ordinal) ||
			    path.StartsWith ("/tmp/", StringComparison.Ordinal) ||
			    path.StartsWith ("/proc/", StringComparison.Ordinal) ||
			    path.StartsWith ("/sys/", StringComparison.Ordinal) ||
			    path.StartsWith ("/cdrom/", StringComparison.Ordinal) ||
			    path.StartsWith ("/home/", StringComparison.Ordinal) ||
			    path.StartsWith ("/media/", StringComparison.Ordinal) ||
			    path.StartsWith ("/mnt/", StringComparison.Ordinal) ||
			    path.StartsWith ("/opt/", StringComparison.Ordinal) ||
			    path.StartsWith ("/var/", StringComparison.Ordinal))

				AddPoints (4);

			ProcessUnixPath ();
		}

		void ProcessUnixPath ()
		{
			// Console.WriteLine ("// process ux path");

			// normally, unix paths don't contain backslashes (unlike windows)
			if (backslashes == 0)
				AddPoints (2);

			// the more slashes there are
			// the more we are convinced it's a path:

			if (slashes > 3)
				AddPoints (3);

			else if (slashes > 1)
				AddPoints (2);

			else if (slashes == 1)
				AddPoints (1);
		}

		void ProcessExtension (string ext)
		{
			// Console.WriteLine ("// process extension");

			int length = ext.Length;

			// now we look at the extension length
			// NB: extension name also includes a dot (.)

			if (length < 2 || length > 6)
				return;

			if (length == 4) // this is very common for extensions => really good sign :-)
				AddPoints (4);

			else
				AddPoints (3); // less common but still good
		}

		void TryGuessUsage (Instruction ldstr)
		{
			// Console.WriteLine ("// guess usage");

			// here we hope to get some additional points
			// from further usage analysis

			// no further usage, good-bye!
			if (ldstr.Next == null)
				return;

			// we handle two cases:

			// #1: string can be stored into a local variable or field with a well-sounding name*
			// * - well-sounding == name.Contains ("dir") || name.Contains ("file") || ...

			if (CheckIfStored (ldstr.Next))
				return;

			// if we reach this point, it means #1 didn't catch anything

			// now, try option #2: navigate to the closest call(i|virt)? or newobj
			// and see what we can learn

			Instruction current = ldstr.Next;
			// this counter will be used to calculate parameter position
			int paramOffset = 0;

			// take next instruction if any until it is call(i|virt)? or newobj
			while (current != null && !CheckIfMethodOrCtorIsCalled (current, paramOffset)) {
				current = current.Next;
				paramOffset++;
			}
		}

		// true == handled
		// false == unhandled
		bool CheckIfStored (Instruction afterLdstr)
		{
			switch (afterLdstr.OpCode.Code) {
			case Code.Stfld: // store into field
			case Code.Stsfld:
				CheckIdentifier ((afterLdstr.Operand as FieldReference).Name);
				break;
			default:
				if (afterLdstr.IsStoreLocal ())
					CheckIdentifier (afterLdstr.GetVariable (method_body.Method).Name);
				else
					return false;
				break;
			}

			return true; // handled
		}

		// true == handled
		// false == unhandled
		bool CheckIfMethodOrCtorIsCalled (Instruction ins, int currentOffset)
		{
			switch (ins.OpCode.Code) {
			case Code.Call:
			case Code.Calli:
			case Code.Callvirt:
				// this is a call
				MethodReference target = ins.Operand as MethodReference;

				// this happens sometimes so it's worth checking
				if (target == null)
					return true;

				// we can avoid some false positives by doing additional checks here

				TypeReference tr = target.DeclaringType;
				string nameSpace = tr.Namespace;
				string typeName = tr.Name;
				string methodName = target.Name;

				if (nameSpace == "Microsoft.Win32" && typeName.StartsWith ("Registry", StringComparison.Ordinal) // registry keys
				    || (nameSpace.StartsWith ("System.Xml", StringComparison.Ordinal) // xpath expressions
					&& methodName.StartsWith ("Select", StringComparison.Ordinal))) {
					AddPoints (-42);
					return true; // handled
				}

				// see what we can learn

				if (target.HasParameters && (target.Parameters.Count == 1) && 
					methodName.StartsWith ("set_", StringComparison.Ordinal)) {
					// to improve performance, don't Resolve () to call IsSpecialName
					// this is a setter (in 99% cases)
					CheckIdentifier (methodName);
				} else {
					// we can also check parameter name
					CheckMethodParameterName (target, currentOffset);
				}
				break;

			case Code.Newobj:
				// this is a constructor call
				MethodReference ctor = (MethodReference) ins.Operand;
				// avoid catching regular expressions
				if (ctor.DeclaringType.IsNamed ("System.Text.RegularExpressions", "Regex"))
					AddPoints (-42);

				break;

			default:
				return false;
			}

			return true; // handled
		}

		void CheckMethodParameterName (MethodReference methodReference, int parameterOffset)
		{
			MethodDefinition method = methodReference.Resolve ();
			if ((method == null) || !method.HasParameters)
				return;

			IList<ParameterDefinition> pdc = method.Parameters;
			int parameterIndex = pdc.Count - parameterOffset - 1;

			// to prevent some uncommon situations
			if (parameterIndex < 0)
				return;

			// parameterOffset is distance in instructions between ldstr and call(i|virt)?
			ParameterDefinition parameter = pdc [parameterIndex];

			// if its name is 'pathy', score some points!
			CheckIdentifier (parameter.Name);
		}

		void CheckIdentifier (string name)
		{
			if (IdentifierLooksLikePath (name))
				AddPoints (4);
		}

		static bool IdentifierLooksLikePath (string name)
		{
			// Console.WriteLine ("// analyzing identifier '{0}'", name);
			return (name.IndexOf ("file", StringComparison.OrdinalIgnoreCase) >= 0) ||
				(name.IndexOf ("dir",  StringComparison.OrdinalIgnoreCase) >= 0) ||
				(name.IndexOf ("path", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		static int CountOccurences (string str, char c)
		{
			// helper method to tell how many times a certain character occurs in the string
			int n = 0;
			for (int i = 0; i < str.Length; i++)
				if (str [i] == c)
					n++;
			return n;
		}

		Confidence? GetConfidence (Instruction ldstr, string candidate)
		{
			// check if string is already in cache
			Confidence? result;
			if (resultCache.TryGetValue (candidate, out result))
				return result;

			// process and cache result
			result = CheckIfStringIsHardcodedPath (ldstr, candidate);
			resultCache.Add (candidate, result);
			return result;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, we don't check it
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Ldstr instructions in this method
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Ldstr))
				return RuleResult.DoesNotApply;

			method_body = method.Body;

			// enumerate instructions to look for strings
			foreach (Instruction ins in method_body.Instructions) {
				// Console.WriteLine ("{0} {1}", ins.OpCode, ins.Operand);

				if (!ins.Is (Code.Ldstr))
					continue;

				slashes = backslashes = current_score = 0;

				// check if loaded string is a hardcoded path
				string candidate = (ins.Operand as string);

				// don't check too short strings (we do this very earlier to avoid caching the small values)
				if (candidate.Length < 4)
					continue;

				Confidence? conf = GetConfidence (ins, candidate);

				// if sure enough, report the problem with the candidate string
				// important as this allows a quick false positive check without checking the source code
				if (conf.HasValue) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"string \"{0}\" looks quite like a filename.", candidate);
					Runner.Report (method, ins, Severity.High, conf.Value, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
