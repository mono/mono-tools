//
// Gendarme.Rules.Security.SealedTypeWithInheritanceDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Security {

	public class SealedTypeWithInheritanceDemandRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// 1 - this applies only to sealed types
			if (!type.IsSealed)
				return runner.RuleSuccess;

			// 2 - the type must have an InheritanceDemand
			if (type.SecurityDeclarations.Count == 0)
				return runner.RuleSuccess;

			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				if (declsec.Action == SecurityAction.InheritDemand)
					return runner.RuleFailure;
			}
			// the action wasn't an inheritance demand check
			return runner.RuleSuccess;
		}
	}
}
