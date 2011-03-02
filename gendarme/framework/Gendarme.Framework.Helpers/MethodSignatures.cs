//
// Gendarme.Framework.MethodSignatures
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Defines commonly used MethodSignatures
	/// </summary>
	// <see cref="Gendarme.Framework.Helpers.MethodSignature"/>
	public static class MethodSignatures {
		private static readonly string [] NoParameter = new string [0];

		private static bool CheckAttributes (MethodReference method, MethodAttributes attrs)
		{
			MethodDefinition md = method.Resolve ();
			return ((md == null) || ((md.Attributes & attrs) == attrs));
		}

		// System.Object
		public static readonly new MethodSignature Equals = new MethodSignature ("Equals", "System.Boolean", new string [] { "System.Object" },
			(method) => (CheckAttributes (method, MethodAttributes.Public)));
		public static readonly MethodSignature Finalize = new MethodSignature ("Finalize", "System.Void", NoParameter,
			(method) => (CheckAttributes (method, MethodAttributes.Family)));
		public static readonly new MethodSignature GetHashCode = new MethodSignature ("GetHashCode", "System.Int32", NoParameter,
			(method) => (CheckAttributes (method, MethodAttributes.Public | MethodAttributes.Virtual)));
		public static readonly new MethodSignature ToString = new MethodSignature ("ToString", "System.String", NoParameter,
			(method) => (CheckAttributes (method, MethodAttributes.Public | MethodAttributes.Virtual)));

		// IClonable
		public static readonly MethodSignature Clone = new MethodSignature ("Clone", null, NoParameter);

		// IDisposable
		public static readonly MethodSignature Dispose = new MethodSignature ("Dispose", "System.Void", NoParameter);
		public static readonly MethodSignature DisposeExplicit = new MethodSignature ("System.IDisposable.Dispose", "System.Void", NoParameter);

		// ISerialization
		private static string [] SerializationParameters = new string [] { "System.Runtime.Serialization.SerializationInfo", "System.Runtime.Serialization.StreamingContext" };
		public static readonly MethodSignature SerializationConstructor = new MethodSignature (".ctor", "System.Void", SerializationParameters);
		public static readonly MethodSignature GetObjectData = new MethodSignature ("GetObjectData", "System.Void", SerializationParameters);
		public static readonly MethodSignature SerializationEventHandler = new MethodSignature (null, "System.Void", new string [] { "System.Runtime.Serialization.StreamingContext" },
			(method) => (CheckAttributes (method, MethodAttributes.Private)));

		// operators
		private static bool IsOperator (MethodReference method, int parameters)
		{
			if (!method.HasParameters || (method.Parameters.Count != parameters))
				return false;

			return CheckAttributes (method, MethodAttributes.Static | MethodAttributes.SpecialName);
		}

		// unary
		public static readonly MethodSignature op_UnaryPlus = new MethodSignature ("op_UnaryPlus", (method) => IsOperator (method, 1));				// +5
		public static readonly MethodSignature op_UnaryNegation = new MethodSignature ("op_UnaryNegation", (method) => IsOperator (method, 1));			// -5
		public static readonly MethodSignature op_LogicalNot = new MethodSignature ("op_LogicalNot", (method) => IsOperator (method, 1));			// !true
		public static readonly MethodSignature op_OnesComplement = new MethodSignature ("op_OnesComplement", (method) => IsOperator (method, 1));		// ~5

		public static readonly MethodSignature op_Increment = new MethodSignature ("op_Increment", (method) => IsOperator (method, 1));				// 5++
		public static readonly MethodSignature op_Decrement = new MethodSignature ("op_Decrement", (method) => IsOperator (method, 1));				// 5--
		public static readonly MethodSignature op_True = new MethodSignature ("op_True", "System.Boolean", (method) => IsOperator (method, 1));			// if (object)		
		public static readonly MethodSignature op_False = new MethodSignature ("op_False", "System.Boolean", (method) => IsOperator (method, 1));		// if (object)

		// binary
		public static readonly MethodSignature op_Addition = new MethodSignature ("op_Addition", (method) => IsOperator (method, 2));				// 5 + 5
		public static readonly MethodSignature op_Subtraction = new MethodSignature ("op_Subtraction", (method) => IsOperator (method, 2));			// 5 - 5 
		public static readonly MethodSignature op_Multiply = new MethodSignature ("op_Multiply", (method) => IsOperator (method, 2));				// 5 * 5
		public static readonly MethodSignature op_Division = new MethodSignature ("op_Division", (method) => IsOperator (method, 2));				// 5 / 5
		public static readonly MethodSignature op_Modulus = new MethodSignature ("op_Modulus", (method) => IsOperator (method, 2));				// 5 % 5

		public static readonly MethodSignature op_BitwiseAnd = new MethodSignature ("op_BitwiseAnd", (method) => IsOperator (method, 2));			// 5 & 5
		public static readonly MethodSignature op_BitwiseOr = new MethodSignature ("op_BitwiseOr", (method) => IsOperator (method, 2));				// 5 | 5
		public static readonly MethodSignature op_ExclusiveOr = new MethodSignature ("op_ExclusiveOr", (method) => IsOperator (method, 2));			// 5 ^ 5

		public static readonly MethodSignature op_LeftShift = new MethodSignature ("op_LeftShift", (method) => IsOperator (method, 2));				// 5 << 5
		public static readonly MethodSignature op_RightShift = new MethodSignature ("op_RightShift", (method) => IsOperator (method, 2));			// 5 >> 5

		// comparison
		public static readonly MethodSignature op_Equality = new MethodSignature ("op_Equality", (method) => IsOperator (method, 2));				// 5 == 5
		public static readonly MethodSignature op_Inequality = new MethodSignature ("op_Inequality", (method) => IsOperator (method, 2));			// 5 != 5
		public static readonly MethodSignature op_GreaterThan = new MethodSignature ("op_GreaterThan", (method) => IsOperator (method, 2));			// 5 > 5
		public static readonly MethodSignature op_LessThan = new MethodSignature ("op_LessThan", (method) => IsOperator (method, 2));				// 5 < 5
		public static readonly MethodSignature op_GreaterThanOrEqual = new MethodSignature ("op_GreaterThanOrEqual", (method) => IsOperator (method, 2));	// 5 >= 5
		public static readonly MethodSignature op_LessThanOrEqual = new MethodSignature ("op_LessThanOrEqual", (method) => IsOperator (method, 2));		// 5 <= 5

		// Invoke
		public static readonly MethodSignature Invoke = new MethodSignature ("Invoke");

		// TryParse
		public static readonly MethodSignature TryParse = new MethodSignature ("TryParse",
			delegate (MethodReference method) {
				if (!method.ReturnType.IsNamed ("System", "Boolean"))
					return false;

				IList<ParameterDefinition> pdc = method.Parameters;
				if (!pdc [0].ParameterType.IsNamed ("System", "String"))
					return false;

				TypeReference last = pdc [pdc.Count - 1].ParameterType;
				if (!last.IsByReference)
					return false;

				TypeReference mtype = method.DeclaringType;
				if (last.Namespace != mtype.Namespace)
					return false;

				string pt_name = last.Name;
				return (String.Compare (pt_name, 0, mtype.Name, 0, pt_name.Length - 1, StringComparison.Ordinal) == 0);
			}
		);

		// Parse - first parameter (required) is a string, return the same type as the declraring type
		public static readonly MethodSignature Parse = new MethodSignature ("Parse",
			delegate (MethodReference method) {
				if (!method.HasParameters)
					return false;
				if (!method.ReturnType.IsNamed (method.DeclaringType.Namespace, method.DeclaringType.Name))
					return false;
				return method.Parameters [0].ParameterType.IsNamed ("System", "String");
			}
		);
	}
}
