//
// Gendarme.Rules.Smells.AvoidCodeDuplicatedInSiblingClassesRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections.Generic;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	[Problem ("There is similar code in various methods in sibling classes.  Your code will be better if you can unify them.")]
	[Solution ("You can apply the Pull Up Method refactoring.")]
	public class AvoidCodeDuplicatedInSiblingClassesRule : Rule, ITypeRule {

		private CodeDuplicatedLocator codeDuplicatedLocator = new CodeDuplicatedLocator ();

		private void FindCodeDuplicated (TypeDefinition type, ICollection<TypeDefinition> siblingClasses)
		{
			foreach (MethodDefinition method in type.Methods)
				foreach (TypeDefinition sibling in siblingClasses)
					codeDuplicatedLocator.CompareMethodAgainstTypeMethods (this, method, sibling);
		}

		private void CompareSiblingClasses (ICollection<TypeDefinition> siblingClasses)
		{
			foreach (TypeDefinition type in siblingClasses) {
				FindCodeDuplicated (type, siblingClasses);
				codeDuplicatedLocator.CheckedTypes.AddIfNew (type.Name);
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			ICollection<TypeDefinition> siblingClasses = Utilities.GetInheritedClassesFrom (type);
			if (siblingClasses.Count >= 2) {
				codeDuplicatedLocator.Clear ();
				CompareSiblingClasses (siblingClasses);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
