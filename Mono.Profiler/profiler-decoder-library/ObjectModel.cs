// Author:
// Massimiliano Mantione (massi@ximian.com)
//
// (C) 2008 Novell, Inc  http://www.novell.com
//

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
using System.IO;
using System.Collections.Generic;

namespace  Mono.Profiler {
	public class LoadedClass : BaseLoadedClass {
		uint allocatedBytes;
		public uint AllocatedBytes {
			get {
				return allocatedBytes;
			}
		}
		uint currentlyAllocatedBytes;
		public uint CurrentlyAllocatedBytes {
			get {
				return currentlyAllocatedBytes;
			}
		}
		public static Comparison<LoadedClass> CompareByAllocatedBytes = delegate (LoadedClass a, LoadedClass b) {
			return a.AllocatedBytes.CompareTo (b.AllocatedBytes);
		};
		
		internal void InstanceCreated (uint size, LoadedMethod method, bool jitTime, StackTrace stackTrace) {
			allocatedBytes += size;
			currentlyAllocatedBytes += size;
			if (method != null) {
				Dictionary<uint,AllocationsPerMethod> methods;
				if (! jitTime) {
					if (allocationsPerMethod == null) {
						allocationsPerMethod = new Dictionary<uint,AllocationsPerMethod> ();
					}
					methods = allocationsPerMethod;
				} else {
					if (allocationsPerMethodAtJitTime == null) {
						allocationsPerMethodAtJitTime = new Dictionary<uint,AllocationsPerMethod> ();
					}
					methods = allocationsPerMethodAtJitTime;
				}
				
				AllocationsPerMethod callerMethod;
				if (methods.ContainsKey (method.ID)) {
					callerMethod = methods [method.ID];
				} else {
					callerMethod = new AllocationsPerMethod (method);
					methods.Add (method.ID, callerMethod);
				}
				callerMethod.Allocation (size, stackTrace);
			}
		}
		
		internal void InstanceFreed (uint size) {
			currentlyAllocatedBytes -= size;
		}
		
		public abstract class AllocationsPerItem<T> {
			protected T item;
			uint allocatedBytes;
			public uint AllocatedBytes {
				get {
					return allocatedBytes;
				}
			}
			uint allocatedInstances;
			public uint AllocatedInstances {
				get {
					return allocatedInstances;
				}
			}
			protected void InternalAllocation (uint allocatedBytes) {
				this.allocatedBytes += allocatedBytes;
				this.allocatedInstances ++;
			}
			
			public static Comparison<AllocationsPerItem<T>> CompareByAllocatedBytes = delegate (AllocationsPerItem<T> a, AllocationsPerItem<T> b) {
				return a.AllocatedBytes.CompareTo (b.AllocatedBytes);
			};
			public static Comparison<AllocationsPerItem<T>> CompareByAllocatedInstances = delegate (AllocationsPerItem<T> a, AllocationsPerItem<T> b) {
				return a.AllocatedInstances.CompareTo (b.AllocatedInstances);
			};
			
			protected AllocationsPerItem (T item) {
				this.item = item;
				allocatedInstances = 0;
				allocatedBytes = 0;
			}
		}
		public class AllocationsPerMethod : AllocationsPerItem<LoadedMethod> {
			public LoadedMethod Method {
				get {
					return item;
				}
			}
			
			Dictionary<uint,AllocationsPerStackTrace> stackTraces;
			public AllocationsPerStackTrace[] StackTraces {
				get {
					if (stackTraces != null) {
						AllocationsPerStackTrace[] result = new AllocationsPerStackTrace [stackTraces.Count];
						stackTraces.Values.CopyTo (result, 0);
						return result;
					} else {
						return new AllocationsPerStackTrace [0];
					}
				}
			}
			public int StackTracesCount {
				get {
					if (stackTraces != null) {
						return stackTraces.Count;
					} else {
						return 0;
					}
				}
			}
			
			internal void Allocation (uint allocatedBytes, StackTrace stackTrace) {
				InternalAllocation (allocatedBytes);
				if (stackTrace != null) {
					if (stackTraces == null) {
						stackTraces = new Dictionary<uint,AllocationsPerStackTrace> ();
					}
					
					AllocationsPerStackTrace allocationsPerStackTrace;
					if (stackTraces.ContainsKey (stackTrace.ID)) {
						allocationsPerStackTrace = stackTraces [stackTrace.ID];
					} else {
						allocationsPerStackTrace = new AllocationsPerStackTrace (stackTrace);
						stackTraces [stackTrace.ID] = allocationsPerStackTrace;
					}
					allocationsPerStackTrace.Allocation (allocatedBytes);
				}
			}
			
			public AllocationsPerMethod (LoadedMethod method) : base (method) {
			}
		}
		public class AllocationsPerStackTrace : AllocationsPerItem<StackTrace> {
			public StackTrace Trace {
				get {
					return item;
				}
			}
			
			internal void Allocation (uint allocatedBytes) {
				InternalAllocation (allocatedBytes);
			}
			
			public AllocationsPerStackTrace (StackTrace trace) : base (trace) {
			}
		}
		
		Dictionary<uint,AllocationsPerMethod> allocationsPerMethod;
		public AllocationsPerMethod[] Methods {
			get {
				if (allocationsPerMethod != null) {
					AllocationsPerMethod[] result = new AllocationsPerMethod [allocationsPerMethod.Count];
					allocationsPerMethod.Values.CopyTo (result, 0);
					return result;
				} else {
					return new AllocationsPerMethod [0];
				}
			}
		}
		
		Dictionary<uint,AllocationsPerMethod> allocationsPerMethodAtJitTime;
		public AllocationsPerMethod[] MethodsAtJitTime {
			get {
				if (allocationsPerMethodAtJitTime != null) {
					AllocationsPerMethod[] result = new AllocationsPerMethod [allocationsPerMethodAtJitTime.Count];
					allocationsPerMethodAtJitTime.Values.CopyTo (result, 0);
					return result;
				} else {
					return new AllocationsPerMethod [0];
				}
			}
		}
		public int MethodsAtJitTimeCount {
			get {
				if (allocationsPerMethodAtJitTime != null) {
					return allocationsPerMethodAtJitTime.Values.Count;
				} else {
					return 0;
				}
			}
		}
		
		public LoadedClass (uint id, string name, uint size): base (id, name, size) {
			allocatedBytes = 0;
			currentlyAllocatedBytes = 0;
			allocationsPerMethod = null;
		}
	}
	
	public class StackTrace {
		LoadedMethod topMethod;
		public LoadedMethod TopMethod {
			get {
				return topMethod;
			}
		}
		StackTrace caller;
		public StackTrace Caller {
			get {
				return caller;
			}
		}
		bool methodIsBeingJitted;
		public bool MethodIsBeingJitted {
			get {
				return methodIsBeingJitted;
			}
		}
		uint level;
		public uint Level {
			get {
				return level;
			}
		}
		uint id;
		public uint ID {
			get {
				return id;
			}
		}
		
		static uint nextFreeId = 1;
		
		StackTrace (LoadedMethod topMethod, StackTrace caller, bool methodIsBeingJitted) {
			this.topMethod = topMethod;
			this.caller = caller;
			this.methodIsBeingJitted = methodIsBeingJitted;
			this.level = caller != null ? caller.level + 1 : 1;
			this.id = nextFreeId;
			nextFreeId ++;
		}
		
		bool MatchesCallStack (CallStack.StackFrame stack) {
			StackTrace currentTrace = this;
			
			while ((currentTrace != null) && (stack != null)) {
				if (currentTrace.TopMethod != stack.Method) {
					return false;
				}
				if (currentTrace.methodIsBeingJitted != stack.IsBeingJitted) {
					return false;
				}
				currentTrace = currentTrace.Caller;
				stack = stack.Caller;
			}
			
			if ((currentTrace == null) && (stack == null)) {
				return true;
			} else {
				return false;
			}
		}
		
		internal static StackTrace NewStackTrace (CallStack stack) {
			return NewStackTrace (stack.StackTop);
		}
		
		static StackTrace NewStackTrace (CallStack.StackFrame frame) {
			if (frame == null) {
				return null;
			}
			
			if (frame.Level> (uint) tracesByLevel.Length) {
				Dictionary<uint,List<StackTrace>> [] newTracesByLevel = new Dictionary<uint,List<StackTrace>> [frame.Level * 2];
				Array.Copy (tracesByLevel, newTracesByLevel, tracesByLevel.Length);
				tracesByLevel = newTracesByLevel;
			}
			
			Dictionary<uint,List<StackTrace>> tracesByMethod = tracesByLevel [frame.Level];
			if (tracesByLevel [frame.Level] == null) {
				tracesByMethod = new Dictionary<uint,List<StackTrace>> ();
				tracesByLevel [frame.Level] = tracesByMethod;
			}
			
			List<StackTrace> traces;
			if (tracesByMethod.ContainsKey (frame.Method.ID)) {
				traces = tracesByMethod [frame.Method.ID];
			} else {
				traces = new List<StackTrace> ();
				tracesByMethod [frame.Method.ID] = traces;
			}
			
			foreach (StackTrace trace in traces) {
				if (trace.MatchesCallStack (frame)) {
					return trace;
				}
			}
			
			StackTrace callerTrace = NewStackTrace (frame.Caller);
			StackTrace result = new StackTrace (frame.Method, callerTrace, frame.IsBeingJitted);
			traces.Add (result);
			return result;
		}
		
		static Dictionary<uint,List<StackTrace>> [] tracesByLevel = new Dictionary<uint,List<StackTrace>> [64];
	}
	
	class CallStack {
		internal class StackFrame {
			LoadedMethod method;
			public LoadedMethod Method {
				get {
					return method;
				}
				internal set {
					method = value;
				}
			}
			ulong startCounter;
			public ulong StartCounter {
				get {
					return startCounter;
				}
				internal set {
					startCounter = value;
				}
			}
			bool isBeingJitted;
			public bool IsBeingJitted {
				get {
					return isBeingJitted;
				}
				internal set {
					isBeingJitted = value;
				}
			}
			StackFrame caller;
			public StackFrame Caller {
				get {
					return caller;
				}
				internal set {
					caller = value;
				}
			}
			uint level;
			public uint Level {
				get {
					return level;
				}
			}
			
			public void SetLevel () {
				level = (caller != null) ? (caller.Level + 1) : 1;
			}
			
			internal StackFrame (LoadedMethod method, ulong startCounter, bool isBeingJitted, StackFrame caller) {
				this.method = method;
				this.startCounter = startCounter;
				this.isBeingJitted = isBeingJitted;
				this.caller = caller;
				SetLevel ();
			}
			
			static StackFrame freeFrames = null;
			internal static StackFrame FrameFactory (LoadedMethod method, ulong startCounter, bool isBeingJitted, StackFrame caller) {
				if (freeFrames != null) {
					StackFrame result = freeFrames;
					freeFrames = result.Caller;
					result.Method = method;
					result.startCounter = startCounter;
					result.isBeingJitted = isBeingJitted;
					result.Caller = caller;
					result.SetLevel ();
					return result;
				} else {
					return new StackFrame (method, startCounter, isBeingJitted, caller);
				}
			}
			internal static void FreeFrame (StackFrame frame) {
				frame.Caller = freeFrames;
				freeFrames = frame;
			}
		}
		
		ulong threadId;
		public ulong ThreadId {
			get {
				return threadId;
			}
		}
		
		StackFrame stackTop;
		internal StackFrame StackTop {
			get {
				return stackTop;
			}
		}
		
		public uint Depth {
			get {
				return (stackTop != null) ? stackTop.Level : 0;
			}
		}
		
		void PopMethod (LoadedMethod method, ulong counter, bool isBeingJitted) {
			while (stackTop != null) {
				LoadedMethod topMethod = stackTop.Method;
				bool topMethodIsBeingJitted = stackTop.IsBeingJitted;
				StackFrame callerFrame = stackTop.Caller;
				LoadedMethod callerMethod = (callerFrame != null)? callerFrame.Method : null;
				
				if (! topMethodIsBeingJitted) {
					topMethod.MethodCalled (counter - stackTop.StartCounter, callerMethod);
				}
				
				StackFrame.FreeFrame (stackTop);
				stackTop = callerFrame;
				if ((topMethod == method) && (topMethodIsBeingJitted == isBeingJitted)) {
					return;
				}
			}
		}
		
		internal void MethodEnter (LoadedMethod method, ulong counter) {
			stackTop = StackFrame.FrameFactory (method, counter, false, stackTop);
		}
		internal void MethodExit (LoadedMethod method, ulong counter) {
			PopMethod (method, counter, false);
		}
		internal void TopMethodExit (ulong counter) {
			MethodExit (stackTop.Method, counter);
		}
		
		internal void MethodJitStart (LoadedMethod method, ulong counter) {
			stackTop = StackFrame.FrameFactory (method, counter, true, stackTop);
		}
		internal void MethodJitEnd (LoadedMethod method, ulong counter) {
			PopMethod (method, counter, true);
		}
		
		internal void AdjustStack (uint lastValidFrame, uint topSectionSize, StackSectionElement<LoadedClass,LoadedMethod>[] topSection) {
			if (Depth >= lastValidFrame) {
				while (Depth > lastValidFrame) {
					StackFrame lastTop = stackTop;
					stackTop = stackTop.Caller;
					StackFrame.FreeFrame (lastTop);
				}
				for (int i = (int) topSectionSize - 1; i >= 0; i--) {
					stackTop = StackFrame.FrameFactory (topSection [i].Method, 0, topSection [i].IsBeingJitted, stackTop);
				}
			} else {
				throw new Exception (String.Format ("Depth is {0} but lastValidFrame is {1}", Depth, lastValidFrame));
			}
		}
		
		internal CallStack (ulong threadId) {
			this.threadId = threadId;
			stackTop = null;
		}
	}
	
	
	public class StatisticalHitItemCallInformation {
		IStatisticalHitItem item;
		public IStatisticalHitItem Item {
			get {
				return item;
			}
		}
		uint calls;
		public uint Calls {
			get {
				return calls;
			}
			internal set {
				calls = value;
			}
		}
		internal void AddCall () {
			calls ++;
		}
		public StatisticalHitItemCallInformation (IStatisticalHitItem item) {
			this.item = item;
			this.calls = 0;
		}
	}
	
	public class StatisticalHitItemCallCounts {
		public static Comparison<IStatisticalHitItem> CompareByStatisticalHits = delegate (IStatisticalHitItem a, IStatisticalHitItem b) {
			int result = a.StatisticalHits.CompareTo (b.StatisticalHits);
			if ((result == 0) && a.HasCallCounts && b.HasCallCounts) {
				StatisticalHitItemCallCounts aCounts = a.CallCounts;
				StatisticalHitItemCallCounts bCounts = b.CallCounts;
				result = aCounts.CallersCount.CompareTo (bCounts.CallersCount);
				if (result == 0) {
					result = aCounts.CalleesCount.CompareTo (bCounts.CalleesCount);
				}
			}
			return result;
		};
		
		List<StatisticalHitItemCallInformation> callers;
		List<StatisticalHitItemCallInformation> callees;
		
		static StatisticalHitItemCallInformation[] GetSortedInfo (List<StatisticalHitItemCallInformation> list) {
			StatisticalHitItemCallInformation[] result = list.ToArray ();
			Array.Sort (result, delegate (StatisticalHitItemCallInformation a, StatisticalHitItemCallInformation b) {
				return a.Calls.CompareTo (b.Calls);
			});
			Array.Reverse (result);
			return result;
		}
		
		public StatisticalHitItemCallInformation[] Callers {
			get {
				return GetSortedInfo (callers);
			}
		}
		public StatisticalHitItemCallInformation[] Callees {
			get {
				return GetSortedInfo (callees);
			}
		}
				
		public int CallersCount {
			get {
				return callers.Count;
			}
		}
		public int CalleesCount {
			get {
				return callees.Count;
			}
		}
		
		void AddCall (List<StatisticalHitItemCallInformation> list, IStatisticalHitItem item) {
			foreach (StatisticalHitItemCallInformation info in list) {
				if (info.Item == item) {
					info.AddCall ();
					return;
				}
			}
			StatisticalHitItemCallInformation newInfo = new StatisticalHitItemCallInformation (item);
			newInfo.AddCall ();
			list.Add (newInfo);
		}
		
		internal void AddCaller (IStatisticalHitItem caller) {
			AddCall (callers, caller);
		}
		internal void AddCallee (IStatisticalHitItem callee) {
			AddCall (callees, callee);
		}
		
		public StatisticalHitItemCallCounts () {
			callers = new List<StatisticalHitItemCallInformation> ();
			callees = new List<StatisticalHitItemCallInformation> ();
		}
	}
	
	public interface IStatisticalHitItem {
		string Name {get;}
		uint StatisticalHits {get;}
		bool HasCallCounts {get;}
		StatisticalHitItemCallCounts CallCounts {get;}
	}
	
	public class LoadedMethod : BaseLoadedMethod<LoadedClass>, IStatisticalHitItem {
		ulong clicks;
		public ulong Clicks {
			get {
				return clicks;
			}
			internal set {
				clicks = value;
			}
		}
		public static Comparison<LoadedMethod> CompareByTotalClicks = delegate (LoadedMethod a, LoadedMethod b) {
			return a.Clicks.CompareTo (b.Clicks);
		};
		public static Comparison<LoadedMethod> CompareByEffectiveClicks = delegate (LoadedMethod a, LoadedMethod b) {
			return (a.Clicks - a.CalledClicks).CompareTo (b.Clicks - b.CalledClicks);
		};
		
		ulong calledClicks;
		public ulong CalledClicks {
			get {
				return calledClicks;
			}
			internal set {
				calledClicks = value;
			}
		}
		
		uint statisticalHits;
		public uint StatisticalHits {
			get {
				return statisticalHits;
			}
			internal set {
				statisticalHits = value;
			}
		}
		string IStatisticalHitItem.Name {
			get {
				return Class.Name + "." + this.Name;
			}
		}
		
		StatisticalHitItemCallCounts callCounts;
		public bool HasCallCounts {
			get {
				return (callCounts != null);
			}
		}
		public StatisticalHitItemCallCounts CallCounts {
			get {
				if (callCounts == null) {
					callCounts = new StatisticalHitItemCallCounts ();
				}
				return callCounts;
			}
		}
		
		ulong startJit;
		public ulong StartJit {
			get {
				return startJit;
			}
			internal set {
				startJit = value;
			}
		}
		ulong jitClicks;
		public ulong JitClicks {
			get {
				return jitClicks;
			}
			internal set {
				jitClicks = value;
			}
		}
		public static Comparison<LoadedMethod> CompareByJitClicks = delegate (LoadedMethod a, LoadedMethod b) {
			return a.JitClicks.CompareTo (b.JitClicks);
		};
		
		public class ClicksPerCalledMethod {
			LoadedMethod method;
			public LoadedMethod Method {
				get {
					return method;
				}
			}
			
			ulong clicks;
			public ulong Clicks {
				get {
					return clicks;
				}
				internal set {
					clicks = value;
				}
			}
			public static Comparison<ClicksPerCalledMethod> CompareByClicks = delegate (ClicksPerCalledMethod a, ClicksPerCalledMethod b) {
				return a.Clicks.CompareTo (b.Clicks);
			};
			
			public ClicksPerCalledMethod (LoadedMethod method) {
				this.method = method;
				clicks = 0;
			}
		}
		
		Dictionary<uint,ClicksPerCalledMethod> clicksPerCalledMethod;
		public ClicksPerCalledMethod[] Methods {
			get {
				if (clicksPerCalledMethod != null) {
					ClicksPerCalledMethod[] result = new ClicksPerCalledMethod [clicksPerCalledMethod.Count];
					clicksPerCalledMethod.Values.CopyTo (result, 0);
					return result;
				} else {
					return new ClicksPerCalledMethod [0];
				}
			}
		}
		
		public class CallsPerCallerMethod {
			LoadedMethod method;
			public LoadedMethod Callees {
				get {
					return method;
				}
			}
			
			uint calls;
			public uint Calls {
				get {
					return calls;
				}
				internal set {
					calls = value;
				}
			}
			public static Comparison<CallsPerCallerMethod> CompareByCalls = delegate (CallsPerCallerMethod a, CallsPerCallerMethod b) {
				return a.Calls.CompareTo (b.Calls);
			};
			
			public CallsPerCallerMethod (LoadedMethod method) {
				this.method = method;
				calls = 0;
			}
		}
		
		Dictionary<uint,CallsPerCallerMethod> callsPerCallerMethod;
		public CallsPerCallerMethod[] Callers {
			get {
				if (callsPerCallerMethod != null) {
					CallsPerCallerMethod[] result = new CallsPerCallerMethod [callsPerCallerMethod.Count];
					callsPerCallerMethod.Values.CopyTo (result, 0);
					return result;
				} else {
					return new CallsPerCallerMethod [0];
				}
			}
		}
		
		internal void MethodCalled (ulong clicks, LoadedMethod caller) {
			this.clicks += clicks;
			
			if (caller != null) {
				caller.CalleeReturns (this, clicks);
				
				if (callsPerCallerMethod == null) {
					callsPerCallerMethod = new Dictionary<uint,CallsPerCallerMethod> ();
				}
				
				CallsPerCallerMethod callerCalls;
				if (callsPerCallerMethod.ContainsKey (caller.ID)) {
					callerCalls = callsPerCallerMethod [caller.ID];
				} else {
					callerCalls = new CallsPerCallerMethod (caller);
					callsPerCallerMethod.Add (caller.ID, callerCalls);
				}
				
				callerCalls.Calls += 1;
			}
		}
		
		internal void CalleeReturns (LoadedMethod callee, ulong clicks) {
			if (clicksPerCalledMethod == null) {
				clicksPerCalledMethod = new Dictionary<uint,ClicksPerCalledMethod> ();
			}
			
			ClicksPerCalledMethod calledMethodClicks;
			if (clicksPerCalledMethod.ContainsKey (callee.ID)) {
				calledMethodClicks = clicksPerCalledMethod [callee.ID];
			} else {
				calledMethodClicks = new ClicksPerCalledMethod (callee);
				clicksPerCalledMethod.Add (callee.ID, calledMethodClicks);
			}
			
			calledMethodClicks.Clicks += clicks;
			calledClicks += clicks;
		}
		
		public LoadedMethod (uint id, LoadedClass c, string name): base (id, c, name) {
			clicks = 0;
			calledClicks = 0;
			jitClicks = 0;
			statisticalHits = 0;
		}
	}
	
	public class UnmanagedFunctionFromID : BaseUnmanagedFunctionFromID<ExecutableMemoryRegion,UnmanagedFunctionFromRegion>, IStatisticalHitItem {
		uint statisticalHits;
		public uint StatisticalHits {
			get {
				return statisticalHits;
			}
			internal set {
				statisticalHits = value;
			}
		}
		
		string IStatisticalHitItem.Name {
			get {
				return "[" + Region.Name + "]" + this.Name;
			}
		}
		
		StatisticalHitItemCallCounts callCounts;
		public bool HasCallCounts {
			get {
				return (callCounts != null);
			}
		}
		public StatisticalHitItemCallCounts CallCounts {
			get {
				if (callCounts == null) {
					callCounts = new StatisticalHitItemCallCounts ();
				}
				return callCounts;
			}
		}
		
		public UnmanagedFunctionFromID (uint id, string name, ExecutableMemoryRegion region) : base (id, name, region) {
			statisticalHits = 0;
		}
	}
	
	public class UnmanagedFunctionFromRegion : BaseUnmanagedFunctionFromRegion, IStatisticalHitItem {
		uint statisticalHits;
		public uint StatisticalHits {
			get {
				return statisticalHits;
			}
			internal set {
				statisticalHits = value;
			}
		}
		
		public UnmanagedFunctionFromRegion () {
			statisticalHits = 0;
		}
		
		StatisticalHitItemCallCounts callCounts;
		public bool HasCallCounts {
			get {
				return (callCounts != null);
			}
		}
		public StatisticalHitItemCallCounts CallCounts {
			get {
				if (callCounts == null) {
					callCounts = new StatisticalHitItemCallCounts ();
				}
				return callCounts;
			}
		}
		
		string IStatisticalHitItem.Name {
			get {
				IExecutableMemoryRegion<IUnmanagedFunctionFromRegion> r = Region;
				return String.Format ("[{0}({1}-{2})]{3}", r != null ? r.Name : "NULL", this.StartOffset, this.EndOffset, this.Name);
			}
		}
	}
	
	public class ExecutableMemoryRegion : BaseExecutableMemoryRegion<UnmanagedFunctionFromRegion>, IStatisticalHitItem {
		uint statisticalHits;
		public uint StatisticalHits {
			get {
				return statisticalHits;
			}
			internal set {
				statisticalHits = value;
			}
		}
		internal void IncrementStatisticalHits () {
			statisticalHits ++;
		}
		
		string IStatisticalHitItem.Name {
			get {
				return String.Format ("[{0}](UNKNOWN)", this.Name);
			}
		}
		
		StatisticalHitItemCallCounts callCounts;
		public bool HasCallCounts {
			get {
				return (callCounts != null);
			}
		}
		public StatisticalHitItemCallCounts CallCounts {
			get {
				if (callCounts == null) {
					callCounts = new StatisticalHitItemCallCounts ();
				}
				return callCounts;
			}
		}
		
		public ExecutableMemoryRegion (uint id, string name, uint fileOffset, ulong startAddress, ulong endAddress) : base (id, name, fileOffset, startAddress, endAddress) {
				statisticalHits = 0;
		}
	}
	
	public class HeapObject : BaseHeapObject<HeapObject,LoadedClass> {
		public HeapObject (ulong ID) : base (ID) {}
	}
	
	public class HeapSnapshot : BaseHeapSnapshot<HeapObject,LoadedClass> {
		public class AllocationStatisticsPerClass {
			LoadedClass c;
			public LoadedClass Class {
				get {
					return c;
				}
				internal set {
					c = value;
				}
			}
			uint allocatedBytes;
			public uint AllocatedBytes {
				get {
					return allocatedBytes;
				}
			}
			uint freedBytes;
			public uint FreedBytes {
				get {
					return freedBytes;
				}
			}
			
			public static Comparison<AllocationStatisticsPerClass> CompareByAllocatedBytes = delegate (AllocationStatisticsPerClass a, AllocationStatisticsPerClass b) {
				return a.AllocatedBytes.CompareTo (b.AllocatedBytes);
			};
			
			public void BytesFreed (uint bytes) {
				allocatedBytes -= bytes;
				freedBytes += bytes;
			}
			
			public AllocationStatisticsPerClass (LoadedClass c) {
				this.c = c;
				this.allocatedBytes = c.CurrentlyAllocatedBytes;
				this.freedBytes = 0;
			}
		}
		
		AllocationStatisticsPerClass[] allocationStatistics;
		public AllocationStatisticsPerClass[] AllocationStatistics {
			get {
				int count = 0;
				foreach (AllocationStatisticsPerClass aspc in allocationStatistics) {
					if (aspc != null) {
						count ++;
					}
				}
				AllocationStatisticsPerClass[] result = new AllocationStatisticsPerClass [count];
				count = 0;
				foreach (AllocationStatisticsPerClass aspc in allocationStatistics) {
					if (aspc != null) {
						result [count] = aspc;
						count ++;
					}
				}
				return result;
			}
		}
		
		public void HeapObjectUnreachable (LoadedClass c, uint size) {
			AllocationStatisticsPerClass statisticsPerClass = allocationStatistics [c.ID];
			statisticsPerClass.BytesFreed (size);
		}
		
		public HeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, LoadedClass[] initialAllocations, bool recordSnapshot) : base (delegate (ulong ID) {return new HeapObject (ID);}, collection, startCounter, startTime, endCounter, endTime, recordSnapshot) {
			uint maxClassId = 0;
			foreach (LoadedClass c in initialAllocations) {
				if (c.ID > maxClassId) {
					maxClassId = c.ID;
				}
			}
			allocationStatistics = new AllocationStatisticsPerClass [maxClassId + 1];
			foreach (LoadedClass c in initialAllocations) {
				AllocationStatisticsPerClass statisticsPerClass = new AllocationStatisticsPerClass (c);
				allocationStatistics [c.ID] = statisticsPerClass;
			}
		}
	}
	
	public interface IHeapObjectFilter {
		string Description {
			get;
		}
		bool Filter (HeapObject heapObject);
	}
	
	public abstract class FilterHeapObjectByClass : IHeapObjectFilter {
		protected LoadedClass c;
		public LoadedClass Class {
			get {
				return c;
			}
		}
		
		public abstract bool Filter (HeapObject heapObject);
		
		string description;
		public string Description {
			get {
				return description;
			}
		}
		
		protected FilterHeapObjectByClass (LoadedClass c, string description) {
			this.c = c;
			this.description = description;
		}
	}
	
	public class HeapObjectIsOfClass : FilterHeapObjectByClass {
		static string BuildDescription (LoadedClass c) {
			return String.Format ("Object has class {0}", c.Name);
		}
		
		public override bool Filter (HeapObject heapObject) {
			return heapObject.Class == c;
		}
		
		public HeapObjectIsOfClass (LoadedClass c) : base (c, BuildDescription (c)) {
		}
	}
	
	public class HeapObjectReferencesObjectOfClass : FilterHeapObjectByClass {
		static string BuildDescription (LoadedClass c) {
			return String.Format ("Object references object of class {0}", c.Name);
		}
		
		public override bool Filter (HeapObject heapObject) {
			foreach (HeapObject ho in heapObject.References) {
				if (ho.Class == c) {
					return true;
				}
			}
			return false;
		}
		
		public HeapObjectReferencesObjectOfClass (LoadedClass c) : base (c, BuildDescription (c)) {
		}
	}
	
	public class HeapObjectIsReferencedByObjectOfClass : FilterHeapObjectByClass {
		static string BuildDescription (LoadedClass c) {
			return String.Format ("Object is referenced by object of class {0}", c.Name);
		}
		
		public override bool Filter (HeapObject heapObject) {
			foreach (HeapObject ho in heapObject.BackReferences) {
				if (ho.Class == c) {
					return true;
				}
			}
			return false;
		}
		
		public HeapObjectIsReferencedByObjectOfClass (LoadedClass c) : base (c, BuildDescription (c)) {
		}
	}
	
	public abstract class HeapObjectSet {
		public static Comparison<HeapObject> CompareHeapObjectsByID = delegate (HeapObject a, HeapObject b) {
			return a.ID.CompareTo (b.ID);
		};
		
		public class HeapObjectSetClassStatistics {
			LoadedClass c;
			public LoadedClass Class {
				get {
					return c;
				}
			}
			uint allocatedBytes;
			public uint AllocatedBytes {
				get {
					return allocatedBytes;
				}
				internal set {
					allocatedBytes = value;
				}
			}
			public HeapObjectSetClassStatistics (LoadedClass c, uint allocatedBytes) {
				this.c = c;
				this.allocatedBytes = allocatedBytes;
			}
			
			public static Comparison<HeapObjectSetClassStatistics> CompareByAllocatedBytes = delegate (HeapObjectSetClassStatistics a, HeapObjectSetClassStatistics b) {
				return a.AllocatedBytes.CompareTo (b.AllocatedBytes);
			};
		}
		
		string shortDescription;
		public string ShortDescription {
			get {
				return shortDescription;
			}
		}
		string longDescription;
		public string LongDescription {
			get {
				return longDescription;
			}
		}
		HeapObject[] heapObjects;
		public HeapObject[] HeapObjects {
			get {
				return heapObjects;
			}
		}
		HeapObjectSetClassStatistics[] classStatistics;
		public HeapObjectSetClassStatistics[] ClassStatistics {
			get {
				return classStatistics;
			}
		}
		uint allocatedBytes;
		public uint AllocatedBytes {
			get {
				return allocatedBytes;
			}
		}
		
		protected HeapObjectSet (string shortDescription, string longDescription, HeapObject[] heapObjects) {
			this.shortDescription = shortDescription;
			this.longDescription = longDescription;
			this.heapObjects = heapObjects;
			allocatedBytes = 0;
			
			Array.Sort (this.heapObjects, CompareHeapObjectsByID);
			
			Dictionary<ulong,HeapObjectSetClassStatistics> statistics = new Dictionary<ulong,HeapObjectSetClassStatistics> ();
			foreach (HeapObject ho in heapObjects) {
				HeapObjectSetClassStatistics cs;
				if (statistics.ContainsKey (ho.Class.ID)) {
					cs = statistics [ho.Class.ID];
					cs.AllocatedBytes += ho.Size;
					allocatedBytes += ho.Size;
				} else {
					cs = new HeapObjectSetClassStatistics (ho.Class, ho.Size);
					statistics [ho.Class.ID] = cs;
				}
			}
			classStatistics = new HeapObjectSetClassStatistics [statistics.Values.Count];
			statistics.Values.CopyTo (classStatistics, 0);
			Array.Sort (classStatistics, HeapObjectSetClassStatistics.CompareByAllocatedBytes);
			Array.Reverse (classStatistics);
		}
	}
	
	public class HeapObjectSetFromSnapshot : HeapObjectSet {
		HeapSnapshot heapSnapshot;
		public HeapSnapshot HeapSnapshot {
			get {
				return heapSnapshot;
			}
		}
		
		public HeapObjectSetFromSnapshot (HeapSnapshot heapSnapshot):
			base (String.Format ("Snapshot done at {0}", heapSnapshot.StartTime),
			      String.Format ("Snapshot done at {0}", heapSnapshot.StartTime),
			      heapSnapshot.HeapObjects) {
			this.heapSnapshot = heapSnapshot;
		}
	}
	
	public class HeapObjectSetFromFilter : HeapObjectSet {
		HeapObjectSet baseSet;
		public HeapObjectSet BaseSet {
			get {
				return baseSet;
			}
		}
		
		IHeapObjectFilter filter;
		public IHeapObjectFilter Filter {
			get {
				return filter;
			}
		}
		
		static HeapObject[] filterSet (HeapObjectSet baseSet, IHeapObjectFilter filter) {
			List<HeapObject> newSet = new List<HeapObject> ();
			foreach (HeapObject ho in baseSet.HeapObjects) {
				if (filter.Filter (ho)) {
					newSet.Add (ho);
				}
			}
			HeapObject[] result = new HeapObject [newSet.Count];
			newSet.CopyTo (result);
			return result;
		}
		
		public HeapObjectSetFromFilter (HeapObjectSet baseSet, IHeapObjectFilter filter): base (filter.Description, String.Format ("{0} and {1}", filter.Description, baseSet.LongDescription), filterSet (baseSet, filter)) {
			this.baseSet = baseSet;
			this.filter = filter;
		}
	}
	
	public class HeapObjectSetFromComparison : HeapObjectSet {
		HeapObjectSet baseSet;
		public HeapObjectSet BaseSet {
			get {
				return baseSet;
			}
		}
		
		HeapObjectSet otherSet;
		public HeapObjectSet OtherSet {
			get {
				return otherSet;
			}
		}
		
		static string buildShortDescription (HeapObjectSet otherSet) {
			return String.Format("Object not in {0}", otherSet.ShortDescription);
		}
		
		static string buildLongDescription (HeapObjectSet otherSet) {
			return String.Format("Object not in {0}", otherSet.LongDescription);
		}
		
		public static void PerformComparison (HeapObjectSet firstSet, HeapObjectSet secondSet, out HeapObjectSet onlyInFirstSet, out HeapObjectSet onlyInSecondSet) {
			List<HeapObject> onlyInFirst = new List<HeapObject> ();
			List<HeapObject> onlyInSecond = new List<HeapObject> ();
			
			int firstIndex = 0;
			int secondIndex = 0;
			HeapObject[] firstObjects = firstSet.HeapObjects;
			HeapObject[] secondObjects = secondSet.HeapObjects;
			
			while ((firstIndex < firstObjects.Length) || (secondIndex < secondObjects.Length)) {
				if (firstIndex >= firstObjects.Length) {
					while (secondIndex < secondObjects.Length) {
						onlyInSecond.Add (secondObjects [secondIndex]);
						secondIndex ++;
					}
				} else if (secondIndex >= secondObjects.Length) {
					while (firstIndex < secondObjects.Length) {
						onlyInFirst.Add (firstObjects [firstIndex]);
						firstIndex ++;
					}
				} else {
					HeapObject firstObject = firstObjects [firstIndex];
					HeapObject secondObject = secondObjects [secondIndex];
					if (firstObject.ID < secondObject.ID) {
						onlyInFirst.Add (firstObject);
						firstIndex ++;
					} else if (secondObject.ID < firstObject.ID) {
						onlyInSecond.Add (secondObject);
						secondIndex ++;
					} else {
						firstIndex ++;
						secondIndex ++;
					}
				}
			}
			
			onlyInFirstSet = new HeapObjectSetFromComparison(firstSet, secondSet, onlyInFirst.ToArray ());
			onlyInSecondSet = new HeapObjectSetFromComparison(secondSet, firstSet, onlyInSecond.ToArray ());
		}
		
		HeapObjectSetFromComparison (HeapObjectSet baseSet, HeapObjectSet otherSet, HeapObject[] heapObjects): base (buildShortDescription (otherSet), buildLongDescription (otherSet), heapObjects) {
			this.baseSet = baseSet;
			this.otherSet = otherSet;
		}
	}
	
	public class LoadedElementFactory : ILoadedElementFactory<LoadedClass,LoadedMethod,UnmanagedFunctionFromRegion,UnmanagedFunctionFromID,ExecutableMemoryRegion,HeapObject,HeapSnapshot> {
		bool recordHeapSnapshots = true;
		public bool RecordHeapSnapshots {
			get {
				return recordHeapSnapshots;
			}
			set {
				recordHeapSnapshots = value;
			}
		}
		
		public LoadedClass NewClass (uint id, string name, uint size) {
			return new LoadedClass (id, name, size);
		}
		public LoadedMethod NewMethod (uint id, LoadedClass c, string name) {
			return new LoadedMethod (id, c, name);
		}
		public ExecutableMemoryRegion NewExecutableMemoryRegion (uint id, string fileName, uint fileOffset, ulong startAddress, ulong endAddress) {
			return new ExecutableMemoryRegion (id, fileName, fileOffset, startAddress, endAddress);
		}
		public HeapSnapshot NewHeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, LoadedClass[] initialAllocations, bool recordSnapshots) {
			return new HeapSnapshot (collection, startCounter, startTime, endCounter, endTime, initialAllocations, recordSnapshots);
		}
		public UnmanagedFunctionFromID NewUnmanagedFunction (uint id, string name, ExecutableMemoryRegion region) {
			return new UnmanagedFunctionFromID (id, name, region);
		}
	}
	
	public class AllocationSummary : BaseAllocationSummary<LoadedClass> {
		public AllocationSummary (uint collection, ulong startCounter, DateTime startTime) : base (collection, startCounter, startTime) {
		}
	}
}
