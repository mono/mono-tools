//
// Gendarme.Rules.Design.ProvideAlternativeNamesForOperatorOverloadsRule
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
using System.Collections.Generic;
using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	public class ProvideAlternativeNamesForOperatorOverloadsRule : ITypeRule {

		static string [] NoParameter = new string [] { };
		static string [] OneParameter = new string [] { null }; //new string [1] = one parameter of any type


		static MethodSignature Compare = new MethodSignature ("Compare", null, OneParameter);

		static KeyValuePair<MethodSignature, MethodSignature> [] AlternativeMethodNames = new KeyValuePair<MethodSignature, MethodSignature> [] { 
			//unary
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_UnaryPlus, new MethodSignature ("Plus", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_UnaryNegation, new MethodSignature ("Negate", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LogicalNot, new MethodSignature ("LogicalNot", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_OnesComplement, new MethodSignature ("OnesComplement", null, NoParameter)),
			
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Increment, new MethodSignature ("Increment", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Decrement, new MethodSignature ("Decrement", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_True, new MethodSignature ("IsTrue", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_False, new MethodSignature ("IsFalse", null, NoParameter)),

			//binary
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Addition, new MethodSignature ("Add", null, OneParameter)), 
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Subtraction, new MethodSignature ("Subtract", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Multiply, new MethodSignature ("Multiply", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Division, new MethodSignature ("Divide", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Modulus, new MethodSignature ("Modulus", null, OneParameter)),

			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_BitwiseAnd, new MethodSignature ("BitwiseAnd", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_BitwiseOr, new MethodSignature ("BitwiseOr", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_ExclusiveOr, new MethodSignature ("ExclusiveOr", null, OneParameter)),
			
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LeftShift, new MethodSignature ("LeftShift", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_RightShift, new MethodSignature ("RightShift", null, OneParameter)),
		
			// new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Equality, MethodSignatures.Equals), //handled by OverrideEqualsMethodRule
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Inequality, MethodSignatures.Equals),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThan, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LessThan, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThanOrEqual, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LessThanOrEqual, Compare),
		};

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			if (type.IsEnum || type.IsInterface)
				return runner.RuleSuccess;

			MessageCollection results = null;

			foreach (var kv in AlternativeMethodNames) {
				MethodDefinition op = type.GetMethod (kv.Key);
				if (op == null)
					continue;
				bool alternativeDefined = false;
				foreach (MethodDefinition alternative in type.Methods) {
					if (kv.Value.Matches (alternative)) {
						alternativeDefined = true;
						break;
					}
				}

				if (!alternativeDefined) {
					if (results == null)
						results = new MessageCollection ();
					Location loc = new Location (op);
					string s = String.Format ("This type implements the '{0}' operator. Some languages do not support overloaded operators so an alternative '{1}' method should be provided.",
						kv.Key.Name, kv.Value.Name);
					Message msg = new Message (s, loc, MessageType.Warning);
					results.Add (msg);
				}
			}

			return results;
		}
	}
}
