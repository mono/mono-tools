// 
// Unit tests for DoNotShortCircuitCertificateCheckRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Gendarme.Rules.Security;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Security {

	[TestFixture]
	public class DoNotShortCircuitCertificateCheckTest : MethodRuleTestFixture<DoNotShortCircuitCertificateCheckRule> {

		public void NonBoolReturnValue (int a, int b, int c, int d)
		{
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply<DoNotShortCircuitCertificateCheckTest> ("NonBoolReturnValue");
		}

		// e.g. an application where the local time source cannot be trusted
		public class AllowExpiredCertificatePolicy : ICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return ((uint) certificateProblem == ((uint) 0x800B0101));
			}
		}

		// e.g. an application that is specific to a (service using a) specific certificate
		public class AllowSpecificCertificatePolicy : ICertificatePolicy {

			bool ICertificatePolicy.CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return (certificate.GetCertHashString () == "D62F48D013EE7FB58B79074512670D9C5B3A5DA9");
			}
		}

		[Test]
		public void PolicySuccess ()
		{
			AssertRuleSuccess<AllowExpiredCertificatePolicy> ("CheckValidationResult");
			AssertRuleSuccess<AllowSpecificCertificatePolicy> ("System.Net.ICertificatePolicy.CheckValidationResult");
		}

		public class NotImplementedCertificatePolicy : ICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				throw new NotImplementedException ();
			}
		}

		public class NullCertificatePolicy : ICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return true;
			}
		}

		public class DualCertificatePolicy : ICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return true;
			}
		
			bool ICertificatePolicy.CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return (request != null);
			}
		}

		[Test]
		public void PolicyFailure ()
		{
			AssertRuleFailure<NotImplementedCertificatePolicy> ("CheckValidationResult", 1);
			AssertRuleFailure<NullCertificatePolicy> ("CheckValidationResult", 1);
			AssertRuleFailure<DualCertificatePolicy> ("CheckValidationResult", 1);
			AssertRuleFailure<DualCertificatePolicy> ("System.Net.ICertificatePolicy.CheckValidationResult", 1);
		}

		public class DoesNotImplementICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return true;
			}
		}

		public class ImplementICertificatePolicy : ICertificatePolicy {

			public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return true;
			}

			public bool CheckValidationResultFalse (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return false;
			}
		}

		[Test]
		public void PolicyMisnamed ()
		{
			AssertRuleSuccess<DoesNotImplementICertificatePolicy> ("CheckValidationResult");
			AssertRuleSuccess<ImplementICertificatePolicy> ("CheckValidationResultFalse");
		}

		public interface IMyPolicy : ICertificatePolicy {
			string Name { get; }
		}

		abstract public class AbstractIndirectPolicy : IMyPolicy {

			bool result;

			public AbstractIndirectPolicy (bool value)
			{
				result = value;
			}

			public string Name {
				get { return "My Policy"; }
			}

			protected bool Result {
				get { return result; }
			}

			abstract public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem);
		}

		public class IndirectPolicy : AbstractIndirectPolicy {

			public IndirectPolicy ()
				: base (false)
			{
			}

			public override bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
			{
				return Result;
			}
		}

		[Test]
		public void PolicyIndirect ()
		{
			AssertRuleDoesNotApply<AbstractIndirectPolicy> ("CheckValidationResult");
			AssertRuleFailure<IndirectPolicy> ("CheckValidationResult", 1);
		}

		public bool AllowAnyNameRemoteCertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch);
		}

		public bool AllowExpiredRemoteCertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return (chain.ChainStatus [0].Status == X509ChainStatusFlags.NotTimeValid); 
		}

		public bool AllowSpecificCertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return (certificate.GetCertHashString () == "D62F48D013EE7FB58B79074512670D9C5B3A5DA9");
		}

		[Test]
		public void CallbackSuccess ()
		{
			AssertRuleSuccess<DoNotShortCircuitCertificateCheckTest> ("AllowAnyNameRemoteCertificateValidation");
			AssertRuleSuccess<DoNotShortCircuitCertificateCheckTest> ("AllowExpiredRemoteCertificateValidation");
			AssertRuleSuccess<DoNotShortCircuitCertificateCheckTest> ("AllowSpecificCertificateValidation");
		}

		public bool NotImplementedRemoteCertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			throw new NotImplementedException ();
		}

		public bool NullRemoteCertificateValidation (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		[Test]
		public void CallbackFailure ()
		{
			AssertRuleFailure<DoNotShortCircuitCertificateCheckTest> ("NotImplementedRemoteCertificateValidation", 1);
			AssertRuleFailure<DoNotShortCircuitCertificateCheckTest> ("NullRemoteCertificateValidation", 1);
		}

		public bool BoolReturnValue (int a, int b, int c, int d)
		{
			return false;
		}

		[Test]
		public void NoneSuccess ()
		{
			AssertRuleSuccess<DoNotShortCircuitCertificateCheckTest> ("BoolReturnValue");
		}
	}
}
