// 
// Gendarme.Rules.Exceptions.DoNotThrowReservedExceptionRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule will fire if an <c>System.ExecutionEngineException</c>, <c>System.IndexOutOfRangeException</c>,
	/// <c>NullReferenceException</c>, or <c>System.OutOfMemoryException</c> class is
	/// instantiated. These exceptions are for use by the runtime and should not be thrown by
	/// user code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Add (object obj)
	/// {
	///	if (obj == null) {
	///		throw new NullReferenceException ("obj");
	///	}
	///	Inner.Add (obj);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void Add (object obj)
	/// {
	///	if (obj == null) {
	///		throw new ArgumentNullException ("obj");
	///	}
	///	Inner.Add (obj);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method creates an ExecutionEngineException, IndexOutOfRangeException, NullReferenceException, or OutOfMemoryException.")]
	[Solution ("Throw an exception which is not reserved by the runtime.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
	public class DoNotThrowReservedExceptionRule : NewExceptionsRule {

		protected override bool CheckException (TypeReference type)
		{
			if (type == null)
				return false;

			switch (type.Name) {
			case "ExecutionEngineException":
			case "IndexOutOfRangeException":
			case "NullReferenceException":
			case "OutOfMemoryException":
				return (type.Namespace == "System");
			default:
				return false;
			}
		}

		protected override Severity Severity {
			get { return Severity.High; }
		}
	}
}
