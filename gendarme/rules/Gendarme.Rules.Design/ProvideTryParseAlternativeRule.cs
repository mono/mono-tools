//
// Gendarme.Rules.Design.ProvideTryParseAlternativeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule will warn on any type that provide <c>Parse(string...)</c>
	/// method(s) without supplying <c>TryParse</c> alternative method(s).
	/// Using a <c>TryParse</c> method is easier since it is less-error prone 
	/// (no need to handle all possible exceptions) and remove the performance 
	/// issue regarding throwing/catching exceptions.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public struct Int32Tuple {
	/// 	private int first;
	/// 	private int second;
	/// 	
	/// 	public Int32Tuple (int a, int b)
	/// 	{
	/// 		first = a;
	/// 		second = b;
	/// 	}
	/// 	
	///	// documented to throw only Argument[Null]Exception or FormatException
	/// 	static Int32Tuple Parse (string s)
	/// 	{
	/// 		if (s == null) {
	/// 			throw new ArgumentNullException ("s");
	/// 		} else if (s.Length == 0) {
	/// 			throw new ArgumentException ("s");
	/// 		} else {
	/// 			string [] items = s.Split (':');
	/// 			// without the try/catch it would be much harder to
	/// 			// be 100% certain of all possible exceptions - like
	/// 			// IndexOfOfRangeException and what Int32.Parse can throw
	/// 			try {
	/// 				int a = Int32.Parse (items [0]);
	/// 				int b = Int32.Parse (items [1]);
	/// 				return new Int32Tuple (a, b);
	/// 			}
	/// 			catch (Exception e) {
	/// 				throw new FormatException ("Invalid data", e);
	/// 			}
	/// 		}
	/// 	}
	/// }	
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public struct Int32Tuple {
	/// 	private int first;
	/// 	private int second;
	/// 	
	/// 	public Int32Tuple (int a, int b)
	/// 	{
	/// 		first = a;
	/// 		second = b;
	/// 	}
	/// 	
	///	// documented to throw only Argument[Null]Exception or FormatException
	/// 	static Int32Tuple Parse (string s)
	/// 	{
	/// 		if (s == null) {
	/// 			throw new ArgumentNullException ("s");
	/// 		} else if (s.Length == 0) {
	/// 			throw new ArgumentException ("s");
	/// 		} else {
	/// 			// re-implemented using the exception-less TryParse
	/// 			Int32Tuple tuple;
	/// 			if (!TryParse (s, out tuple))
	/// 				throw new FormatException ("Invalid data");
	/// 			return tuple;
	/// 		}
	/// 	}
	/// 	
	/// 	static bool TryParse (string s, out Int32Tuple tuple)
	/// 	{
	/// 		tuple = new Int32Tuple ();
	/// 		if (String.IsNullOrEmpty (s))
	/// 			return false;
	/// 		string [] items = s.Split (':');
	/// 		if (items.Length != 2)
	/// 			return false;
	/// 		int a;
	/// 		if (!Int32.TryParse (s, out a))
	/// 			return false;
	/// 		int b;
	/// 		if (!Int32.TryParse (s, out b))
	/// 			return false;
	/// 		tuple.first = a;
	/// 		tuple.second = b;
	/// 		return true;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("This type provides a Parse(System.String) method without an, exception-less, TryParse alternative.")]
	[Solution ("Add a TryParse alternative method that allow parsing without any exception handling from the caller.")]
	public class ProvideTryParseAlternativeRule : Rule, ITypeRule {

		static bool HasTryParseMethod (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				if (MethodSignatures.TryParse.Matches (method))
					return true;
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// we look for a Parse method defined in the type
			bool has_parse = false;
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				if (MethodSignatures.Parse.Matches (method)) {
					// the type provides a "<type> <type>::Parse(string)" method
					has_parse = true;
					break;
				}
			}

			// if no Parse method is found; or
			// if a Parse method is found along with a TryParse method
			if (!has_parse || HasTryParseMethod (type))
				return RuleResult.Success;

			// otherwise we report a defect to add a TryParse alternative
			Runner.Report (type, Severity.Medium, Confidence.High);
			return RuleResult.Failure;
		}
	}
}

