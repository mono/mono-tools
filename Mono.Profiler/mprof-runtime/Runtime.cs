using System;
using System.Runtime.CompilerServices;

namespace Mono.Profiler {
	public class RuntimeControls {
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void TakeHeapSnapshot ();
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void EnableProfiler ();
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void DisableProfiler ();
	}
}
