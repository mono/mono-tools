// 
// Gendarme.Rules.BadPractice.OnlyUseDisposeForIDisposableTypesRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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

using System;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// To avoid confusing developers methods named Dispose should be
	/// reserved for types that implement IDisposable. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// internal sealed class Worker
	/// {
	/// 	// This class uses one or more temporary files to do its work.
	/// 	private List&lt;string&gt; files = new List&lt;string&gt; ();
	/// 	
	/// 	// This is confusing: developers will think they can do things
	/// 	// like use the instance with a using statement.
	/// 	public void Dispose ()
	/// 	{
	/// 		foreach (string path in files) {
	/// 			File.Delete (path);
	/// 		}
	/// 			
	/// 		files.Clear ();
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// internal sealed class Worker
	/// {
	/// 	// This class uses one or more temporary files to do its work.
	/// 	private List&lt;string&gt; files = new List&lt;string&gt; ();
	/// 	
	/// 	public void Reset ()
	/// 	{
	/// 		foreach (string path in files) {
	/// 			File.Delete (path);
	/// 		}
	/// 			
	/// 		files.Clear ();
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("A type has a method named Dispose, but does not implement IDisposable.")]
	[Solution ("Rename the method or implement IDisposable.")]
	public sealed class OnlyUseDisposeForIDisposableTypesRule : Rule, ITypeRule {
	
		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasMethods || type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			Log.WriteLine (this);
			Log.WriteLine (this, "----------------------------------");
			Log.WriteLine (this, type);
			
			if (!type.Implements ("System", "IDisposable")) {
				Log.WriteLine (this, "type does not implement IDisposable");

				foreach (MethodDefinition method in type.Methods.Where (m => m.Name == "Dispose"))
				{
					Log.WriteLine (this, "found {0}", method);
					
					Severity severity;
					if (method.IsVisible ())
						severity = Severity.High;
					else if (!method.IsPrivate)
						severity = Severity.Medium;
					else
						severity = Severity.Low;
					
					// Confidence is not total because we may not be able to resolve 
					// every base class.
					Runner.Report (method, severity, Confidence.High);	
				}
			}
									
			return Runner.CurrentRuleResult;
		}
	}
}
