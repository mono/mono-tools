//
// Gendarme.Rules.Correctness.ProvideValidXmlStringRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2009 Cedric Vivier
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
using System.Xml;
using System.Xml.XPath;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using System.Text.RegularExpressions;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule verifies that valid XML string arguments are passed as arguments.
	/// </summary>
	/// <example>
	/// Bad example (using LoadXml):
	/// <code>
	/// XmlDocument doc = new XmlDocument ();
	/// doc.LoadXml ("&lt;book&gt;");
	/// </code>
	/// </example>
	/// <example>
	/// Good example (using LoadXml):
	/// <code>
	/// XmlDocument doc = new XmlDocument ();
	/// doc.LoadXml ("&lt;book /&gt;");
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (using InnerXml):
	/// <code>
	/// bookElement.InnerXml = "&lt;author&gt;Robert J. Sawyer&lt;/authr&gt;";
	/// </code>
	/// </example>
	/// <example>
	/// Good example (using InnerXml):
	/// <code>
	/// bookElement.InnerXml = "&lt;author&gt;Robert J. Sawyer&lt;/author&gt;";
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("An invalid XML string is provided to a method.")]
	[Solution ("Fix the invalid XML string.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class ProvideValidXmlStringRule : Rule, IMethodRule {

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

		void CheckString (MethodDefinition method, Instruction ins, int argumentOffset)
		{
			Instruction ld = ins.TraceBack (method, argumentOffset);
			if (null == ld)
				return;

			switch (ld.OpCode.Code) {
			case Code.Ldstr:
				CheckString (method, ins, (string) ld.Operand);
				break;
			case Code.Ldsfld:
				FieldReference f = (FieldReference) ld.Operand;
				if (f.Name == "Empty" && f.DeclaringType.IsNamed ("System", "String"))
					CheckString (method, ins, null);
				break;
			case Code.Ldnull:
				CheckString (method, ins, null);
				break;
			}
		}

		void CheckString (MethodDefinition method, Instruction ins, string xml)
		{
			if (string.IsNullOrEmpty (xml)) {
				Runner.Report (method, ins, Severity.High, Confidence.Total, "XML string is null or empty.");
				return;
			}

			try {
				(new XmlDocument ()).LoadXml (xml);
			} catch (XmlException e) {
				string msg = String.Format (CultureInfo.InvariantCulture, 
					"XML string '{0}' is invalid. Details: {1}", xml, e.Message);
				Runner.Report (method, ins, Severity.High, Confidence.High, msg);
			}
		}

		void CheckCall (MethodDefinition method, Instruction ins, MethodReference mref)
		{
			if (null == mref || !mref.HasParameters)
				return;

			switch (mref.Name) {
			case "LoadXml":
				if (mref.DeclaringType.IsNamed ("System.Xml", "XmlDocument"))
					CheckString (method, ins, -1);
				break;
			case "set_InnerXml":
			case "set_OuterXml":
				TypeReference tr = mref.DeclaringType;
				if (tr.Inherits ("System.Xml", "XmlNode") || tr.Inherits ("System.Xml.XPath", "XPathNavigator"))
					CheckString (method, ins, -1);
				break;
			case "AppendChild":
			case "PrependChild":
			case "InsertAfter":
			case "InsertBefore":
				IList<ParameterDefinition> pdc = mref.Parameters;
				if (pdc.Count == 1
					&& pdc [0].ParameterType.IsNamed ("System", "String")
					&& mref.DeclaringType.Inherits ("System.Xml.XPath", "XPathNavigator"))
					CheckString (method, ins, -1);
				break;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			//is there any interesting opcode in the method?
			OpCodeBitmask calls = OpCodeBitmask.Calls;
			if (!calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (!calls.Get (ins.OpCode.Code))
					continue;

				CheckCall (method, ins, (MethodReference) ins.Operand);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
