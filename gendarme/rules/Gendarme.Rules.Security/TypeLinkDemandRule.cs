//
// Gendarme.Rules.Security.TypeLinkDemandRule
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
using System.Security;

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Security {

	public class TypeLinkDemandRule: ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// #1 - rule apply to types (and nested types) that are publicly visible
			switch (type.Attributes & TypeAttributes.VisibilityMask) {
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				break;
			default:
				return runner.RuleSuccess;
			}

			// #2 - rule apply to types that aren't sealed
			if (type.IsSealed)
				return runner.RuleSuccess;

			PermissionSet link = null;
			PermissionSet inherit = null;
			// #3 - rule apply to types with a LinkDemand
			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.LinkDemand:
				case Mono.Cecil.SecurityAction.NonCasLinkDemand:
					link = declsec.PermissionSet;
					break;
				case Mono.Cecil.SecurityAction.InheritDemand:
				case Mono.Cecil.SecurityAction.NonCasInheritance:
					inherit = declsec.PermissionSet;
					break;
				}
			}

			if (link == null)
				return runner.RuleSuccess; // no LinkDemand == no problem

			// #4 - rule apply if there are virtual methods defined
			bool virt = false;
			foreach (MethodDefinition method in type.Methods) {
				// #5 - ensure that the method is declared in this type (i.e. not in a parent)
				if (method.IsVirtual && ((method.DeclaringType as TypeDefinition) == type))
					virt = true;
			}

			if (!virt)
				return runner.RuleSuccess; // no virtual method == no problem

			// *** ok, the rule applies! ***

			// #5 - and ensure the LinkDemand is a subset of the InheritanceDemand
			if (inherit == null)
				return runner.RuleFailure; // LinkDemand without InheritanceDemand
			if (link.IsSubsetOf (inherit))
				return runner.RuleSuccess;
			else
				return runner.RuleFailure;
		}
	}
}
