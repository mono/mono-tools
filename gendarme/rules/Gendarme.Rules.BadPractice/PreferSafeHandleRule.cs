// 
// Gendarme.Rules.BadPractice.PreferSafeHandleRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// In general it is best to interop with native code using 
	/// <c>System.Runtime.InteropServices.SafeHandle</c> instead of 
	/// <c>System.IntPtr</c> or <c>System.UIntPtr</c> because:
	/// <list type = "bullet">
	/// <item>
	/// <description>SafeHandles are type safe.</description>
	/// </item>
	/// <item>
	/// <description>SafeHandles are guaranteed to be disposed of during 
	/// exceptional conditions like a thread aborting unexpectedly or a stack 
	/// overflow.</description>
	/// </item>
	/// <item>
	/// <description>SafeHandles are not vulnerable to reycle attacks.</description>
	/// </item>
	/// <item>
	/// <description>You don't need to write a finalizer which can be tricky 
	/// to do because they execute within their own thread, may execute on 
	/// partially constructed objects, and normally tear down the application
	/// if you allow an exception to escape from them.</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// using System.Runtime.InteropServices;
	/// using System.Security;
	/// using System.Security.Permissions;
	/// 
	/// // If cleaning up the native resource in a timely manner is important you can
	/// // implement IDisposable.
	/// public sealed class Database {
	/// 	~Database ()
	/// 	{
	/// 		// This will execute even if the ctor throws so it is important to check
	/// 		// to see if the fields are initialized.
	/// 		if (m_database != IntPtr.Zero) {
	/// 			NativeMethods.sqlite3_close (m_database);
	/// 		}
	/// 	}
	/// 
	/// 	public Database (string path)
	/// 	{		
	/// 		NativeMethods.OpenFlags flags = NativeMethods.OpenFlags.READWRITE | NativeMethods.OpenFlags.CREATE;
	/// 		int err = NativeMethods.sqlite3_open_v2 (path, out m_database, flags, IntPtr.Zero);
	/// 		// handle errors
	/// 	}
	/// 	
	/// 	// exec and query methods would go here
	/// 							
	/// 	[SuppressUnmanagedCodeSecurity]
	/// 	private static class NativeMethods {
	/// 		[Flags]
	/// 		public enum OpenFlags : int {
	/// 			READONLY = 0x00000001,
	/// 			READWRITE = 0x00000002,
	/// 			CREATE = 0x00000004,
	/// 			// ...
	/// 		}
	/// 		
	/// 		[DllImport ("sqlite3")]
	/// 		public static extern int sqlite3_close (IntPtr db);
	/// 					
	/// 	   	[DllImport ("sqlite3")]
	/// 		public static extern int sqlite3_open_v2 (string fileName, out IntPtr db, OpenFlags flags, IntPtr module);
	/// 	}
	/// 	
	///     private IntPtr m_database;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// using System.Runtime.ConstrainedExecution;
	/// using System.Runtime.InteropServices;
	/// using System.Security;
	/// using System.Security.Permissions;
	/// 
	/// // If cleaning up the native resource in a timely manner is important you can
	/// // implement IDisposable, but you do not need to implement a finalizer because
	/// // SafeHandle will take care of the cleanup.
	/// internal sealed class Database {
	/// 	public Database (string path)
	/// 	{
	/// 		NativeMethods.OpenFlags flags = NativeMethods.OpenFlags.READWRITE | NativeMethods.OpenFlags.CREATE;
	/// 		m_database = new SqlitePtr (path, flags);
	/// 	}
	/// 	
	/// 	// exec and query methods would go here
	/// 	
	/// 	// This corresponds to a native sqlite3*.
	/// 	[SecurityPermission (SecurityAction.InheritanceDemand, UnmanagedCode = true)]
	/// 	[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
	/// 	private sealed class SqlitePtr : SafeHandle {
	/// 		public SqlitePtr (string path, NativeMethods.OpenFlags flags) : base (IntPtr.Zero, true)
	/// 		{		
	/// 			int err = NativeMethods.sqlite3_open_v2 (path, out handle, flags, IntPtr.Zero);
	/// 			// handle errors
	/// 		}
	/// 		
	/// 		public override bool IsInvalid { 
	/// 			get {
	/// 				return (handle == IntPtr.Zero);
	/// 			}
	/// 		}
	/// 		
	/// 		// This will not be called if the handle is invalid. Note that this method should not throw.
	/// 		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
	/// 		protected override bool ReleaseHandle ()
	/// 		{
	/// 			NativeMethods.sqlite3_close (this);
	/// 			return true;
	/// 		}
	/// 	}
	/// 
	/// 	[SuppressUnmanagedCodeSecurity]
	/// 	private static class NativeMethods {
	/// 		[Flags]
	/// 		public enum OpenFlags : int {
	/// 			READONLY = 0x00000001,
	/// 			READWRITE = 0x00000002,
	/// 			CREATE = 0x00000004,
	/// 			// ...
	/// 		}
	/// 		
	/// 		[DllImport ("sqlite3")]
	/// 		public static extern int sqlite3_close (SqlitePtr db);
	/// 				
	/// 		// Open must take an IntPtr but all other methods take a type safe SqlitePtr.
	/// 		[DllImport ("sqlite3")]
	/// 		public static extern int sqlite3_open_v2 (string fileName, out IntPtr db, OpenFlags flags, IntPtr module);
	/// 	}
	/// 
	/// 	private SqlitePtr m_database;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("The type uses System.IntPtr or System.UIntPtr instead of System.Runtime.InteropServices.SafeHandle.")]
	[Solution ("Consider changing the code to use SafeHandle.")]
	[FxCopCompatibility ("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
	public sealed class PreferSafeHandleRule : Rule, ITypeRule {
	
		static FieldDefinition FindIntPtr (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields) {
				TypeReference ftype = field.FieldType;
				if (ftype.Namespace == "System") {
					string name = ftype.Name;
					if ((name == "IntPtr") || (name == "UIntPtr"))
						return field;
				}
			}
			
			return null;
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// SafeHandle was introduced in .NET 2.0 so disable the rule if the
			// assembly is targeting something earlier.
			Runner.AnalyzeModule += (object o, RunnerEventArgs e) => {
				Active = e.CurrentModule.Runtime >= TargetRuntime.Net_2_0;
			};
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasFields || type.IsEnum)
				return RuleResult.DoesNotApply;

			Log.WriteLine (this);
			Log.WriteLine (this, "----------------------------------");
			Log.WriteLine (this, type);
						
			FieldDefinition field = FindIntPtr (type);
			if (field != null) {
				Confidence confidence = Confidence.Low;
				
				MethodDefinition finalizer = type.GetMethod (MethodSignatures.Finalize);
				if (finalizer != null) 
					confidence = (Confidence) ((int) confidence - 1);	// lower numbers have higher confidence

				if (type.Implements ("System", "IDisposable"))
					confidence = (Confidence) ((int) confidence - 1);

				Log.WriteLine (this, "'{0}' is an IntPtr.", field.Name);
				Runner.Report (field, Severity.Medium, confidence);
			}
						
			return Runner.CurrentRuleResult;
		}
	}
}
