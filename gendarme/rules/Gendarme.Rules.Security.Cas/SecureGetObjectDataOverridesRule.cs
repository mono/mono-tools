//
// Gendarme.Rules.Security.Cas.SecureGetObjectDataOverridesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security;
using System.Security.Permissions;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Security.Cas {

	/// <summary>
	/// This rule fires if a type implements <c>System.Runtime.Serialization.ISerializable</c>
	/// but the <c>GetObjectData</c> method is not protected with a <c>Demand</c> or 
	/// <c>LinkDemand</c> for <c>SerializationFormatter</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad : ISerializable {
	/// 	public override void GetObjectData (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good : ISerializable {
	///	[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
	/// 	public override void GetObjectData (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security.</remarks>

	[Problem ("The method is not protected correctly against a serialization attack.")]
	[Solution ("A security Demand for SerializationFormatter should be added to protect the method.")]
	public class SecureGetObjectDataOverridesRule : Rule, ITypeRule {

		private const string NotFound = "No [Link]Demand was found.";

		static PermissionSet _ruleSet;

		static PermissionSet RuleSet {
			get {
				if (_ruleSet == null) {
					SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.SerializationFormatter);
					_ruleSet = new PermissionSet (PermissionState.None);
					_ruleSet.AddPermission (sp);
				}
				return _ruleSet;
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to types that implements ISerializable
			if (!type.Implements ("System.Runtime.Serialization", "ISerializable"))
				return RuleResult.DoesNotApply;

			MethodDefinition method = type.GetMethod (MethodSignatures.GetObjectData);
			if (method == null)
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// is there any security applied ?
			if (!method.HasSecurityDeclarations) {
				Runner.Report (method, Severity.High, Confidence.Total, NotFound);
				return RuleResult.Failure;
			}

			// the SerializationFormatter must be a subset of the one (of the) demand(s)
			bool demand = false;
			foreach (SecurityDeclaration declsec in method.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.Demand:
				case Mono.Cecil.SecurityAction.NonCasDemand:
				case Mono.Cecil.SecurityAction.LinkDemand:
				case Mono.Cecil.SecurityAction.NonCasLinkDemand:
					demand = true;
					if (!RuleSet.IsSubsetOf (declsec.ToPermissionSet ())) {
						string message = String.Format (CultureInfo.InvariantCulture,
							"{0} is not a subset of {1} permission set",
							"SerializationFormatter", declsec.Action);
						Runner.Report (method, Severity.High, Confidence.Total, message);
					}
					break;
				}
			}

			// there was no [NonCas][Link]Demand but other actions are possible
			if (!demand)
				Runner.Report (method, Severity.High, Confidence.Total, NotFound);

			return Runner.CurrentRuleResult;
		}
	}
}
