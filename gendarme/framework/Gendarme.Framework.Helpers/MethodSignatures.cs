//
// Gendarme.Framework.MethodSignatures
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Defines commonly used MethodSignatures
	/// </summary>
	/// <see cref="Gendarme.Framework.Helpers.MethodSignature"/>
	public static class MethodSignatures {
		private static readonly string [] NoParameter = new string [0];
		private static readonly string [] OneParameter = new string [1];
		private static readonly string [] TwoParameters = new string [2];

		// System.Object
		public static readonly new MethodSignature Equals = new MethodSignature ("Equals", "System.Boolean", new string [] { "System.Object" },  MethodAttributes.Public);
		public static readonly MethodSignature Finalize = new MethodSignature ("Finalize", "System.Void", NoParameter, MethodAttributes.Family);
		public static readonly new MethodSignature GetHashCode = new MethodSignature ("GetHashCode", "System.Int32", NoParameter, MethodAttributes.Public | MethodAttributes.Virtual);
		public static readonly new MethodSignature ToString = new MethodSignature ("ToString", "System.String", NoParameter, MethodAttributes.Public | MethodAttributes.Virtual);

		// IClonable
		public static readonly MethodSignature Clone = new MethodSignature ("Clone", null, NoParameter);

		// IDisposable
		public static readonly MethodSignature Dispose = new MethodSignature ("Dispose", "System.Void", NoParameter);
		public static readonly MethodSignature DisposeExplicit = new MethodSignature ("System.IDisposable.Dispose", "System.Void", NoParameter);

		// ISerialization
		private static string [] SerializationParameters = new string [] { "System.Runtime.Serialization.SerializationInfo", "System.Runtime.Serialization.StreamingContext" };
		public static readonly MethodSignature SerializationConstructor = new MethodSignature (".ctor", "System.Void", SerializationParameters);
		public static readonly MethodSignature GetObjectData = new MethodSignature ("GetObjectData", "System.Void", SerializationParameters);
		public static readonly MethodSignature SerializationEventHandler = new MethodSignature (null, "System.Void", new string [] { "System.Runtime.Serialization.StreamingContext" }, MethodAttributes.Private);

		// operators
		private static readonly MethodAttributes OperatorAttributes = MethodAttributes.Static | MethodAttributes.SpecialName;
		
		// unary
		public static readonly MethodSignature op_UnaryPlus = new MethodSignature ("op_UnaryPlus", null, OneParameter, OperatorAttributes);		// +5
		public static readonly MethodSignature op_UnaryNegation = new MethodSignature ("op_UnaryNegation", null, OneParameter, OperatorAttributes);	// -5
		public static readonly MethodSignature op_LogicalNot = new MethodSignature ("op_LogicalNot", null, OneParameter, OperatorAttributes);		// !true
		public static readonly MethodSignature op_OnesComplement = new MethodSignature ("op_OnesComplement", null, OneParameter, OperatorAttributes);	// ~5

		public static readonly MethodSignature op_Increment = new MethodSignature ("op_Increment", null, OneParameter, OperatorAttributes);		// 5++
		public static readonly MethodSignature op_Decrement = new MethodSignature ("op_Decrement", null, OneParameter, OperatorAttributes);		// 5--
		public static readonly MethodSignature op_True = new MethodSignature ("op_True", "System.Boolean", OneParameter, OperatorAttributes);		// if (object)		
		public static readonly MethodSignature op_False = new MethodSignature ("op_False", "System.Boolean", OneParameter, OperatorAttributes);		// if (object)

		// binary
		public static readonly MethodSignature op_Addition = new MethodSignature ("op_Addition", null, TwoParameters, OperatorAttributes);		// 5 + 5
		public static readonly MethodSignature op_Subtraction = new MethodSignature ("op_Subtraction", null, TwoParameters, OperatorAttributes);	// 5 - 5 
		public static readonly MethodSignature op_Multiply = new MethodSignature ("op_Multiply", null, TwoParameters, OperatorAttributes);		// 5 * 5
		public static readonly MethodSignature op_Division = new MethodSignature ("op_Division", null, TwoParameters, OperatorAttributes);		// 5 / 5
		public static readonly MethodSignature op_Modulus = new MethodSignature ("op_Modulus", null, TwoParameters, OperatorAttributes);		// 5 % 5

		public static readonly MethodSignature op_BitwiseAnd = new MethodSignature ("op_BitwiseAnd", null, TwoParameters, OperatorAttributes);		// 5 & 5
		public static readonly MethodSignature op_BitwiseOr = new MethodSignature ("op_BitwiseOr", null, TwoParameters, OperatorAttributes);		// 5 | 5
		public static readonly MethodSignature op_ExclusiveOr = new MethodSignature ("op_ExclusiveOr", null, TwoParameters, OperatorAttributes);	// 5 ^ 5

		public static readonly MethodSignature op_LeftShift = new MethodSignature ("op_LeftShift", null, TwoParameters, OperatorAttributes);		// 5 << 5
		public static readonly MethodSignature op_RightShift = new MethodSignature ("op_RightShift", null, TwoParameters, OperatorAttributes);		// 5 >> 5

		// comparison
		public static readonly MethodSignature op_Equality = new MethodSignature ("op_Equality", null, TwoParameters, OperatorAttributes);			// 5 == 5
		public static readonly MethodSignature op_Inequality = new MethodSignature ("op_Inequality", null, TwoParameters, OperatorAttributes);			// 5 != 5
		public static readonly MethodSignature op_GreaterThan = new MethodSignature ("op_GreaterThan", null, TwoParameters, OperatorAttributes);		// 5 > 5
		public static readonly MethodSignature op_LessThan = new MethodSignature ("op_LessThan", null, TwoParameters, OperatorAttributes);			// 5 < 5
		public static readonly MethodSignature op_GreaterThanOrEqual = new MethodSignature ("op_GreaterThanOrEqual", null, TwoParameters, OperatorAttributes);	// 5 >= 5
		public static readonly MethodSignature op_LessThanOrEqual = new MethodSignature ("op_LessThanOrEqual", null, TwoParameters, OperatorAttributes);	// 5 <= 5

		//Invoke
		public static readonly MethodSignature Invoke = new MethodSignature ("Invoke");
	}
}
