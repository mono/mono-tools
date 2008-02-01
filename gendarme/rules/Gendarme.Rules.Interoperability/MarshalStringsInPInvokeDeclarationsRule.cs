//
// Gendarme.Rules.Interoperability.MarshalStringsInPInvokeDeclarationsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2008 Daniel Abramov
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

namespace Gendarme.Rules.Interoperability {

	public class MarshalStringsInPInvokeDeclarationsRule : IMethodRule {

		private static bool IsStringOrSBuilder (TypeReference reference)
		{
			TypeReference original = reference.GetOriginalType ();
			return original.FullName == "System.String" || original.FullName == "System.Text.StringBuilder";
		}

		private static void AddBadParameterMessage (ref MessageCollection messages, ParameterDefinition param)
		{
			if (messages == null)
				messages = new MessageCollection ();

			Location loc = new Location (param.Method as MethodDefinition);
			string text = string.Format ("Parameter '{0}', of type '{1}', does not have [MarshalAs] attribute, yet no [DllImport CharSet=] is set for the method '{2}'.", 
				param.Name, param.ParameterType.Name, param.Method.Name);
			Message msg = new Message (text, loc, MessageType.Error);
			messages.Add (msg);
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.IsPInvokeImpl || !method.PInvokeInfo.IsCharSetNotSpec)
				return runner.RuleSuccess;

			MessageCollection messages = runner.RuleSuccess;
			foreach (ParameterDefinition parameter in method.Parameters) {
				if (IsStringOrSBuilder (parameter.ParameterType) && (parameter.MarshalSpec == null)) {
					AddBadParameterMessage (ref messages, parameter);
				}
			}
			return messages;
		}
	}
}
