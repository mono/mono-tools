//
// Gendarme.Rules.Interoperability.UseManagedAlternativesToPInvokeRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Linq;

using Gendarme.Framework;
using Mono.Cecil;

namespace Gendarme.Rules.Interoperability {

	// TODO: QueryPerformanceFrequency can be replaced with StopWatch

	/// <summary>
	/// This rule will fire if an external (P/Invoke) method is called but a managed
	/// alternative is provided by the .NET framework.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [DllImport ("kernel32.dll")]
	/// static extern void Sleep (uint dwMilliseconds);
	/// 
	/// public void WaitTwoSeconds ()
	/// {
	///	Sleep (2000);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void WaitTwoSeconds ()
	/// {
	///	System.Threading.Thread.Sleep (2000);
	/// }
	/// </code>
	/// </example>

	[Problem ("There is a managed API which can be used instead of the p/invoke call.")]
	[Solution ("Use the suggested managed alternative to replace your p/invoke call and remove its declaration.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")]
	public class UseManagedAlternativesToPInvokeRule : Rule, IMethodRule {

		private sealed class ManagedAlternatives {

			public ManagedAlternatives (string module, string alternatives)
				: this (module, alternatives, TargetRuntime.Net_1_1)
			{
			}

			public ManagedAlternatives (string module, string alternatives, TargetRuntime runtime)
			{
				Module = module;
				Alternatives = alternatives;
				Runtime = runtime;
			}

			public string Alternatives { get; set; }

			public string Module { get; set; }

			public TargetRuntime Runtime { get; set; }
		}

		private static Dictionary<string, ManagedAlternatives> managedAlternatives =
			new Dictionary<string, ManagedAlternatives> () {
				{ "Sleep", new ManagedAlternatives ("kernel32.dll", "System.Threading.Thread::Sleep()") },
				{ "FindFirstFile", new ManagedAlternatives ("kernel32.dll", "System.IO.Directory::GetDirectories(), System.IO.Directory::GetFiles(), System.IO.Directory::GetFileSystemEntries()") },
				{ "ReadFile", new ManagedAlternatives ("kernel32.dll", "System.IO.FileStream") },
				{ "WaitForMultipleObjects", new ManagedAlternatives ("kernel32.dll", "System.Threading.WaitHandle::WaitAny(), System.Threading.WaitHandle::WaitAll()") },
				{ "GetLastError", new ManagedAlternatives ("kernel32.dll", "System.Runtime.InteropServices.Marshal::GetLastWin32Error()") },
				{ "MessageBox", new ManagedAlternatives ("user32.dll", "System.Windows.Forms.MessageBox::Show()") },
				{ "Beep", new ManagedAlternatives ("kernel32.dll", "System.Console::Beep()", TargetRuntime.Net_2_0) },
				{ "PlaySound", new ManagedAlternatives ("winmm.dll", "System.Media.SoundPlayer", TargetRuntime.Net_2_0) }
			};

		private static ManagedAlternatives GetManagedAlternatives (MethodDefinition method)
		{
			ManagedAlternatives alts;
			if (managedAlternatives.TryGetValue (method.Name, out alts)) {
				// can we apply this alternative to the framework being used ?
				if (method.DeclaringType.Module.Runtime < alts.Runtime)
					return null;
				// make sure we're talking about the exact same (module-wise) p/invoke declaration
				if (alts.Module.StartsWith (method.PInvokeInfo.Module.Name, StringComparison.OrdinalIgnoreCase))
					return alts;
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to non-pinvoke methods
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			// rule apply, looks for alternatives

			ManagedAlternatives alternatives = GetManagedAlternatives (method);
			if (alternatives == null)
				return RuleResult.Success;

			string message = String.Format (CultureInfo.InvariantCulture, 
				"Try to replace the platform-dependent call '{0}' by (one of) the following alternative(s): {1}.",
				method.Name, alternatives.Alternatives);
			Runner.Report (method, Severity.Low, Confidence.High, message);
			return RuleResult.Failure;
		}
	}
}
