//
// Gendarme.Rules.Design.TypesWithDisposableFieldsShouldBeDisposableRule
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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	public class TypesWithDisposableFieldsShouldBeDisposableRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// rule doesn't apply to enums, interfaces or structs
			if (type.IsEnum || type.IsInterface || type.IsValueType)
				return runner.RuleSuccess;

			MethodDefinition explicitDisposeMethod = null;
			MethodDefinition implicitDisposeMethod = null;

			bool abstractWarning = false;

			if (type.Implements ("System.IDisposable")) {
				implicitDisposeMethod = type.GetMethod (MethodSignatures.Dispose);
				explicitDisposeMethod = type.GetMethod (MethodSignatures.DisposeExplicit);

				if (IsAbstractMethod (implicitDisposeMethod))
					abstractWarning = true;
				if (IsAbstractMethod (explicitDisposeMethod))
					abstractWarning = true;

				if (abstractWarning == false)
					return runner.RuleSuccess;
			}

			MessageCollection results = null;

			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				TypeDefinition fieldType = field.FieldType.GetOriginalType () as TypeDefinition;
				if (fieldType == null)
					continue; //TODO: Implemts for TypeReference
				if (fieldType.Implements ("System.IDisposable")) {
					if (results == null)
						results = new MessageCollection ();
					Location loc = new Location (field);
					if (abstractWarning)
						results.Add (new Message (string.Format ("{1} is Disposeable. {0} shoud implement a non abstract Dispose() method.", type.FullName, field.Name), loc, MessageType.Warning));
					else
						results.Add (new Message (string.Format ("{1} is Disposeable. {0} should implement System.IDisposable to release unmanaged resources.", type.FullName, field.Name), loc, MessageType.Error));

				}
			}

			if (results == null)
				return runner.RuleSuccess;

			if (IsAbstractMethod (implicitDisposeMethod))
				results.Add (GenerateAbstractWarning (implicitDisposeMethod));
			if (IsAbstractMethod (explicitDisposeMethod))
				results.Add (GenerateAbstractWarning (explicitDisposeMethod));

			return results;
		}

		private static bool IsAbstractMethod (MethodDefinition method)
		{
			return method != null && method.IsAbstract;
		}

		private static Message GenerateAbstractWarning (MethodDefinition method)
		{
			Location loc = new Location (method);
			return new Message (string.Format ("{0} has at least one Disposable field. Marking {1}() as abstract shifts the job of disposing those fields to the inheritors of this class.", method.DeclaringType.FullName, method.Name), loc, MessageType.Warning);
		}
	}
}
