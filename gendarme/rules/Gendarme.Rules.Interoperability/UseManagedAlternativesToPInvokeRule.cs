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
using System.Linq;
using System.Collections.Generic;

using Gendarme.Framework;
using Mono.Cecil;

namespace Gendarme.Rules.Interoperability {

	[Problem ("There is a potential managed API for the p/invoke declaration.")]
	[Solution ("Use the suggested managed alternative to your p/invoke call and remove it's declaration.")]
	public class UseManagedAlternativesToPInvokeRule : Rule, IMethodRule {

		private struct PInvokeCall {

			private string module;
			private string method;

			public PInvokeCall (string module, string methodName)
			{
				this.module = FormatModuleName (module);
				method = methodName;
			}

			public string MethodName { 
				get { return method; }
			}

			private static string FormatModuleName (string module)
			{
				if (string.IsNullOrEmpty (module))
					return null;

				module = module.ToLower ();
				if (!module.EndsWith (".dll"))
					module += ".dll";
				return module;
			}

			public override bool Equals (object obj)
			{
				if (obj is PInvokeCall)
					return (this == (PInvokeCall) obj);

				return false;
			}

			public static bool operator == (PInvokeCall call1, PInvokeCall call2)
			{
				return ((call1.module == call2.module) && (call1.MethodName == call2.MethodName));
			}

			public static bool operator != (PInvokeCall call1, PInvokeCall call2)
			{
				return ((call1.module != call2.module) || (call1.MethodName != call2.MethodName));
			}

			public override int GetHashCode ()
			{
				return (module.GetHashCode () ^ MethodName.GetHashCode ());
			}
		}

		private sealed class ManagedAlternatives {

			private List<string> alternatives = new List<string> ();
			private TargetRuntime runtime;

			public ManagedAlternatives (params string [] alternatives)
			{
				runtime = TargetRuntime.NET_1_1;
				AddAlternatives (alternatives);
			}

			public ManagedAlternatives (TargetRuntime runtime, params string [] alternatives)
			{
				this.runtime = runtime;
				AddAlternatives (alternatives);
			}

			private void AddAlternatives (IEnumerable<string> alternatives)
			{
				foreach (string alternative in alternatives)
					this.alternatives.Add (alternative);
			}

			public TargetRuntime Runtime {
				get { return runtime; }
			}

			public override string ToString ()
			{
				return alternatives.Aggregate<string> ((a1, a2) => a1 + ", " + a2); // join 'em
			}
		}

		private static Dictionary<PInvokeCall, ManagedAlternatives> managedAlternatives =
			new Dictionary<PInvokeCall, ManagedAlternatives> () {
				{ new PInvokeCall ("kernel32.dll", "Sleep"), new ManagedAlternatives ("System.Threading.Thread::Sleep ()") },
				{ new PInvokeCall ("kernel32.dll", "FindFirstFile"), new ManagedAlternatives ("System.IO.Directory::GetDirectories ()", "System.IO.Directory::GetFiles ()", "System.IO.Directory::GetFileSystemEntries ()") },
				{ new PInvokeCall ("kernel32.dll", "ReadFile"), new ManagedAlternatives ("System.IO.FileStream") },
				{ new PInvokeCall ("kernel32.dll", "WaitForMultipleObjects"), new ManagedAlternatives ("System.Threading.WaitHandle::WaitAny ()", "System.Threading.WaitHandle::WaitAll ()") },
				{ new PInvokeCall ("kernel32.dll", "GetLastError"), new ManagedAlternatives ("System.Runtime.InteropServices.Marshal::GetLastWin32Error ()") },
				{ new PInvokeCall ("user32.dll", "MessageBox"), new ManagedAlternatives ("System.Windows.Forms.MessageBox::Show ()") },
				{ new PInvokeCall ("kernel32.dll", "Beep"), new ManagedAlternatives (TargetRuntime.NET_2_0, "System.Console::Beep ()") },
				{ new PInvokeCall ("winmm.dll", "PlaySound"), new ManagedAlternatives (TargetRuntime.NET_2_0, "System.Media.SoundPlayer") }
			};

		private static ManagedAlternatives GetManagedAlternatives (MethodDefinition method, TargetRuntime runtime)
		{
			string moduleName = method.PInvokeInfo.Module.Name;
			PInvokeCall callInfo = new PInvokeCall (moduleName, method.Name);
			if (managedAlternatives.ContainsKey (callInfo)) {
				ManagedAlternatives alts = managedAlternatives [callInfo];
				if (runtime >= alts.Runtime)
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

			ManagedAlternatives alternatives = GetManagedAlternatives (method, method.DeclaringType.Module.Assembly.Runtime);
			if (alternatives == null)
				return RuleResult.Success;

			string message = string.Format ("Do not perform platform-dependent call ({0}) if it can be avoided. Use (one of) the following alternative(s) provided by .NET Framework: {1}.", method.Name, alternatives);
			Runner.Report (method, Severity.Low, Confidence.High, message);
			return RuleResult.Failure;
		}
	}
}
