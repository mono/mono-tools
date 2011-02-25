// 
// Gendarme.Rules.Design.UseCorrectDisposeSignaturesRule
//
// Authors:
//	Jesse Jones  <jesjones@mindpring.com>
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {
	
	/// <summary>
	/// There is a convention that should be followed when implementing <c>IDisposable</c>. Part
	/// of this convention is that Dispose methods should have specific signatures. In
	/// particular an <c>IDisposable</c> type's Dispose methods should either be nullary or unary
	/// with a bool argument, <c>Dispose ()</c> should not be virtual, <c>Dispose (bool)</c> should
	/// not be public, and unsealed types should have a <c>protected virtual Dispose (bool)</c> method.
	/// For more details see: [http://www.bluebytesoftware.com/blog/2005/04/08/DGUpdateDisposeFinalizationAndResourceManagement.aspx].
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Unsealed : IDisposable
	/// {
	/// 	~Unsealed ()
	/// 	{
	/// 		Dispose (false);
	/// 	}
	/// 	
	/// 	public void Dispose ()
	/// 	{
	/// 		Dispose (true);
	/// 		GC.SuppressFinalize (this);
	/// 	}
	/// 	
	/// 	// This is not virtual so resources in derived classes cannot be
	/// 	// cleaned up in a timely fashion if Unsealed.Dispose () is called.
	/// 	protected void Dispose (bool disposing)
	/// 	{
	/// 		if (!Disposed) {
	/// 			// clean up my resources
	/// 			Disposed = true;
	/// 		}
	/// 	}
	/// 	
	/// 	protected bool Disposed {
	/// 		get;
	/// 		set;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Unsealed : IDisposable
	/// {
	/// 	// Unsealed classes should have a finalizer even if they do nothing 
	/// 	// in the Dispose (false) case to ensure derived classes are cleaned 
	/// 	// up properly.
	/// 	~Unsealed ()
	/// 	{
	/// 		Dispose (false);
	/// 	}
	/// 	
	/// 	public Unsealed ()
	/// 	{
	/// 	}
	/// 	
	/// 	public void Work ()
	/// 	{
	/// 		// In general all public methods should throw ObjectDisposedException
	/// 		// if Dispose has been called.
	/// 		if (Disposed) {
	/// 			throw new ObjectDisposedException (GetType ().Name);
	/// 		}
	/// 	}
	/// 	
	/// 	public void Dispose ()
	/// 	{
	/// 		Dispose (true);
	/// 		GC.SuppressFinalize (this);
	/// 	}
	/// 	
	/// 	protected virtual void Dispose (bool disposing)
	/// 	{
	/// 		// Multiple Dispose calls should be OK.
	/// 		if (!Disposed) {
	/// 			if (disposing) {
	/// 				// None of our fields have been finalized so it's safe to
	/// 				// clean them up here. 
	/// 			}
	/// 		
	/// 			// Our fields may have been finalized so we should only
	/// 			// touch native fields (e.g. IntPtr or UIntPtr fields) here.
	/// 			Disposed = true;
	/// 		}
	/// 	}
	/// 	
	/// 	protected bool Disposed {
	/// 		get; 
	/// 		private set;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>
	
	[Problem ("An IDisposable type does not conform to the guidelines for its Dispose methods.")]
	[Solution ("Fix the signature of the methods or add the Dispose (bool) overload.")]
	public sealed class UseCorrectDisposeSignaturesRule : Rule, ITypeRule {
	
		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsInterface || type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;
			
			if (type.Implements ("System", "IDisposable")) {
				Log.WriteLine (this);
				Log.WriteLine (this, "----------------------------------");
				Log.WriteLine (this, type);
				
				MethodDefinition dispose0 = null;
				MethodDefinition dispose1 = null;
				FindDisposeMethods (type, ref dispose0, ref dispose1);
				
				// The compiler will normally require that the type have a declaration for
				// Dispose unless the base class also implements IDisposable. In that
				// case we'll ignore any defects because the type doesn't actually
				// implement IDisposable.
				if (dispose0 != null || dispose1 != null) {
					CheckDispose0 (dispose0);
					CheckDispose1 (type, dispose1);
				}
			}
			
			return Runner.CurrentRuleResult;
		}
		
		private void FindDisposeMethods (TypeDefinition type, ref MethodDefinition dispose0, ref MethodDefinition dispose1)
		{
			foreach (MethodDefinition method in type.Methods.Where (m => m.Name == "Dispose")) {
				if (MethodSignatures.Dispose.Matches (method)) {
					dispose0 = method;
				
				} else if (DisposeBool.Matches (method)) {
					dispose1 = method;
				
				} else {
					string message = "Found a Dispose method with a bad signature.";
					Log.WriteLine (this, "{0}", message);
					Runner.Report (method, Severity.Medium, Confidence.Total, message);
				}
			}
		}
		
		private void CheckDispose0 (MethodDefinition dispose0)
		{
			if (dispose0 != null) {
				if (dispose0.IsVirtual && !dispose0.IsFinal) {
					string message = "Dispose () should not be virtual.";
					Log.WriteLine (this, "{0}", message);
					Runner.Report (dispose0, Severity.Medium, Confidence.Total, message);
				}
				
				if (!dispose0.IsVirtual && (dispose0.Attributes & MethodAttributes.NewSlot) == 0) {
					string message = "The type should not hide the base class Dispose () method.";
					Log.WriteLine (this, "{0}", message);
					Runner.Report (dispose0, Severity.Medium, Confidence.Total, message);
				}
			}
		}
		
		private void CheckDispose1 (TypeDefinition type, MethodDefinition dispose1)
		{
			if (type.IsSealed) {
				if (dispose1 != null) {				// sealed classes don't need Dispose (bool)
					if (!dispose1.IsPrivate && DirectlyImplementsIDisposable (type)) {
						string message = "Dispose (bool) should be private for sealed types.";
						Log.WriteLine (this, "{0}", message);
						Runner.Report (dispose1, Severity.Medium, Confidence.Total, message);
					}
				}
			
			} else {
				if (dispose1 == null) {
					if (DirectlyImplementsIDisposable (type)) {
						string message = "Unsealed types should have a protected virtual Dispose (bool) method.";
						Log.WriteLine (this, "{0}", message);
						Runner.Report (type, Severity.Medium, Confidence.Total, message);
					}
				
				} else {
					if (!dispose1.IsFamily) {
						string message = "Dispose (bool) should be protected for unsealed types.";
						Log.WriteLine (this, "{0}", message);
						Runner.Report (type, Severity.Medium, Confidence.Total, message);
					}
					
					if (!dispose1.IsPrivate && !dispose1.IsVirtual) {
						string message = "Dispose (bool) should be virtual for unsealed types.";
						Log.WriteLine (this, "{0}", message);
						Runner.Report (type, Severity.Medium, Confidence.Total, message);
					}
				}
			}
		}
		
		static bool DirectlyImplementsIDisposable (TypeDefinition type)
		{
			if (type.HasInterfaces) {
				foreach (TypeReference candidate in type.Interfaces) {
					if (candidate.IsNamed ("System", "IDisposable"))
						return true;
				}
			}
			
			return false;
		}
		
		private static readonly MethodSignature DisposeBool = new MethodSignature ("Dispose", "System.Void", new string [] { "System.Boolean"});
	}
}
