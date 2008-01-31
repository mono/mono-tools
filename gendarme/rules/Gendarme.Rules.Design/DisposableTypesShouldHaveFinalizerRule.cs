//
// Gendarme.Rules.Design.DisposableTypesShouldHaveFinalizerRule
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

namespace Gendarme.Rules.Design {

	public class DisposableTypesShouldHaveFinalizerRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			if (!type.Implements ("System.IDisposable"))
				return runner.RuleSuccess;
			if (type.GetFinalizer () != null)
				return runner.RuleSuccess;

			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				if (field.FieldType.GetOriginalType ().IsNative ()) {
					Location loc = new Location (type);
					Message msg = new Message (string.Format ("{0} implements IDisposable and has native fields. It should implement a finalizer to release unmanaged resources.", type.FullName), loc, MessageType.Error);
					return new MessageCollection (msg);
				}
			}

			return runner.RuleSuccess;
		}
	}
}
