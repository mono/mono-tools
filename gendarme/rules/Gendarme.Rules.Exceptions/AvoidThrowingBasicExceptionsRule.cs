// 
// Gendarme.Rules.Exceptions.AvoidThrowingBasicExceptionsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
	/// This rule checks for methods that create basic exceptions like <c>System.Exception</c>,
	/// <c>System.ApplicationException</c> or <c>System.SystemException</c>. Those exceptions
	/// do not provide enough information about the error to be helpful to the consumer
	/// of the library.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Add (object obj)
	/// {
	///	if (obj == null) {
	///		throw new Exception ();
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

	[Problem ("This method creates (and probably throws) an exception of Exception, ApplicationException or SystemException type.")]
	[Solution ("Try to use a more specific exception type. If none of existing types meet your needs, create a custom exception class that inherits from System.Exception or any appropriate descendant of it.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
	public class AvoidThrowingBasicExceptionsRule : NewExceptionsRule {

		protected override bool CheckException (TypeReference type)
		{
			if ((type == null) || (type.Namespace != "System"))
				return false;
			string name = type.Name;
			return ((name == "Exception") || (name == "ApplicationException") || (name == "SystemException"));
		}

		protected override Severity Severity {
			get { return Severity.Medium; }
		}
	}
}
