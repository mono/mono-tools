//
// Gendarme.Rules.Smells.CodeDuplicatedLocator class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using System.Collections.Specialized;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Smells {

	internal class CodeDuplicatedLocator {
		private StringCollection methods = new StringCollection ();
		private StringCollection types = new StringCollection ();

		internal StringCollection CheckedMethods {
			get {
				return methods;
			}
		}

		internal StringCollection CheckedTypes {
			get {
				return types;
			}
		}

		private static bool ExistsExpressionsReplied (ICollection currentExpressions, ICollection targetExpressions)
		{
			IEnumerator currentEnumerator = currentExpressions.GetEnumerator ();
			IEnumerator targetEnumerator = targetExpressions.GetEnumerator ();
			bool equality = false;

			while (currentEnumerator.MoveNext () & targetEnumerator.MoveNext ()) {
				Expression currentExpression = (Expression) currentEnumerator.Current;
				Expression targetExpression = (Expression) targetEnumerator.Current;

				if (equality && currentExpression.Equals (targetExpression))
					return true;
				else {
					equality = currentExpression.Equals (targetExpression);
				}
			}
			return false;
		}

		private static ICollection GetExpressionsFrom (MethodBody methodBody)
		{
			ExpressionFillerVisitor expressionFillerVisitor = new ExpressionFillerVisitor ();
			methodBody.Accept (expressionFillerVisitor);
			return expressionFillerVisitor.Expressions;
		}

		private bool CanCompareMethods (MethodDefinition currentMethod, MethodDefinition targetMethod)
		{
			return currentMethod.HasBody && targetMethod.HasBody &&
				!CheckedMethods.Contains (targetMethod.Name) &&
				currentMethod != targetMethod;
		}

		private bool ContainsDuplicatedCode (MethodDefinition currentMethod, MethodDefinition targetMethod)
		{
			if (CanCompareMethods (currentMethod, targetMethod)) {
				ICollection currentExpressions = GetExpressionsFrom (currentMethod.Body);
				ICollection targetExpressions = GetExpressionsFrom (targetMethod.Body);

				return ExistsExpressionsReplied (currentExpressions, targetExpressions);
			}
			return false;
		}

		internal MessageCollection CompareMethodAgainstTypeMethods (MethodDefinition currentMethod, TypeDefinition targetTypeDefinition)
		{
			MessageCollection messageCollection = new MessageCollection ();
			if (!CheckedTypes.Contains (targetTypeDefinition.Name)) {
				foreach (MethodDefinition targetMethod in targetTypeDefinition.Methods) {
					if (ContainsDuplicatedCode (currentMethod, targetMethod)) {
						Location location = new Location (currentMethod);
						Message message = new Message (String.Format ("Exists code duplicated with {0}.{1}", targetTypeDefinition.Name, targetMethod.Name), location, MessageType.Error);
						messageCollection.Add (message);
					}
				}
			}
			return messageCollection;
		}
	}
}
