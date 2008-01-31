//
// DontSwallowErrorsCatchingNonspecificExceptions class
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Exceptions {
	public class DontSwallowErrorsCatchingNonspecificExceptionsRule : IMethodRule {

		//Added System.Object because is the code behind the following block:
		//try {
		//	File.Open (foo, bar);
		//}
		//catch {
		//}
		private string[] forbiddenTypeInCatches = {"System.Exception", "System.SystemException", "System.Object"};

		private bool IsForbiddenTypeInCatches (string typeName)
		{
			foreach (String forbiddenTypeName in forbiddenTypeInCatches) {
				if (typeName.Equals (forbiddenTypeName)) {
					return true;
				}
			}
			return false;
		}

		private static bool ThrowsGeneralException (ExceptionHandler exceptionHandler)
		{
			for (Instruction currentInstruction = exceptionHandler.HandlerStart; currentInstruction != exceptionHandler.HandlerEnd; currentInstruction = currentInstruction.Next) {
				if (currentInstruction.OpCode.Code == Code.Rethrow)
					return true;
			}
			return false;
		}

		public MessageCollection CheckMethod (MethodDefinition methodDefinition, Runner runner)
		{
			if (!methodDefinition.HasBody)
				return runner.RuleSuccess;

			MessageCollection messageCollection = null;
			ExceptionHandlerCollection exceptionHandlerCollection = methodDefinition.Body.ExceptionHandlers;
			foreach (ExceptionHandler exceptionHandler in exceptionHandlerCollection) {
				if (exceptionHandler.Type == ExceptionHandlerType.Catch) {
					string catchTypeName = exceptionHandler.CatchType.FullName;
					if (IsForbiddenTypeInCatches (catchTypeName)) {
						if (!ThrowsGeneralException (exceptionHandler)) {
							Location location = new Location (methodDefinition, exceptionHandler.HandlerStart.Offset);
							Message message = new Message ("Do not swallow errors catching nonspecific exceptions.", location, MessageType.Error);
							if (messageCollection == null)
								messageCollection = new MessageCollection (message);
							else
								messageCollection.Add (message);
						}
					}
				}
			}

			return messageCollection;
		}
	}
}
