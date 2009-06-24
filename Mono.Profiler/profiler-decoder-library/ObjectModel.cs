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
	public class LoadedClass : BaseLoadedClass, IHeapItemSetStatisticsSubject {
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
		
		string IHeapItemSetStatisticsSubject.Description {
			get {
				return Name;
			}
		}
		
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
		
		public static readonly LoadedClass LoadedClassUnavailable = new LoadedClass (0, LoadedAssembly.Unavailable, "(CLASS UNAVAILABLE)", 0);
		
		public LoadedClass (uint id, LoadedAssembly assembly, string name, uint size): base (id, assembly, name, size) {
			allocatedBytes = 0;
			currentlyAllocatedBytes = 0;
			allocationsPerMethod = null;
		}
	}
	
	public class StackTrace : IHeapItemSetStatisticsSubject {
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
		
		ulong clicks;
		public ulong Clicks {
			get {
				return clicks;
			}
		}
		uint calls;
		public ulong Calls {
			get {
				return calls;
			}
		}
		
		public void ResetCalls () {
			calls = 0;
			clicks = 0;
		}
		public void RegisterCall (ulong clicks) {
			this.clicks += clicks;
			calls ++;
		}
		
		static StackTrace[] EmptyCalledFrames = new StackTrace [0];
		List<StackTrace> calledFrames;
		public StackTrace[] CalledFrames {
			get {
				if (calledFrames != null) {
					StackTrace[] result = calledFrames.ToArray ();
					Array.Sort (result, CompareByClicks);
					Array.Reverse (result);
					return result;
				} else {
					return EmptyCalledFrames;
				}
			}
		}
		void AddCalledFrame (StackTrace calledFrame) {
			if (calledFrames == null) {
				calledFrames = new List<StackTrace> ();
			}
			calledFrames.Add (calledFrame);
		}
		
		public static Comparison<StackTrace> CompareByClicks = delegate (StackTrace a, StackTrace b) {
			return a.Clicks.CompareTo (b.Clicks);
		};
		public static Comparison<StackTrace> CompareByCalls = delegate (StackTrace a, StackTrace b) {
			return a.Calls.CompareTo (b.Calls);
		};
		
		public void Write (TextWriter writer, int depth, string indentationString) {
			writer.Write ("CallStack of id ");
			writer.Write (id);
			writer.Write ('\n');
			
			StackTrace currentFrame = this;
			while (currentFrame != null) {
				StackTrace nextFrame = currentFrame.Caller;
				for (int i = 0; i < depth; i++) {
					writer.Write (indentationString);
				}
				writer.Write (currentFrame.TopMethod.Name);
				if (nextFrame != null) {
					writer.Write ('\n');
				}
				currentFrame = nextFrame;
			}
		}
		
		public string FullDescription {
			get {
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				sb.Append ("CallStack of id ");
				sb.Append (id);
				sb.Append ('\n');
				
				StackTrace currentFrame = this;
				while (currentFrame != null) {
					StackTrace nextFrame = currentFrame.Caller;
					sb.Append ("    ");
					sb.Append (currentFrame.TopMethod.Class.Name);
					sb.Append (".");
					sb.Append (currentFrame.TopMethod.Name);
					if (nextFrame != null) {
						sb.Append ('\n');
					}
					currentFrame = nextFrame;
				}
				
				return sb.ToString ();
			}
		}
		string IHeapItemSetStatisticsSubject.Description {
			get {
				return FullDescription;
			}
		}
		
		public static readonly StackTrace StackTraceUnavailable = new StackTrace (CallStack.StackFrame.StackFrameUnavailable.Method, null, false, 0);
		
		StackTrace (LoadedMethod topMethod, StackTrace caller, bool methodIsBeingJitted, uint id) {
			this.clicks = 0;
			this.calls = 0;
			this.calledFrames = null;
			this.topMethod = topMethod;
			this.caller = caller;
			if (caller != null) {
				caller.AddCalledFrame (this);
			}
			this.methodIsBeingJitted = methodIsBeingJitted;
			this.level = caller != null ? caller.level + 1 : 1;
			this.id = id;
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
		
		internal class Factory {
			List<StackTrace> rootFrames;
			public StackTrace[] RootFrames {
				get {
					StackTrace[] result = rootFrames.ToArray ();
					Array.Sort (result, StackTrace.CompareByClicks);
					Array.Reverse (result);
					return result;
				}
			}
			uint nextFreeId;
			Dictionary<uint,List<StackTrace>> [] tracesByLevel;
			
			public Factory () {
				rootFrames = new List<StackTrace> ();
				// Important: StackTrace.StackTraceUnavailable gets id 0, which must be unique
				nextFreeId = 1;
				tracesByLevel = new Dictionary<uint,List<StackTrace>> [64];
			}
			
			public StackTrace NewStackTrace (CallStack.StackFrame frame) {
				if (frame == null) {
					return null;
				}
				
				if (frame.Level >= (uint) tracesByLevel.Length) {
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
				StackTrace result = new StackTrace (frame.Method, callerTrace, frame.IsBeingJitted, nextFreeId);
				nextFreeId ++;
				traces.Add (result);
				if (callerTrace == null) {
					rootFrames.Add (result);
				}
				return result;
			}
			
			public StackTrace NewStackTrace (CallStack stack) {
				return NewStackTrace (stack.StackTop);
			}
		}
	}
	
	class CallStack {
		public class StackFrame {
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
			
			public static readonly StackFrame StackFrameUnavailable = new StackFrame (LoadedMethod.LoadedMethodForStackTraceUnavailable, 0, false, null);
			
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
		
		void PopMethod (StackTrace.Factory stackTraceFactory, LoadedMethod method, ulong counter, bool isBeingJitted) {
			while (stackTop != null) {
				LoadedMethod topMethod = stackTop.Method;
				bool topMethodIsBeingJitted = stackTop.IsBeingJitted;
				StackFrame callerFrame = stackTop.Caller;
				LoadedMethod callerMethod = (callerFrame != null)? callerFrame.Method : null;
				
				if (! topMethodIsBeingJitted) {
					ulong delta = counter - stackTop.StartCounter;
					
					topMethod.MethodCalled (delta, callerMethod);
					StackTrace trace = stackTraceFactory.NewStackTrace (this);
					if (trace != null) {
						trace.RegisterCall (delta);
					}
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
		internal void MethodExit (StackTrace.Factory stackTraceFactory, LoadedMethod method, ulong counter) {
			PopMethod (stackTraceFactory, method, counter, false);
		}
		internal void TopMethodExit (StackTrace.Factory stackTraceFactory, ulong counter) {
			MethodExit (stackTraceFactory, stackTop.Method, counter);
		}
		
		internal void MethodJitStart (LoadedMethod method, ulong counter) {
			stackTop = StackFrame.FrameFactory (method, counter, true, stackTop);
		}
		internal void MethodJitEnd (StackTrace.Factory stackTraceFactory, LoadedMethod method, ulong counter) {
			PopMethod (stackTraceFactory, method, counter, true);
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
	
	public class StatisticalHitItemTreeNode {
		IStatisticalHitItem item;
		public IStatisticalHitItem Item {
			get {
				return item;
			}
		}
		
		public string Name {
			get {
				return item.Name;
			}
		}
		
		uint hitCount;
		void IncrementHitCount () {
			hitCount ++;
		}
		public uint HitCount {
			get {
				return hitCount;
			}
		}
		
		public static Comparison<StatisticalHitItemTreeNode> CompareByHitCount = delegate (StatisticalHitItemTreeNode a, StatisticalHitItemTreeNode b) {
			int result = b.HitCount.CompareTo (a.HitCount);
			if (result == 0) {
				result = a.Name.CompareTo (b.Name);
			}
			return result;
		};
		
		
		Dictionary<string,StatisticalHitItemTreeNode> children;
		static StatisticalHitItemTreeNode[] emptyChildren = new StatisticalHitItemTreeNode [0];
		public int ChildrenCount {
			get {
				return (children != null) ? children.Count : 0;
			}
		}
		public StatisticalHitItemTreeNode[] Children {
			get {
				if (children != null) {
					StatisticalHitItemTreeNode[] result = new StatisticalHitItemTreeNode [children.Count];
					int resultIndex = 0;
					foreach (StatisticalHitItemTreeNode child in children.Values) {
						result [resultIndex] = child;
						resultIndex ++;
					}
					Array.Sort (result, CompareByHitCount);
					return result;
				} else {
					return emptyChildren;
				}
			}
		}
		
		public StatisticalHitItemTreeNode AddChild (IStatisticalHitItem childItem) {
			if (children == null) {
				children = new Dictionary<string, StatisticalHitItemTreeNode> ();
			}
			StatisticalHitItemTreeNode child;
			if (children.ContainsKey (childItem.Name)) {
				child = children [childItem.Name];
			} else {
				child = new StatisticalHitItemTreeNode (childItem);
				children [childItem.Name] = child;
			}
			child.IncrementHitCount ();
			return child;
		}
		
		public void PrintTree (TextWriter writer, uint fatherHits, int level) {
			for (int i = 0; i < level; i++) {
				writer.Write ("    ");
			}
			writer.WriteLine ("{0,5:F2}% ({1}) {2}", ((((double) HitCount) / fatherHits) * 100), HitCount, Name);
			foreach (StatisticalHitItemTreeNode child in Children) {
				child.PrintTree (writer, HitCount, level + 1);
			}
		}
		
		public StatisticalHitItemTreeNode (IStatisticalHitItem item) {
			this.item = item;
			this.hitCount = 0;
			this.children = null;
		}
	}
	
	public class LoadedMethod : BaseLoadedMethod<LoadedClass>, IStatisticalHitItem, IHeapItemSetStatisticsSubject {
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
		
		string IHeapItemSetStatisticsSubject.Description {
			get {
				return Class.Name + "." + Name;
			}
		}
		
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
		
		public static readonly LoadedMethod LoadedMethodUnavailable = new LoadedMethod (0, LoadedClass.LoadedClassUnavailable, false, "(METHOD UNAVAILABLE)");
		public static readonly LoadedMethod LoadedMethodForStackTraceUnavailable = new LoadedMethod (0, LoadedClass.LoadedClassUnavailable, false, "(CALL STACK UNAVAILABLE)");
		
		public LoadedMethod (uint id, LoadedClass c, bool isWrapper, string name): base (id, c, isWrapper, name) {
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
	
	public class UnmanagedFunctionFromRegion : BaseUnmanagedFunctionFromRegion<UnmanagedFunctionFromRegion>, IStatisticalHitItem {
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
				IExecutableMemoryRegion<UnmanagedFunctionFromRegion> r = Region;
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
	
	public interface IHeapItem : IAllocatedObject<LoadedClass> {
		LoadedMethod AllocatorMethod {get;}
		StackTrace AllocationCallStack {get;}
		bool AllocationHappenedAtJitTime {get;}
	}
	
	public class HeapObject : BaseHeapObject<HeapObject,LoadedClass>, IHeapItem {
		AllocatedObject allocation;
		public AllocatedObject Allocation {
			get {
				return allocation;
			}
			set {
				allocation = value;
				if ((allocation.Class != Class) || (allocation.ID != ID)) {
					throw new Exception (String.Format ("Cannot accept allocation of class {0} and ID {1} for object of class {2} and ID {3}", allocation.Class, allocation.ID, Class, ID));
				}
			}
		}
		public void FindAllocation (ProviderOfPreviousAllocationsSets previousSetsProvider) {
			foreach (HeapItemSet<AllocatedObject> allocations in previousSetsProvider.PreviousAllocationsSets ()) {
				allocation = allocations [ID];
				if (allocation != null) {
					return;
				}
			}
		}
		public LoadedMethod AllocatorMethod {
			get {
				return allocation != null ? allocation.AllocatorMethod : null;
			}
		}
		public StackTrace AllocationCallStack {
			get {
				return allocation != null ? allocation.Trace : null;
			}
		}
		public bool AllocationHappenedAtJitTime {
			get {
				return allocation != null ? allocation.AllocationHappenedAtJitTime : false;
			}
		}
		public HeapObject (ulong ID) : base (ID) {}
	}
	
	public class AllocatedObject : IHeapItem {
		ulong id;
		public ulong ID {
			get {
				return id;
			}
		}
		LoadedClass c;
		public LoadedClass Class {
			get {
				return c;
			}
		}
		uint size;
		public uint Size {
			get {
				return size;
			}
		}
		LoadedMethod caller;
		public LoadedMethod Caller {
			get {
				return caller;
			}
		}
		public LoadedMethod AllocatorMethod {
			get {
				return caller;
			}
		}
		bool jitTime;
		public bool JitTime {
			get {
				return jitTime;
			}
		}
		public bool AllocationHappenedAtJitTime {
			get {
				return jitTime;
			}
		}
		StackTrace trace;
		public StackTrace Trace {
			get {
				return trace;
			}
		}
		public StackTrace AllocationCallStack {
			get {
				return trace;
			}
		}
		
		public AllocatedObject (ulong id, LoadedClass c, uint size, LoadedMethod caller, bool jitTime, StackTrace trace) {
			this.id = id;
			this.c = c;
			this.size = size;
			this.caller = caller;
			this.jitTime = jitTime;
			this.trace = trace;
		}
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
		
		public HeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, TimeSpan headerStartTime, LoadedClass[] initialAllocations, bool recordSnapshot) : base (delegate (ulong ID) {return new HeapObject (ID);}, collection, startCounter, startTime, endCounter, endTime, headerStartTime, recordSnapshot) {
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
	
	public interface IHeapItemFilter<HI> where HI : IHeapItem {
		string Description {
			get;
		}
		bool Filter (HI heapItem);
	}
	public interface IAllocatedObjectFilter : IHeapItemFilter<AllocatedObject> {
	}
	public interface IHeapObjectFilter : IHeapItemFilter<HeapObject> {
	}
	
	public abstract class FilterHeapItemByClass<HI> : IHeapItemFilter<HI> where HI : IHeapItem {
		protected LoadedClass c;
		public LoadedClass Class {
			get {
				return c;
			}
		}
		
		public abstract bool Filter (HI heapItem);
		
		string description;
		public string Description {
			get {
				return description;
			}
		}
		
		public FilterHeapItemByClass (LoadedClass c, string description) {
			this.c = c;
			this.description = description;
		}
	}

	public abstract class FilterHeapItemByAllocatorMethod<HI> : IHeapItemFilter<HI> where HI : IHeapItem {
		protected LoadedMethod allocatorMethod;
		public LoadedMethod AllocatorMethod {
			get {
				return allocatorMethod;
			}
		}
		
		public abstract bool Filter (HI heapItem);
		
		string description;
		public string Description {
			get {
				return description;
			}
		}
		
		protected FilterHeapItemByAllocatorMethod (LoadedMethod allocatorMethod, string description) {
			this.allocatorMethod = allocatorMethod;
			this.description = description;
		}
	}
	
	public class HeapItemWasAllocatedByMethod<HI> : FilterHeapItemByAllocatorMethod<HI> where HI : IHeapItem {
		public override bool Filter (HI heapItem) {
			return heapItem.AllocatorMethod == AllocatorMethod;
		}
		
		public HeapItemWasAllocatedByMethod (LoadedMethod allocatorMethod) : base (allocatorMethod, String.Format ("Object was allocated by {0}", allocatorMethod.Name)) {
		}
	}
	
	public class FilterHeapItemByAllocationCallStack<HI> : IHeapItemFilter<HI> where HI : IHeapItem {
		protected StackTrace allocationCallStack;
		public StackTrace AllocationCallStack {
			get {
				return allocationCallStack;
			}
		}
		
		public bool Filter (HI heapItem) {
			return heapItem.AllocationCallStack == allocationCallStack;
		}
		
		string description;
		public string Description {
			get {
				return description;
			}
		}
		
		public FilterHeapItemByAllocationCallStack (StackTrace allocationCallStack) {
			this.allocationCallStack = allocationCallStack;
			this.description = String.Format ("Allocation has call stack:\n{0}", allocationCallStack.FullDescription);
		}
	}
	
	public class HeapItemIsOfClass<HI> : FilterHeapItemByClass<HI> where HI : IHeapItem {
		protected static string BuildDescription (LoadedClass c) {
			return String.Format ("Object has class {0}", c.Name);
		}
		
		public override bool Filter (HI heapItem) {
			return heapItem.Class == c;
		}
		
		public HeapItemIsOfClass (LoadedClass c) : base (c, BuildDescription (c)) {
		}
	}
	
	public class HeapObjectIsOfClass : HeapItemIsOfClass<HeapObject>, IHeapObjectFilter {
		public HeapObjectIsOfClass (LoadedClass c) : base (c) {
		}
	}
	
	public class AllocatedObjectIsOfClass : HeapItemIsOfClass<AllocatedObject>, IAllocatedObjectFilter {
		public AllocatedObjectIsOfClass (LoadedClass c) : base (c) {
		}
	}
	
	public abstract class FilterHeapObjectByClass : FilterHeapItemByClass<HeapObject>, IHeapObjectFilter {
		protected FilterHeapObjectByClass (LoadedClass c, string description) : base (c, description) {
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
	
	public interface IHeapItemSetStatisticsSubject {
		string Description {get;}
		uint ID {get;}
	}
	public delegate HISSS GetHeapItemStatisticsSubject<HI,HISSS> (HI item) where HI : IHeapItem where HISSS : IHeapItemSetStatisticsSubject;
	public delegate HISSBS NewHeapItemStatisticsBySubject<HISSBS,HISSS> (HISSS subject) where HISSS : IHeapItemSetStatisticsSubject where HISSBS : HeapItemSetStatisticsBySubject<HISSS>;
	
	public interface IHeapItemSetStatisticsBySubject {
		IHeapItemSetStatisticsSubject Subject {get;}
		uint ItemsCount {get;}
		uint AllocatedBytes {get;}
	}
	
	public abstract class HeapItemSetStatisticsBySubject<HISSS> : IHeapItemSetStatisticsBySubject where HISSS : IHeapItemSetStatisticsSubject {
		HISSS subject;
		protected HISSS Subject {
			get {
				return subject;
			}
		}
		IHeapItemSetStatisticsSubject IHeapItemSetStatisticsBySubject.Subject {
			get {
				return subject;
			}
		}
		
		uint itemsCount;
		public uint ItemsCount {
			get {
				return itemsCount;
			}
		}
		
		uint allocatedBytes;
		public uint AllocatedBytes {
			get {
				return allocatedBytes;
			}
		}
		
		internal void AddItem (IHeapItem item) {
			itemsCount ++;
			allocatedBytes += item.Size;
		}
		
		protected abstract HISSS GetUnavailableSubject ();
		
		public HeapItemSetStatisticsBySubject (HISSS subject) {
			this.subject = subject != null ? subject : GetUnavailableSubject ();
			this.itemsCount = 0;
			this.allocatedBytes = 0;
		}
		
		public static Comparison<HeapItemSetStatisticsBySubject<HISSS>> CompareByAllocatedBytes = delegate (HeapItemSetStatisticsBySubject<HISSS> a, HeapItemSetStatisticsBySubject<HISSS> b) {
			return a.AllocatedBytes.CompareTo (b.AllocatedBytes);
		};
	}
	
	public class HeapItemSetClassStatistics : HeapItemSetStatisticsBySubject<LoadedClass> {
		public LoadedClass Class {
			get {
				return Subject;
			}
		}
		protected override LoadedClass GetUnavailableSubject () {
			return LoadedClass.LoadedClassUnavailable;
		}
		public HeapItemSetClassStatistics (LoadedClass c) : base (c) {
		}
	}
	
	public class HeapItemSetMethodStatistics : HeapItemSetStatisticsBySubject<LoadedMethod> {
		public LoadedMethod Method {
			get {
				return Subject;
			}
		}
		protected override LoadedMethod GetUnavailableSubject () {
			return LoadedMethod.LoadedMethodUnavailable;
		}
		public HeapItemSetMethodStatistics (LoadedMethod method) : base (method) {
		}
	}
	
	public class HeapItemSetCallStackStatistics : HeapItemSetStatisticsBySubject<StackTrace> {
		public StackTrace CallStack {
			get {
				return Subject;
			}
		}
		protected override StackTrace GetUnavailableSubject () {
			return StackTrace.StackTraceUnavailable;
		}
		public HeapItemSetCallStackStatistics (StackTrace callStack) : base (callStack) {
		}
	}
	
	public interface IHeapItemSet {
		bool ContainsItem (ulong id);
		string ShortDescription {get;}
		string LongDescription {get;}
		IHeapItem[] Elements {get;}
		HeapItemSetClassStatistics[] ClassStatistics {get;}
		HeapItemSetMethodStatistics[] AllocatorMethodStatistics {get;}
		HeapItemSetCallStackStatistics[] AllocationCallStackStatistics {get;}
		uint AllocatedBytes {get;}
		bool ObjectAllocationsArePresent {get;}
		void FindObjectAllocations (ProviderOfPreviousAllocationsSets previousSetsProvider);
	}
	
	public interface ProviderOfPreviousAllocationsSets {
		IEnumerable<HeapItemSet<AllocatedObject>> PreviousAllocationsSets ();
	}
	
	public abstract class HeapItemSet<HI> : IHeapItemSet where HI : IHeapItem {
		public static Comparison<HI> CompareHeapItemsByID = delegate (HI a, HI b) {
			return a.ID.CompareTo (b.ID);
		};
		
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
		HI[] elements;
		public HI[] Elements {
			get {
				return elements;
			}
		}
		IHeapItem[] IHeapItemSet.Elements {
			get {
				IHeapItem[] result = new IHeapItem [elements.Length];
				Array.Copy (elements, result, elements.Length);
				return result;
			}
		}
		HeapItemSetClassStatistics[] classStatistics;
		public HeapItemSetClassStatistics[] ClassStatistics {
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
		
		protected HISSBS[] BuildStatistics<HISSS,HISSBS> (GetHeapItemStatisticsSubject<HI,HISSS> getSubject, NewHeapItemStatisticsBySubject<HISSBS,HISSS> newStatistics) where HISSS : IHeapItemSetStatisticsSubject where HISSBS : HeapItemSetStatisticsBySubject<HISSS> {
			Dictionary<uint,HISSBS> statistics = new Dictionary<uint,HISSBS> ();
			
			foreach (HI hi in elements) {
				HISSS subject = getSubject (hi);
				HISSBS s;
				uint id;
				if (subject != null) {
					id = subject.ID;
				} else {
					id = 0;;
				}
				if (statistics.ContainsKey (id)) {
					s = statistics [id];
				} else {
					s = newStatistics (subject);
					statistics [id] = s;
				}
				s.AddItem (hi);
			}
			HISSBS[] result = new HISSBS [statistics.Values.Count];
			statistics.Values.CopyTo (result, 0);
			Array.Sort (result, HeapItemSetStatisticsBySubject<HISSS>.CompareByAllocatedBytes);
			Array.Reverse (result);
			
			return result;
		}
		
		HeapItemSetMethodStatistics[] allocatorMethodStatistics;
		public HeapItemSetMethodStatistics[] AllocatorMethodStatistics {
			get {
				if ((allocatorMethodStatistics == null) && ObjectAllocationsArePresent) {
					allocatorMethodStatistics = BuildStatistics<LoadedMethod,HeapItemSetMethodStatistics> (delegate (HI item) {
						return item.AllocatorMethod;
					}, delegate (LoadedMethod m) {
						return new HeapItemSetMethodStatistics (m);
					});
				}
				return allocatorMethodStatistics;
			}
		}
		public bool HasAllocatorMethodStatistics {
			get {
				return allocatorMethodStatistics != null;
			}
		}
		
		HeapItemSetCallStackStatistics[] allocationCallStackStatistics;
		public HeapItemSetCallStackStatistics[] AllocationCallStackStatistics {
			get {
				if ((allocationCallStackStatistics == null) && ObjectAllocationsArePresent) {
					allocationCallStackStatistics = BuildStatistics<StackTrace,HeapItemSetCallStackStatistics> (delegate (HI item) {
						return item.AllocationCallStack;
					}, delegate (StackTrace s) {
						return new HeapItemSetCallStackStatistics (s);
					});
				}
				return allocationCallStackStatistics;
			}
		}
		public bool HasAllocationCallStackStatistics {
			get {
				return allocationCallStackStatistics != null;
			}
		}
		
		
		public void CompareWithSet<OHI> (HeapItemSet<OHI> otherSet, out HeapItemSet<HI> onlyInThisSet, out HeapItemSet<OHI> onlyInOtherSet) where OHI : IHeapItem  {
			HeapItemSetFromComparison<HI,OHI>.PerformComparison<HI,OHI> (this, otherSet, out onlyInThisSet, out onlyInOtherSet);
		}
		
		public HeapItemSet<HI> IntersectWithSet<OHI> (HeapItemSet<OHI> otherSet) where OHI : IHeapItem  {
			return HeapItemSetFromComparison<HI,OHI>.PerformIntersection<HI,OHI> (this, otherSet);
		}
		
		public HI this [ulong id] {
			get {
				int lowIndex = -1;
				int highIndex = elements.Length;
				
				while (true) {
					int span = (highIndex - lowIndex) / 2;
					
					if (span > 0) {
						int middleIndex = lowIndex + span;
						HI middleElement = elements [middleIndex];
						ulong middleID = middleElement.ID;
						if (middleID > id) {
							highIndex = middleIndex;
						} else if (middleID < id) {
							lowIndex = middleIndex;
						} else {
							return middleElement;
						}
					} else {
						return default (HI);
					}
				}
			}
		}
		public HI this [HI item] {
			get {
				return this [item.ID];
			}
		}
		
		public bool ContainsItem (ulong id) {
			return this [id] != null;
		}
		
		public HeapItemSet<HeapObject> ObjectsReferencingItemInSet (HeapItemSet<HeapObject> objectSet) {
			return Mono.Profiler.HeapItemSetFromComparison<HI,HeapObject>.ObjectsReferencingItemInSet (this, objectSet);
		}
		public HeapItemSet<HeapObject> ObjectsReferencedByItemInSet (HeapItemSet<HeapObject> objectSet) {
			return Mono.Profiler.HeapItemSetFromComparison<HI,HeapObject>.ObjectsReferencedByItemInSet (this, objectSet);
		}
		
		static void FindObjectAllocations (HeapItemSet<HeapObject> baseSet, ProviderOfPreviousAllocationsSets previousSetsProvider) {
			foreach (HeapObject heapObject in baseSet.Elements) {
				if (heapObject.Allocation == null) {
					heapObject.FindAllocation (previousSetsProvider);
				}
			}
		}
		
		bool objectAllocationsArePresent;
		public bool ObjectAllocationsArePresent {
			get {
				return objectAllocationsArePresent;
			}
		}
		public void FindObjectAllocations (ProviderOfPreviousAllocationsSets previousSetsProvider) {
			if ((! objectAllocationsArePresent)) {
				HeapItemSet<HeapObject> baseSet = this as HeapItemSet<HeapObject>;
				if (baseSet != null) {
					FindObjectAllocations (baseSet, previousSetsProvider);
					objectAllocationsArePresent = true;
				}
			}
		}
		
		protected HeapItemSet (string shortDescription, string longDescription, HI[] elements, bool objectAllocationsArePresent) {
			this.shortDescription = shortDescription;
			this.longDescription = longDescription;
			this.elements = elements;
			this.objectAllocationsArePresent = objectAllocationsArePresent;
			allocatedBytes = 0;
			
			Array.Sort (this.elements, CompareHeapItemsByID);
			
			classStatistics = BuildStatistics<LoadedClass,HeapItemSetClassStatistics> (delegate (HI item) {
				allocatedBytes += item.Size;
				return item.Class;
			}, delegate (LoadedClass c) {
				return new HeapItemSetClassStatistics (c);
			});
		}
	}
	
	public class HeapObjectSetFromSnapshot : HeapItemSet<HeapObject> {
		HeapSnapshot heapSnapshot;
		public HeapSnapshot HeapSnapshot {
			get {
				return heapSnapshot;
			}
		}
		
		public HeapObjectSetFromSnapshot (HeapSnapshot heapSnapshot):
			base (String.Format ("Heap at {0}.{1:000}s", heapSnapshot.HeaderStartTime.Seconds, heapSnapshot.HeaderStartTime.Milliseconds),
			      String.Format ("Heap snapshot taken at {0}.{1:000}s", heapSnapshot.HeaderStartTime.Seconds, heapSnapshot.HeaderStartTime.Milliseconds),
			      heapSnapshot.HeapObjects, false) {
			this.heapSnapshot = heapSnapshot;
		}
	}
	
	public class AllocatedObjectSetFromEvents : HeapItemSet<AllocatedObject> {
		public AllocatedObjectSetFromEvents (TimeSpan timeFromStart, AllocatedObject[] allocations):
			base (String.Format ("Allocations {0}.{1:000}s", timeFromStart.Seconds, timeFromStart.Milliseconds),
			      String.Format ("Allocations taken from {0}.{1:000}s", timeFromStart.Seconds, timeFromStart.Milliseconds),
			      allocations, true) {
		}
	}
	
	public class HeapItemSetFromFilter<HI> : HeapItemSet<HI> where HI : IHeapItem {
		HeapItemSet<HI> baseSet;
		public HeapItemSet<HI> BaseSet {
			get {
				return baseSet;
			}
		}
		
		IHeapItemFilter<HI> filter;
		public IHeapItemFilter<HI> Filter {
			get {
				return filter;
			}
		}
		
		static HI[] filterSet (HeapItemSet<HI> baseSet, IHeapItemFilter<HI> filter) {
			List<HI> newSet = new List<HI> ();
			foreach (HI hi in baseSet.Elements) {
				if (filter.Filter (hi)) {
					newSet.Add (hi);
				}
			}
			HI[] result = new HI [newSet.Count];
			newSet.CopyTo (result);
			return result;
		}
		
		public HeapItemSetFromFilter (HeapItemSet<HI> baseSet, IHeapItemFilter<HI> filter): base (filter.Description, String.Format ("{0} and {1}", filter.Description, baseSet.LongDescription), filterSet (baseSet, filter), baseSet.ObjectAllocationsArePresent) {
			this.baseSet = baseSet;
			this.filter = filter;
		}
	}
	
	public class HeapItemSetFromComparison<HI,OHI> : HeapItemSet<HI> where HI : IHeapItem where OHI : IHeapItem {
		HeapItemSet<HI> baseSet;
		public HeapItemSet<HI> BaseSet {
			get {
				return baseSet;
			}
		}
		
		HeapItemSet<OHI> otherSet;
		public HeapItemSet<OHI> OtherSet {
			get {
				return otherSet;
			}
		}
		
		static string buildShortDescription (HeapItemSet<OHI> otherSet, string relation) {
			return String.Format("Object {0} in {1}", relation, otherSet.ShortDescription);
		}
		
		static string buildLongDescription (HeapItemSet<OHI> otherSet, string relation) {
			return String.Format("Object {0} in {1}", relation, otherSet.LongDescription);
		}
		
		public static void PerformComparison<HI1,HI2> (HeapItemSet<HI1> firstSet, HeapItemSet<HI2> secondSet, out HeapItemSet<HI1> onlyInFirstSet, out HeapItemSet<HI2> onlyInSecondSet) where HI1 : IHeapItem where HI2 : IHeapItem {
			List<HI1> onlyInFirst = new List<HI1> ();
			List<HI2> onlyInSecond = new List<HI2> ();
			
			int firstIndex = 0;
			int secondIndex = 0;
			HI1[] firstObjects = firstSet.Elements;
			HI2[] secondObjects = secondSet.Elements;
			
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
					HI1 firstObject = firstObjects [firstIndex];
					HI2 secondObject = secondObjects [secondIndex];
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
			
			onlyInFirstSet = new HeapItemSetFromComparison<HI1,HI2>(firstSet, secondSet, onlyInFirst.ToArray (), "not");
			onlyInSecondSet = new HeapItemSetFromComparison<HI2,HI1>(secondSet, firstSet, onlyInSecond.ToArray (), "not");
		}
		
		public static HeapItemSet<HI1> PerformIntersection<HI1,HI2> (HeapItemSet<HI1> firstSet, HeapItemSet<HI2> secondSet) where HI1 : IHeapItem where HI2 : IHeapItem {
			List<HI1> result = new List<HI1> ();
			
			int firstIndex = 0;
			int secondIndex = 0;
			HI1[] firstObjects = firstSet.Elements;
			HI2[] secondObjects = secondSet.Elements;
			
			Console.WriteLine ("Inside PerformIntersection...");
			
			while ((firstIndex < firstObjects.Length) && (secondIndex < secondObjects.Length)) {
				HI1 firstObject = firstObjects [firstIndex];
				HI2 secondObject = secondObjects [secondIndex];
				if (firstObject.ID < secondObject.ID) {
					firstIndex ++;
				} else if (secondObject.ID < firstObject.ID) {
					secondIndex ++;
				} else {
					result.Add (firstObject);
					firstIndex ++;
					secondIndex ++;
				}
			}
			
			return new HeapItemSetFromComparison<HI1,HI2>(firstSet, secondSet, result.ToArray (), "also");
		}
		
		static bool ObjectReferencesItemInSet (HeapItemSet<HI> itemSet, HeapObject o) {
			foreach (HeapObject reference in o.References) {
				if (itemSet.ContainsItem (reference.ID)) {
					return true;
				}
			}
			return false;
		}
		public static HeapItemSet<HeapObject> ObjectsReferencingItemInSet (HeapItemSet<HI> itemSet, HeapItemSet<HeapObject> objectSet) {
			List<HeapObject> result = new List<HeapObject> ();
			HeapObject[] objects = objectSet.Elements;
			
			foreach (HeapObject o in objects) {
				if (ObjectReferencesItemInSet (itemSet, o)) {
					result.Add (o);
				}
			}
			
			return new HeapItemSetFromComparison<HeapObject,HI>(objectSet, itemSet, result.ToArray (), "references item");
		}
		
		static bool ObjectIsReferencedByItemInSet (HeapItemSet<HI> itemSet, HeapObject o) {
			foreach (HeapObject reference in o.BackReferences) {
				if (itemSet.ContainsItem (reference.ID)) {
					return true;
				}
			}
			return false;
		}
		public static HeapItemSet<HeapObject> ObjectsReferencedByItemInSet (HeapItemSet<HI> itemSet, HeapItemSet<HeapObject> objectSet) {
			List<HeapObject> result = new List<HeapObject> ();
			HeapObject[] objects = objectSet.Elements;
			
			foreach (HeapObject o in objects) {
				if (ObjectIsReferencedByItemInSet (itemSet, o)) {
					result.Add (o);
				}
			}
			
			return new HeapItemSetFromComparison<HeapObject,HI>(objectSet, itemSet, result.ToArray (), "is referenced by item");
		}
		
		HeapItemSetFromComparison (HeapItemSet<HI> baseSet, HeapItemSet<OHI> otherSet, HI[] heapItems, string relation): base (buildShortDescription (otherSet, relation), buildLongDescription (otherSet, relation), heapItems, baseSet.ObjectAllocationsArePresent) {
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
		
		public LoadedAssembly NewAssembly (uint id, string name, string baseName, uint major, uint minor, uint build, uint revision, string culture, string publicKeyToken, bool retargetable) {
			return new LoadedAssembly (id, name, baseName, major, minor, build, revision, culture, publicKeyToken, retargetable);
		}
		public LoadedClass NewClass (uint id, LoadedAssembly assembly,  string name, uint size) {
			return new LoadedClass (id, assembly, name, size);
		}
		public LoadedMethod NewMethod (uint id, LoadedClass c, bool isWrapper, string name) {
			return new LoadedMethod (id, c, isWrapper, name);
		}
		public ExecutableMemoryRegion NewExecutableMemoryRegion (uint id, string fileName, uint fileOffset, ulong startAddress, ulong endAddress) {
			return new ExecutableMemoryRegion (id, fileName, fileOffset, startAddress, endAddress);
		}
		public HeapSnapshot NewHeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, TimeSpan headerStartTime, LoadedClass[] initialAllocations, bool recordSnapshots) {
			return new HeapSnapshot (collection, startCounter, startTime, endCounter, endTime, headerStartTime, initialAllocations, recordSnapshots);
		}
		public UnmanagedFunctionFromID NewUnmanagedFunction (uint id, string name, ExecutableMemoryRegion region) {
			return new UnmanagedFunctionFromID (id, name, region);
		}
	}
	
	public class AllocationSummary : BaseAllocationSummary<LoadedClass> {
		public AllocationSummary (uint collection, ulong startCounter, DateTime startTime) : base (collection, startCounter, startTime) {
		}
	}
	
	public interface MonitorAggregatedDataHandler {
		ulong TicksContended {get;}
		void AddTicksContended (ulong increment);
		ulong ContentionCount {get;}
		void IncrementContentionCount ();
		ulong FailCount {get;}
		void IncrementFailCount ();
		void Reset ();
		
		void WriteName (TextWriter writer, int depth);
		void WriteIndividualStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth);
		void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth);
	}
	
	public interface MonitorContainerDataHandler : MonitorAggregatedDataHandler {
		MonitorAggregatedDataHandler[] ComponentData {get;}
	}
	
	public struct MonitorAggregatedData {
		ulong ticksContended;
		public ulong TicksContended {
			get {
				return ticksContended;
			}
		}
		public void AddTicksContended (ulong increment) {
			ticksContended += increment;
		}
		
		uint contentionCount;
		public ulong ContentionCount {
			get {
				return contentionCount;
			}
		}
		public void IncrementContentionCount () {
			contentionCount ++;
		}
		
		uint failCount;
		public ulong FailCount {
			get {
				return failCount;
			}
		}
		public void IncrementFailCount () {
			failCount ++;
		}
		
		public void Reset () {
			ticksContended = 0;
			contentionCount = 0;
			failCount = 0;
		}
	}
	
	public abstract class BaseMonitorStatisticsCollector : MonitorAggregatedDataHandler {
		protected MonitorAggregatedData data;
		
		public ulong TicksContended {
			get {
				return data.TicksContended;
			}
		}
		public abstract void AddTicksContended (ulong increment);
		public ulong ContentionCount {
			get {
				return data.ContentionCount;
			}
		}
		public abstract void IncrementContentionCount ();
		public ulong FailCount {
			get {
				return data.FailCount;
			}
		}
		public abstract void IncrementFailCount ();
		public abstract void Reset ();
		
		public abstract void WriteName (TextWriter writer, int depth);
		
		protected static readonly string IndentationString = "        ";
		protected void WriteIndentation (TextWriter writer, int depth) {
			for (int i = 0; i < depth; i++) {
				writer.Write (IndentationString);
			}
		}
		public void WriteIndividualStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			if (owner == null) {
				owner = this;
			}
			
			WriteIndentation (writer, depth);
			
			//double secondsContended = processor.ClicksToSeconds (TicksContended);
			//double ownerSecondsContended = processor.ClicksToSeconds (owner.TicksContended);
			writer.Write ("{0,5:F2}% ({1:F6} ticks) contention time ({2} contentions, {3} failures) in ",
			              //(secondsContended / ownerSecondsContended) * 100.0,
			              (((double) TicksContended) / owner.TicksContended) * 100.0,
			              //secondsContended,
			              TicksContended,
			              ContentionCount,
			              FailCount);
			WriteName (writer, depth);
			writer.WriteLine ();
		}
		public abstract void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth);
		
		protected static void WriteComponentStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, MonitorAggregatedDataHandler[] components, ProfilerEventHandler processor, int depth) {
			foreach (MonitorAggregatedDataHandler component in components) {
				component.WriteFullStatistics (writer, owner, processor, depth + 1);
			}
		}
		protected static void WriteSimpleContainerComponentStatistics (TextWriter writer, MonitorContainerDataHandler container, ProfilerEventHandler processor, int depth) {
			WriteComponentStatistics (writer, container, container.ComponentData, processor, depth);
		}
		protected static void WriteSimpleContainerFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, MonitorContainerDataHandler container, ProfilerEventHandler processor, int depth) {
			container.WriteIndividualStatistics (writer, owner, processor, depth);
			WriteComponentStatistics (writer, container, container.ComponentData, processor, depth);
		}
		
		public static Comparison<MonitorAggregatedDataHandler> CompareStatistics = delegate (MonitorAggregatedDataHandler a, MonitorAggregatedDataHandler b) {
			int result = b.TicksContended.CompareTo (a.TicksContended);
			if (result != 0) {
				return result;
			} else {
				return b.ContentionCount.CompareTo (a.ContentionCount);
			}
		};
	}
	
	public abstract class SimpleMonitorStatistics : BaseMonitorStatisticsCollector {
		public override void AddTicksContended (ulong increment) {
			data.AddTicksContended (increment);
		}
		public override void IncrementContentionCount () {
			data.IncrementContentionCount ();
		}
		public override void IncrementFailCount () {
			data.IncrementFailCount ();
		}
		public override void Reset () {
			data.Reset ();
		}
	}
	
	public class GlobalMonitorStatistics : SimpleMonitorStatistics, MonitorContainerDataHandler {
		Dictionary<uint,MonitorStatisticsByClass> statistics;
		public MonitorStatisticsByClass[] Statistics {
			get {
				MonitorStatisticsByClass[] result = new MonitorStatisticsByClass [statistics.Count];
				statistics.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentData {
			get {
				return Statistics;
			}
		}
		
		public void HandleEvent (ulong threadId, MonitorEvent eventCode, LoadedClass c, ulong objectId, StackTrace trace, ulong counter) {
			MonitorStatisticsByClass target;
			if (! statistics.ContainsKey (c.ID)) {
				target = new MonitorStatisticsByClass (this, c);
				statistics [c.ID] = target;
			} else {
				target = statistics [c.ID];
			}
			
			target.HandleEvent (threadId, eventCode, objectId, trace, counter);
		}
		
		public string Name {
			get {
				return "Global statistics";
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			writer.Write (Name);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteSimpleContainerFullStatistics (writer, owner, this, processor, depth);
		}
		
		public void WriteStatistics (TextWriter writer, ProfilerEventHandler processor) {
			WriteFullStatistics (writer, this, processor, 0);
		}
		
		public bool ContainsData {
			get {
				return statistics.Count > 0;
			}
		}
		
		public GlobalMonitorStatistics () {
			statistics = new Dictionary<uint,MonitorStatisticsByClass> ();
		}
	}
	
	public abstract class DependentMonitorStatistics<MS> : BaseMonitorStatisticsCollector where MS : MonitorAggregatedDataHandler {
		MS owner;
		public MS Owner {
			get {
				return owner;
			}
		}
		
		public override void AddTicksContended (ulong increment) {
			data.AddTicksContended (increment);
			owner.AddTicksContended (increment);
		}
		public override void IncrementContentionCount () {
			data.IncrementContentionCount ();
			owner.IncrementContentionCount ();
		}
		public override void IncrementFailCount () {
			data.IncrementFailCount ();
			owner.IncrementFailCount ();
		}
		public override void Reset () {
			data.Reset ();
			owner.Reset ();
		}
		
		protected DependentMonitorStatistics (MS owner) {
			this.owner = owner;
		}
	}
	
	public class MonitorStatisticsByCallerPerClass : SimpleMonitorStatistics, MonitorContainerDataHandler {
		LoadedMethod caller;
		public LoadedMethod Caller {
			get {
				return caller;
			}
		}
		
		Dictionary<uint,MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass>> statistics;
		public MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass>[] Statistics {
			get {
				MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass>[] result = new MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> [statistics.Count];
				statistics.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentData {
			get {
				return Statistics;
			}
		}
		
		public MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> HandleEvent (MonitorEvent eventCode, StackTrace trace) {
			MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> target;
			if (! statistics.ContainsKey (trace.ID)) {
				target = new MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> (this, trace);
				statistics [trace.ID] = target;
			} else {
				target = statistics [trace.ID];
			}
			
			target.HandleEvent (eventCode);
			
			return target;
		}
		
		public string Name {
			get {
				return caller.Class.Name + "." + caller.Name;
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			writer.Write (Name);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteSimpleContainerFullStatistics (writer, owner, this, processor, depth);
		}
		
		public MonitorStatisticsByCallerPerClass (LoadedMethod caller) {
			this.caller = caller;
			statistics = new Dictionary<uint,MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass>> ();
		}
	}
	
	public class MonitorStatisticsByClass : DependentMonitorStatistics<GlobalMonitorStatistics>, MonitorContainerDataHandler {
		LoadedClass c;
		public LoadedClass Class {
			get {
				return c;
			}
		}
		
		Dictionary<ulong,MonitorStatistics> statistics;
		public MonitorStatistics[] Statistics {
			get {
				MonitorStatistics[] result = new MonitorStatistics [statistics.Count];
				statistics.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentData {
			get {
				return Statistics;
			}
		}
		
		Dictionary<uint,MonitorStatisticsByCallerPerClass> statisticsByCaller;
		public MonitorStatisticsByCallerPerClass[] StatisticsByCaller {
			get {
				MonitorStatisticsByCallerPerClass[] result = new MonitorStatisticsByCallerPerClass [statisticsByCaller.Count];
				statisticsByCaller.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentDataByCaller {
			get {
				return StatisticsByCaller;
			}
		}
		
		public void HandleEvent (ulong threadId, MonitorEvent eventCode, ulong objectId, StackTrace trace, ulong counter) {
			MonitorStatisticsByCallerPerClass callerPerClassTarget;
			if (statisticsByCaller.ContainsKey (trace.TopMethod.ID)) {
				callerPerClassTarget = statisticsByCaller [trace.TopMethod.ID];
			} else {
				callerPerClassTarget = new MonitorStatisticsByCallerPerClass (trace.TopMethod);
				statisticsByCaller [trace.TopMethod.ID] = callerPerClassTarget;
			}
			MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> tracePerClassDestination = callerPerClassTarget.HandleEvent (eventCode, trace);
			
			MonitorStatistics target;
			if (statistics.ContainsKey (objectId)) {
				target = statistics [objectId];
			} else {
				target = new MonitorStatistics (this, objectId);
				statistics [objectId] = target;
			}
			
			target.HandleEvent (threadId, eventCode, trace, counter, tracePerClassDestination);
		}
		
		public string Name {
			get {
				return c.Name;
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			writer.Write (Name);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteIndividualStatistics (writer, owner, processor, depth);
			
			WriteIndentation (writer, depth + 1);
			writer.WriteLine ("Statistics by caller:");
			WriteComponentStatistics (writer, this, ComponentDataByCaller, processor, depth + 1);
			
			WriteIndentation (writer, depth + 1);
			writer.WriteLine ("Statistics by monitor:");
			WriteComponentStatistics (writer, this, ComponentData, processor, depth + 1);
		}
		
		public MonitorStatisticsByClass (GlobalMonitorStatistics gms, LoadedClass c) : base (gms) {
			this.c = c;
			statistics = new Dictionary<ulong,MonitorStatistics> ();
			statisticsByCaller = new Dictionary<uint,MonitorStatisticsByCallerPerClass> ();
		}
	}
	
	public class MonitorStatistics : DependentMonitorStatistics<MonitorStatisticsByClass>, MonitorContainerDataHandler {
		struct ThreadState {
			public ulong lastCounterValue;
			//public bool isPerformingRetry;
		}
		
		ThreadState currentThreadState;
		ulong currentThreadId;
		Dictionary<ulong,ThreadState> threadStates;
		
		void SetCurrentThread (ulong threadId) {
			if (currentThreadId != threadId) {
				threadStates [currentThreadId] = currentThreadState;
				if (! threadStates.ContainsKey (threadId)) {
					threadStates [threadId] = new ThreadState ();
				}
				currentThreadState = threadStates [threadId];
				currentThreadId = threadId;
			}
		}
		
		public void HandleEvent (ulong threadId, MonitorEvent eventCode, StackTrace trace, ulong counter, MonitorStatisticsByCallStack<MonitorStatisticsByCallerPerClass> tracePerClassDestination) {
			uint methodId = trace.TopMethod.ID;
			MonitorStatisticsByCaller target;
			if (! statistics.ContainsKey (methodId)) {
				target = new MonitorStatisticsByCaller (this, trace.TopMethod);
				statistics [methodId] = target;
			} else {
				target = statistics [methodId];
			}
			
			MonitorStatisticsByCallStack<MonitorStatisticsByCaller> destination = target.HandleEvent (eventCode, trace);
			
			SetCurrentThread (threadId);
			
			switch (eventCode) {
			case MonitorEvent.CONTENTION:
				tracePerClassDestination.IncrementContentionCount ();
				destination.IncrementContentionCount ();
				currentThreadState.lastCounterValue = counter;
				break;
			case MonitorEvent.DONE:
				tracePerClassDestination.AddTicksContended (counter - currentThreadState.lastCounterValue);
				destination.AddTicksContended (counter - currentThreadState.lastCounterValue);
				currentThreadState.lastCounterValue = counter;
				break;
			case MonitorEvent.FAIL:
				tracePerClassDestination.AddTicksContended (counter - currentThreadState.lastCounterValue);
				destination.AddTicksContended (counter - currentThreadState.lastCounterValue);
				currentThreadState.lastCounterValue = counter;
				
				tracePerClassDestination.IncrementFailCount ();
				destination.IncrementFailCount ();
				break;
			default:
				throw new Exception (String.Format ("Invalid MonitorEvent code {0}", eventCode));
			}
		}
		
		ulong objectId;
		public ulong ObjectId {
			get {
				return objectId;
			}
		}
		
		Dictionary<uint,MonitorStatisticsByCaller> statistics;
		public MonitorStatisticsByCaller[] Statistics {
			get {
				MonitorStatisticsByCaller[] result = new MonitorStatisticsByCaller [statistics.Count];
				statistics.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentData {
			get {
				return Statistics;
			}
		}
		
		public string Name {
			get {
				return "object " + ObjectId;
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			writer.Write (Name);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteSimpleContainerFullStatistics (writer, owner, this, processor, depth);
		}
		
		public MonitorStatistics (MonitorStatisticsByClass msbc, ulong objectId) : base (msbc) {
			this.objectId = objectId;
			currentThreadState.lastCounterValue = 0;
			currentThreadId = 0;
			threadStates = new Dictionary<ulong,ThreadState> ();
			threadStates [0] = new ThreadState ();
			statistics = new Dictionary<uint,MonitorStatisticsByCaller> ();
		}
	}
	
	public class MonitorStatisticsByCaller : DependentMonitorStatistics<MonitorStatistics>, MonitorContainerDataHandler {
		LoadedMethod caller;
		public LoadedMethod Caller {
			get {
				return caller;
			}
		}
		
		Dictionary<uint,MonitorStatisticsByCallStack<MonitorStatisticsByCaller>> statistics;
		public MonitorStatisticsByCallStack<MonitorStatisticsByCaller>[] Statistics {
			get {
				MonitorStatisticsByCallStack<MonitorStatisticsByCaller>[] result = new MonitorStatisticsByCallStack<MonitorStatisticsByCaller> [statistics.Count];
				statistics.Values.CopyTo (result, 0);
				Array.Sort (result, CompareStatistics);
				return result;
			}
		}
		public MonitorAggregatedDataHandler[] ComponentData {
			get {
				return Statistics;
			}
		}
		
		public MonitorStatisticsByCallStack<MonitorStatisticsByCaller> HandleEvent (MonitorEvent eventCode, StackTrace trace) {
			MonitorStatisticsByCallStack<MonitorStatisticsByCaller> target;
			if (! statistics.ContainsKey (trace.ID)) {
				target = new MonitorStatisticsByCallStack<MonitorStatisticsByCaller> (this, trace);
				statistics [trace.ID] = target;
			} else {
				target = statistics [trace.ID];
			}
			
			target.HandleEvent (eventCode);
			
			return target;
		}
		
		public string Name {
			get {
				return caller.Class.Name + "." + caller.Name;
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			writer.Write (Name);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteSimpleContainerFullStatistics (writer, owner, this, processor, depth);
		}
		
		public MonitorStatisticsByCaller (MonitorStatistics ms, LoadedMethod caller) : base (ms) {
			this.caller = caller;
			statistics = new Dictionary<uint,MonitorStatisticsByCallStack<MonitorStatisticsByCaller>> ();
		}
	}
	
	public class MonitorStatisticsByCallStack<C> : DependentMonitorStatistics<C> where C : MonitorContainerDataHandler {
		StackTrace trace;
		public StackTrace Trace {
			get {
				return trace;
			}
		}
		
		public void HandleEvent (MonitorEvent eventCode) {
		}
		
		public string Name {
			get {
				return trace.FullDescription;
			}
		}
		
		public override void WriteName (TextWriter writer, int depth) {
			trace.Write (writer, depth + 2, IndentationString);
		}
		
		public override void WriteFullStatistics (TextWriter writer, MonitorAggregatedDataHandler owner, ProfilerEventHandler processor, int depth) {
			WriteIndividualStatistics (writer, owner, processor, depth);
		}
		
		public MonitorStatisticsByCallStack (C owner, StackTrace trace) : base (owner) {
			this.trace = trace;
		}
	}
}
