// 
// Gendarme.Rules.Design.AttributeArgumentsShouldHaveAccessorsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	public class AttributeArgumentsShouldHaveAccessorsRule : ITypeRule {

		private List<string> allProperties = new List<string> ();

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			// rule applies only to attributes
			if (!typeDefinition.IsAttribute ())
				return runner.RuleSuccess;

			// look through getters
			allProperties.Clear ();
			foreach (PropertyDefinition property in typeDefinition.Properties) {
				if (property.GetMethod != null) {
					allProperties.Add (property.Name);
				}
			}

			Location loc = new Location (typeDefinition);
			MessageCollection messages = runner.RuleSuccess;

			// look through parameters
			foreach (MethodDefinition constructor in typeDefinition.Constructors) {
				foreach (ParameterDefinition param in constructor.Parameters) {
					string correspondingPropertyName = char.ToUpper (param.Name [0]) + param.Name.Substring (1); // pascal case it
					if (!allProperties.Contains (correspondingPropertyName)) {
						Message msg = new Message (
							string.Format ("All parameter values passed to an attribute constructor must be visible through read-only properties. Add '{0}' property to the attribute class.", correspondingPropertyName),
									   loc, MessageType.Error);
						if (messages == runner.RuleSuccess)
							messages = new MessageCollection ();
						messages.Add (msg);
						allProperties.Add (correspondingPropertyName); // to avoid double catching same property (e.g. from different constructors)
					}
				}
			}
			return messages;
		}
	}
}
