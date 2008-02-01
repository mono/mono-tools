//
// Gendarme.Rules.Naming.ParameterNamesShouldMatchOverridenMethodRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

namespace Gendarme.Rules.Naming {

	public class ParameterNamesShouldMatchOverridenMethodRule : IMethodRule {

		private static bool SignatureMatches (MethodDefinition method, MethodDefinition baseMethod, bool explicitInterfaceCheck)
		{
			if (method.Name != baseMethod.Name) {
				if (!explicitInterfaceCheck)
					return false;
				if (method.Name != baseMethod.DeclaringType.FullName + "." + baseMethod.Name)
					return false;
			}
			if (method.ReturnType.ReturnType.ToString () != baseMethod.ReturnType.ReturnType.ToString ())
				return false;
			if (method.Parameters.Count != baseMethod.Parameters.Count)
				return false;
			bool paramtersMatch = true;
			for (int i = 0; i < method.Parameters.Count; i++) {
				if (method.Parameters [i].ParameterType.ToString () != baseMethod.Parameters [i].ParameterType.ToString ()) {
					paramtersMatch = false;
					break;
				}
			}
			if (!paramtersMatch)
				return false;
			return true;
		}

		private static MethodDefinition GetBaseMethod (MethodDefinition method)
		{
			TypeDefinition baseType = (TypeDefinition) method.DeclaringType;
			while (baseType != baseType.BaseType) { //System.Object extends System.Object in cecil
				baseType = baseType.BaseType as TypeDefinition;
				if (baseType == null) //TODO: ToTypeDefinition ()
					break;
				foreach (MethodDefinition baseMethodCandidate in baseType.Methods) {
					if (SignatureMatches (method, baseMethodCandidate, false))
						return baseMethodCandidate;
				}
			}
			return null;
		}

		private static MethodDefinition GetInterfaceMethod (MethodDefinition method)
		{
			TypeDefinition type = (TypeDefinition) method.DeclaringType;
			foreach (TypeReference interfaceReference in type.Interfaces) {
				TypeDefinition interfaceCandidate = interfaceReference as TypeDefinition;
				if (interfaceCandidate == null)
					continue; //TODO: ToTypeDefinition ();
				foreach (MethodDefinition interfaceMethodCandidate in interfaceCandidate.Methods) {
					if (SignatureMatches (method, interfaceMethodCandidate, true))
						return interfaceMethodCandidate;
				}
			}
			return null;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.IsVirtual)
				return runner.RuleSuccess;

			MethodDefinition baseMethod = null;
			if (!method.IsNewSlot)
				baseMethod = GetBaseMethod (method);
			if (baseMethod == null)
				baseMethod = GetInterfaceMethod (method);
			if (baseMethod == null)
				return runner.RuleSuccess;

			MessageCollection results = null;

			for (int i = 0; i < method.Parameters.Count; i++) {
				if (method.Parameters [i].Name != baseMethod.Parameters [i].Name) {
					if (results == null)
						results = new MessageCollection ();
					Location loc = new Location (method);
					Message msg = new Message (string.Format ("The name of parameter {0} ({1}) does not match the name of the parameter in the overriden method ({2}).", i + 1, method.Parameters [i].Name, baseMethod.Parameters [i].Name), loc, MessageType.Warning);
					results.Add (msg);
				}
			}
			return results;
		}
	}
}
