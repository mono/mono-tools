//
// Gendarme.Rules.Design.PreferXmlAbstractionsRule
//
// Authors:
//	Cedric Vivier  <cedricv@neonux.com>
//
// Copyright (C) 2009 Cedric Vivier
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
using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an externally visible method or property uses an <c>XmlDocument</c>, 
	/// <c>XPathDocument</c> or <c>XmlNode</c> argument. The problem with this is that it ties 
	/// your API to a specific implementation so it is difficult to change later. Instead use 
	/// abstract types like <c>IXPathNavigable</c>, <c>XmlReader</c>, <c>XmlWriter</c>, or subtypes
	/// of <c>XmlNode</c>.
	/// </summary>
	/// <example>
	/// Bad example (property):
	/// <code>
	/// public class Application {
	///	public XmlDocument UserData {
	///		get {
	///			return userData;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (property):
	/// <code>
	/// public class Application {
	///	public IXPathNavigable UserData {
	///		get {
	///			return userData;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (method parameter):
	/// <code>
	/// public class Application {
	///	public bool IsValidUserData (XmlDocument userData) 
	///	{
	///		/* implementation */
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (method parameter):
	/// <code>
	/// public class Application {
	///	public bool IsValidUserData (XmlReader userData) 
	///	{
	///		/* implementation */
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("This visible method uses XmlDocument, XPathDocument or XmlNode in its signature. This makes changing the implementation more difficult than it should be.")]
	[Solution ("Use IXPathNavigable, XmlReader, XmlWriter, or a subtype of XmlNode instead.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
	public class PreferXmlAbstractionsRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				foreach (AssemblyNameReference name in e.CurrentModule.AssemblyReferences) {
					if (name.Name == "System.Xml") {
						Active = true;
						return;
					}
				}
				Active = false; //no System.Xml assembly reference has been found
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsVisible ())
				return RuleResult.DoesNotApply;

			MethodReturnType mrt = method.MethodReturnType;
			if (IsSpecificXmlType (mrt.ReturnType))
				Runner.Report (mrt, GetSeverity (method), Confidence.High);

			if (method.HasParameters) {
				foreach (ParameterDefinition parameter in method.Parameters) {
					if (parameter.IsOut)
						continue; //out params already have their rule

					if (IsSpecificXmlType (parameter.ParameterType))
						Runner.Report (parameter, GetSeverity (method), Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}

		static bool IsSpecificXmlType (TypeReference type)
		{
			if (type.Namespace == "System.Xml") {
				string name = type.Name;
				return ((name == "XmlDocument") || (name == "XmlNode"));
			}
			return type.IsNamed ("System.Xml.XPath", "XPathDocument");
		}

		static Severity GetSeverity (MethodDefinition method)
		{
			return method.IsPublic ? Severity.Medium : Severity.Low;
		}
	}
}
