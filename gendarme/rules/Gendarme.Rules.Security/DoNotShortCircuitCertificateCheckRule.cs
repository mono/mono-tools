//
// Gendarme.Rules.Security.DoNotShortCircuitCertificateCheckRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Security {

	/// <summary>
	/// This rule checks for methods that implements pass-through certificate checks.
	/// I.e. methods that override the framework decision about a certificate validity
	/// without checking anything specific about the supplied certificate or error code.
	/// Protocols like TLS/SSL are only secure if the certificates are used correctly.
	/// </summary>
	/// <example>
	/// Bad example (ICertificatePolicy):
	/// <code>
	/// public class AcceptEverythingCertificatePolicy : ICertificatePolicy {
	/// 	public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
	/// 	{
	/// 		// this accepts everything making it easy for MITM 
	/// 		// (Man-in-the-middle) attacks
	/// 		return true;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (ICertificatePolicy):
	/// <code>
	/// public class AllowSpecificCertificatePolicy : ICertificatePolicy {
	/// 	public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
	/// 	{
	/// 		// this accept only a specific certificate, even if others would be ok
	/// 		return (certificate.GetCertHashString () == "D62F48D013EE7FB58B79074512670D9C5B3A5DA9");
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (RemoteCertificateValidationCallback):
	/// <code>
	/// public bool CertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	/// {
	/// 	// this accepts everything making it easy for MITM 
	/// 	// (Man-in-the-middle) attacks
	///	return true;
	/// }
	/// 
	/// SslStream ssl = new SslStream (stream, false, new RemoteCertificateValidationCallback (CertificateValidationCallback), null);
	/// </code>
	/// </example>
	/// <example>
	/// Good example (RemoteCertificateValidationCallback):
	/// <code>
	/// public bool CertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	/// {
	/// 	// this accept only a specific certificate, even if others would be ok
	/// 	return (certificate.GetCertHashString () == "D62F48D013EE7FB58B79074512670D9C5B3A5DA9");
	/// }
	/// 
	/// SslStream ssl = new SslStream (stream, false, new RemoteCertificateValidationCallback (CertificateValidationCallback), null);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("The CheckValidationResult method inside this type looks incomplete or is a 'pass-through'.")]
	[Solution ("Review the certificate policy as it is likely not secure enough to be used in a public network.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DoNotShortCircuitCertificateCheckRule : Rule, IMethodRule {

		static readonly string [] CertificatePolicyParameters = {
			"System.Net.ServicePoint",
			"System.Security.Cryptography.X509Certificates.X509Certificate",
			"System.Net.WebRequest",
			"System.Int32"
		};

		static readonly string [] RemoteCertificateValidationParameters = {
			"System.Object",
			"System.Security.Cryptography.X509Certificates.X509Certificate",
			"System.Security.Cryptography.X509Certificates.X509Chain",
			"System.Net.Security.SslPolicyErrors"
		};

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference System.Math then 
			// none of its method is being called with constants
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Net", "ICertificatePolicy");
					})
				);
			};
		}

		private RuleResult CheckArguments (MethodDefinition method, bool third)
		{
			// if the method loads argument then it could be doing "th right thing"
			// otherwise we already known the code is suspect
			if (OpCodeEngine.GetBitmask (method).Intersect (OpCodeBitmask.LoadArgument)) {
				foreach (Instruction ins in method.Body.Instructions) {
					if (ins.IsLoadArgument ()) {
						ParameterDefinition pd = ins.GetParameter (method);
						if (pd == null)
							continue;

						switch (pd.Index) {
						case 1:
						case 3:
							return RuleResult.Success;
						case 2:
							if (third)
								return RuleResult.Success;
							break;
						}
					}
				}
			}

			Runner.Report (method, Severity.High, Confidence.Normal);
			return RuleResult.Failure;
		}

		private RuleResult CheckPolicy (MethodDefinition method)
		{
			// since ICertificatePolicy is an interface we need to check its name
			string name = method.Name;
			if (name == "CheckValidationResult") {
				if (!method.DeclaringType.Implements ("System.Net", "ICertificatePolicy"))
					return RuleResult.Success;
			} else if (name != "System.Net.ICertificatePolicy.CheckValidationResult")
				return RuleResult.Success;

			// the policy is suspect if it does not touch 
			// * the certificate parameter (2nd); and
			// * the certificateProblem parameter (4th)
			return CheckArguments (method, false);
		}

		private RuleResult CheckCallback (MethodDefinition method)
		{
			// the policy is suspect if it does not touch 
			// * the certificate parameter (2nd); and
			// * the chain parameter (3rd); and
			// * the certificateProblem parameter (4th)
			return CheckArguments (method, true);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.IsAbstract || !method.HasParameters)
				return RuleResult.DoesNotApply;

			IList<ParameterDefinition> pdc = method.Parameters;
			int count = pdc.Count;
			if ((count != 4) || !method.ReturnType.IsNamed ("System", "Boolean"))
				return RuleResult.DoesNotApply;

			// this method could be a candidate for both policy or callback
			bool policy = true;
			bool callback = true;
			// if all the parameters match
			for (int i = 0; i < count; i++) {
				TypeReference ptype = pdc [i].ParameterType;
				if (policy && !ptype.IsNamed (CertificatePolicyParameters [i]))
					policy = false;
				if (callback && !ptype.IsNamed (RemoteCertificateValidationParameters [i]))
					callback = false;
			}

			if (policy)
				return CheckPolicy (method);
			else if (callback)
				return CheckCallback (method);
			return RuleResult.Success;
		}
	}
}
