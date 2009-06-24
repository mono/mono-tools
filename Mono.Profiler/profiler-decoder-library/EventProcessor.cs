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
	class UnknownStatisticalHitsCollector : IStatisticalHitItem {
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
		
		public string Name {
			get {
				return "[UNKNOWN MEMORY REGION]";
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
		
		public UnknownStatisticalHitsCollector () {
			statisticalHits = 0;
		}
	}
	
	public class ProfilerEventHandler : BaseProfilerEventHandler<LoadedClass,LoadedMethod,UnmanagedFunctionFromRegion,UnmanagedFunctionFromID,ExecutableMemoryRegion,LoadedElementHandler<LoadedClass,LoadedMethod,UnmanagedFunctionFromRegion,UnmanagedFunctionFromID,ExecutableMemoryRegion,HeapObject,HeapSnapshot>,HeapObject,HeapSnapshot> {
		Dictionary<ulong,CallStack> perThreadStacks;
		CallStack stack;
		UnknownStatisticalHitsCollector unknownStatisticalHitsCollector;
		GlobalMonitorStatistics globalMonitorStatistics;
		
		public GlobalMonitorStatistics GlobalMonitorStatistics {
			get {
				return globalMonitorStatistics;
			}
		}
		
		StackTrace.Factory stackTraceFactory;
		public StackTrace[] RootFrames {
			get {
				return stackTraceFactory.RootFrames;
			}
		}

		uint version;
		public uint Version {
			get {
				return version;
			}
		}
		
		string runtimeFile;
		public string RuntimeFile {
			get {
				return runtimeFile;
			}
		}
		
		ProfilerFlags flags;
		public ProfilerFlags Flags {
			get {
				return flags;
			}
		}
		
		ulong startCounter;
		public ulong StartCounter {
			get {
				return startCounter;
			}
		}
		
		DateTime startTime;
		public DateTime StartTime {
			get {
				return startTime;
			}
		}
		
		ulong endCounter;
		public ulong EndCounter {
			get {
				return endCounter;
			}
		}
		
		DateTime endTime;
		public DateTime EndTime {
			get {
				return endTime;
			}
		}
		
		ulong currentCounter;
		public ulong CurrentCounter {
			get {
				return currentCounter;
			}
		}
		
		DateTime currentTime;
		public DateTime CurrentTime {
			get {
				return currentTime;
			}
		}
		
		double ticksPerCounterUnit;
		public override Double TicksPerCounterUnit {
			get {
				return ticksPerCounterUnit;
			}
		}
		void UpdateTicksPerCounterUnit () {
			if (currentCounter > startCounter) {
				ulong counterSpan = currentCounter - startCounter;
				TimeSpan timeSpan = currentTime - startTime;
				ticksPerCounterUnit = ((double)timeSpan.Ticks) / ((double)counterSpan);
			}
		}
		void UpdateCounterAndTime (ulong currentCounter, DateTime currentTime) {
			this.currentCounter = currentCounter;
			this.currentTime = currentTime;
			UpdateTicksPerCounterUnit ();
		}
		public DateTime CounterToDateTime (ulong counter) {
			return StartTime + TimeSpan.FromTicks ((long) (ticksPerCounterUnit * (double)(counter - StartCounter)));
		}
		public override TimeSpan ClicksToTimeSpan (ulong clicks) {
			return TimeSpan.FromTicks ((long) (ticksPerCounterUnit * (double)clicks));
		}
		public double ClicksToSeconds (ulong clicks) {
			return (ticksPerCounterUnit * (double)clicks) / TimeSpan.TicksPerSecond;
		}
		
		List<AllocatedObject> allocatedObjects = null;
		public bool RecordAllocations {
			get {
				return allocatedObjects != null;
			}
			set {
				if (value) {
					if (allocatedObjects != null) {
						allocatedObjects.Clear ();
					} else {
						allocatedObjects = new List<AllocatedObject> ();
					}
				} else {
					allocatedObjects = null;
				}
			}
		}
		public AllocatedObject[] AllocatedObjects {
			get {
				if (RecordAllocations) {
					AllocatedObject[] result = allocatedObjects.ToArray ();
					allocatedObjects.Clear ();
					return result;
				} else {
					return null;
				}
			}
		}
		protected void RecordAllocation (LoadedClass c, ulong objectId, uint size, LoadedMethod caller, bool jitTime, StackTrace trace) {
			if (RecordAllocations) {
				allocatedObjects.Add (new AllocatedObject (objectId, c, size, caller, jitTime, trace));
			}
		}
		
		public override void Start (uint version, string runtimeFile, ProfilerFlags flags, ulong startCounter, DateTime startTime) {
			this.version = version;
			this.runtimeFile = runtimeFile;
			this.flags = flags;
			this.startCounter = startCounter;
			this.startTime = startTime;
		}
		
		public override void End (uint version, ulong endCounter, DateTime endTime) {
			if (this.version != version) {
				throw new Exception (String.Format ("Version {0} specified at start is inconsistent witn {1} specified at end", this.version, version));
			}
			this.endCounter = endCounter;
			this.endTime = endTime;
			UpdateCounterAndTime (endCounter, endTime);
		}
		
		public override void StartBlock (ulong startCounter, DateTime startTime, ulong threadId) {
			UpdateCounterAndTime (startCounter, startTime);
		}
		
		public override void EndBlock (ulong endCounter, DateTime endTime, ulong threadId) {
			UpdateCounterAndTime (endCounter, endTime);
		}
		
		public override void ModuleLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public override void ModuleUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		public override void AssemblyLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public override void AssemblyUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		public override void ApplicationDomainLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public override void ApplicationDomainUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		
		public override void SetCurrentThread (ulong threadId) {
			if (perThreadStacks.ContainsKey (threadId)) {
				stack = perThreadStacks [threadId];
			} else {
				stack = new CallStack (threadId);
				perThreadStacks.Add (threadId, stack);
			}
		}
		
		public override void ClassStartLoad (LoadedClass c, ulong counter) {}
		public override void ClassEndLoad (LoadedClass c, ulong counter, bool success) {}
		public override void ClassStartUnload (LoadedClass c, ulong counter) {}
		public override void ClassEndUnload (LoadedClass c, ulong counter) {}
		
		public override void Allocation (LoadedClass c, uint size, LoadedMethod caller, bool jitTime, ulong objectId, ulong counter) {
			StackTrace trace;
			
			if (Directives.AllocationsHaveStackTrace) {
				trace = stackTraceFactory.NewStackTrace (stack);
			} else {
				trace = null;
			}
			
			if (caller == null) {
				if ((stack != null) && (stack.StackTop != null)) {
					caller = stack.StackTop.Method;
					jitTime = stack.StackTop.IsBeingJitted;
				}
			}
			c.InstanceCreated (size, caller, jitTime, trace);
			RecordAllocation (c, objectId, size, caller, jitTime, trace);
		}
		
		public override void MonitorEvent (MonitorEvent eventCode, LoadedClass c, ulong objectId, ulong counter) {
			//Console.WriteLine ("MonitorEvent {0} class {1} object {2} counter {3}", eventCode, c.Name, objectId, counter);
			
			StackTrace trace = stackTraceFactory.NewStackTrace (stack);
			if (trace == null ) {
				trace = StackTrace.StackTraceUnavailable;
			}
			globalMonitorStatistics.HandleEvent (stack.ThreadId, eventCode, c, objectId, trace, counter);
		}
		
		public override void Exception (LoadedClass c, ulong counter) {}
		
		public override void MethodEnter (LoadedMethod m, ulong counter) {
			stack.MethodEnter (m, counter);
		}
		
		public override void MethodExit (LoadedMethod m, ulong counter) {
			stack.MethodExit (stackTraceFactory, m, counter);
		}
		
		public override void MethodJitStart (LoadedMethod m, ulong counter) {
			m.StartJit = counter;
			stack.MethodJitStart (m, counter);
		}
		
		public override void MethodJitEnd (LoadedMethod m, ulong counter, bool success) {
			m.JitClicks += (counter - m.StartJit);
			stack.MethodJitEnd (stackTraceFactory, m, counter);
		}
		
		public override void MethodFreed (LoadedMethod m, ulong counter) {}
		
		public override void AdjustStack (uint lastValidFrame, uint topSectionSize, StackSectionElement<LoadedClass,LoadedMethod>[] topSection) {
			stack.AdjustStack (lastValidFrame, topSectionSize, topSection);
		}
		
		uint remainingCallersInChain;
		IStatisticalHitItem lastCallee;
		StatisticalHitItemTreeNode lastCalleeNode;
		bool eventsArePartOfChain;
		int currentChainIndex;
		IStatisticalHitItem[] currentChain;
		// Returns true if the hit must be counted (this is the first chain item)
		bool HandleCallChain (IStatisticalHitItem caller) {
			if (eventsArePartOfChain) {
				bool result;
				
				if (lastCallee != null) {
					lastCallee.CallCounts.AddCaller (caller);
					caller.CallCounts.AddCallee (lastCallee);
				}
				
				if (lastCalleeNode != null) {
					lastCalleeNode = lastCalleeNode.AddChild (caller);
				} else {
					lastCalleeNode = statisticalItemsByCaller.AddChild (caller);
				}
				
				currentChain [currentChainIndex] = caller;
				currentChainIndex ++;
				
				if (remainingCallersInChain > 0) {
					//Console.WriteLine ("HandleCallChain[{0}]  {1} on {2}", remainingCallersInChain, caller.Name, lastCallee.Name);
					remainingCallersInChain --;
					result = false;
				} else {
					//Console.WriteLine ("HandleCallChain[{0}] {1}", remainingCallersInChain, caller.Name);
					result = true;
					
					StatisticalHitItemTreeNode currentNode = statisticalItemsByCallee;
					while (currentChainIndex > 0) {
						currentChainIndex --;
						currentNode = currentNode.AddChild (currentChain [currentChainIndex]);
					}
					
					eventsArePartOfChain = false;
					Array.Clear (currentChain, 0, currentChain.Length);
				}
				
				lastCallee = caller;
				
				return result;
			} else {
				return true;
			}
		}
		
		public override void StatisticalCallChainStart (uint chainDepth) {
			lastCallee = null;
			lastCalleeNode = null;
			remainingCallersInChain = chainDepth;
			eventsArePartOfChain = true;
			currentChainIndex = 0;
			if (currentChain != null) {
				Array.Clear (currentChain, 0, currentChain.Length);
			} else {
				currentChain = new IStatisticalHitItem [32];
			}
			//Console.WriteLine ("StatisticalCallChainStart ({0})", chainDepth);
		}
		
		public override void MethodStatisticalHit (LoadedMethod m) {
			if (HandleCallChain (m)) {
				m.StatisticalHits ++;
			}
		}
		
		public override void UnmanagedFunctionStatisticalHit (UnmanagedFunctionFromRegion f) {
			if (HandleCallChain (f)) {
				f.StatisticalHits ++;
			}
		}
		public override void UnmanagedFunctionStatisticalHit (UnmanagedFunctionFromID f) {
			if (HandleCallChain (f)) {
				f.StatisticalHits ++;
			}
		}
		public override void UnknownUnmanagedFunctionStatisticalHit (ExecutableMemoryRegion region, uint offset) {
			if (HandleCallChain (region)) {
				region.IncrementStatisticalHits ();
			}
		}
		public override void UnknownUnmanagedFunctionStatisticalHit (ulong address) {
			if (HandleCallChain (unknownStatisticalHitsCollector)) {
				unknownStatisticalHitsCollector.IncrementStatisticalHits ();
			}
		}
		
		public override void ThreadStart (ulong threadId, ulong counter) {}
		public override void ThreadEnd (ulong threadId, ulong counter) {}
		
		public IStatisticalHitItem[] StatisticalHitItems {
			get {
				LoadedMethod[] methods = LoadedElements.Methods;
				ExecutableMemoryRegion [] regions = LoadedElements.ExecutableMemoryRegions;
				UnmanagedFunctionFromRegion[][] regionFunctions = new UnmanagedFunctionFromRegion [regions.Length][];
				UnmanagedFunctionFromID[] idFunctions = LoadedElements.UnmanagedFunctionsByID;
				int resultIndex = 0;
				for (int i = 0; i < regions.Length; i++) {
					ExecutableMemoryRegion region = regions [i];
					regionFunctions [i] = region.Functions;
					resultIndex += regionFunctions [i].Length;
				}
				IStatisticalHitItem[] result = new IStatisticalHitItem [resultIndex + methods.Length + idFunctions.Length + regions.Length + 1];
				
				resultIndex = 0;
				for (int i = 0; i < regions.Length; i++) {
					UnmanagedFunctionFromRegion[] functions = regionFunctions [i];
					Array.ConstrainedCopy (functions, 0, result, resultIndex, functions.Length);
					resultIndex += functions.Length;
				}
				Array.ConstrainedCopy (methods, 0, result, resultIndex, methods.Length);
				resultIndex += methods.Length;
				Array.ConstrainedCopy (idFunctions, 0, result, resultIndex, idFunctions.Length);
				resultIndex += idFunctions.Length;
				Array.ConstrainedCopy (regions, 0, result, resultIndex, regions.Length);
				resultIndex += regions.Length;
				result [resultIndex] = unknownStatisticalHitsCollector;
				
				return result;
			}
		}
		
		StatisticalHitItemTreeNode statisticalItemsByCaller = new StatisticalHitItemTreeNode (null);
		public StatisticalHitItemTreeNode[] StatisticalItemsByCaller {
			get {
				return statisticalItemsByCaller.Children;
			}
		}
		StatisticalHitItemTreeNode statisticalItemsByCallee = new StatisticalHitItemTreeNode (null);
		public StatisticalHitItemTreeNode[] StatisticalItemsByCallee {
			get {
				return statisticalItemsByCallee.Children;
			}
		}
		
		public class GcStatistics {
			ProfilerEventHandler data;
			
			uint collection;
			public ulong Collection {
				get {
					return collection;
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
			ulong endCounter;
			public ulong EndCounter {
				get {
					return endCounter;
				}
				internal set {
					endCounter = value;
				}
			}
			ulong markStartCounter;
			public ulong MarkStartCounter {
				get {
					return markStartCounter;
				}
				internal set {
					markStartCounter = value;
				}
			}
			ulong markEndCounter;
			public ulong MarkEndCounter {
				get {
					return markEndCounter;
				}
				internal set {
					markEndCounter = value;
				}
			}
			ulong sweepStartCounter;
			public ulong SweepStartCounter {
				get {
					return sweepStartCounter;
				}
				internal set {
					sweepStartCounter = value;
				}
			}
			ulong sweepEndCounter;
			public ulong SweepEndCounter {
				get {
					return sweepEndCounter;
				}
				internal set {
					sweepEndCounter = value;
				}
			}
			uint generation;
			public uint Generation {
				get {
					return generation;
				}
				internal set {
					generation = value;
				}
			}
			ulong? newHeapSize;
			public ulong? NewHeapSize {
				get {
					return newHeapSize;
				}
				internal set {
					newHeapSize = value;
				}
			}
			
			public double Start {
				get {
					return data.ClicksToSeconds (startCounter - data.StartCounter);
				}
			}
			public double Duration {
				get {
					return data.ClicksToSeconds (endCounter - startCounter);
				}
			}
			public double MarkDuration {
				get {
					return data.ClicksToSeconds (markEndCounter - markStartCounter);
				}
			}
			public double SweepDuration {
				get {
					return data.ClicksToSeconds (sweepEndCounter - sweepStartCounter);
				}
			}
			
			public GcStatistics (ProfilerEventHandler data, uint collection) {
				this.data = data;
				this.collection = collection;
				startCounter = 0;
				endCounter = 0;
				markStartCounter = 0;
				markEndCounter = 0;
				sweepStartCounter = 0;
				sweepEndCounter = 0;
				generation = 0;
				newHeapSize = null;
			}
		}
		
		List<GcStatistics> gcStatistics;
		static Comparison<GcStatistics> compareGcStatisticsByAllocation = delegate (GcStatistics a, GcStatistics b) {
			return a.Collection.CompareTo (b.Collection);
		};
		public GcStatistics[] GarbageCollectioncStatistics {
			get {
				GcStatistics[] result = gcStatistics.ToArray ();
				Array.Sort (result, compareGcStatisticsByAllocation);
				return result;
			}
		}
		
		GcStatistics currentGcStatistics;
		List<GcStatistics> pendingGcStatistics;
		GcStatistics GetGcStatistics (uint collection) {
			if ((currentGcStatistics != null) && (currentGcStatistics.Collection == collection)) {
				return currentGcStatistics;
			} else {
				foreach (GcStatistics gcs in pendingGcStatistics) {
					if (gcs.Collection == collection) {
						GcStatistics result = gcs;
						if (currentGcStatistics != null) {
							pendingGcStatistics.Add (currentGcStatistics);
						}
						currentGcStatistics = result;
						return result;
					}
				}
				return NewGcStatistics (collection);
			}
		} 
		GcStatistics NewGcStatistics (uint collection) {
			GcStatistics result = new GcStatistics (this, collection);
			if (currentGcStatistics != null) {
				pendingGcStatistics.Add (currentGcStatistics);
			}
			currentGcStatistics = result;
			return result;
		}
		
		public override void GarbageCollectionStart (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = NewGcStatistics (collection);
			gcs.Generation = generation;
			gcs.StartCounter = counter;
		}
		public override void GarbageCollectionEnd (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = GetGcStatistics (collection);
			gcs.EndCounter = counter;
			pendingGcStatistics.Remove (gcs);
			gcStatistics.Add (gcs);
			currentGcStatistics = null;
		}
		public override void GarbageCollectionMarkStart (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = GetGcStatistics (collection);
			gcs.MarkStartCounter = counter;
		}
		public override void GarbageCollectionMarkEnd (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = GetGcStatistics (collection);
			gcs.MarkEndCounter = counter;
		}
		public override void GarbageCollectionSweepStart (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = GetGcStatistics (collection);
			gcs.SweepStartCounter = counter;
		}
		public override void GarbageCollectionSweepEnd (uint collection, uint generation, ulong counter) {
			GcStatistics gcs = GetGcStatistics (collection);
			gcs.SweepEndCounter = counter;
		}
		public override void GarbageCollectionResize (uint collection, ulong newSize) {
			GcStatistics gcs = NewGcStatistics (collection);
			gcs.NewHeapSize = newSize;
			gcStatistics.Add (gcs);
			currentGcStatistics = null;
		}
		
		HeapSnapshot currentHeapSnapshot = null;
		
		public override void HeapReportStart (HeapSnapshot snapshot) {
			currentHeapSnapshot = snapshot;
		}
		public override void HeapObjectUnreachable (LoadedClass c, uint size) {
			c.InstanceFreed (size);
			currentHeapSnapshot.HeapObjectUnreachable (c, size);
		}
		public override void HeapObjectReachable (HeapObject o) {
		}
		public override void HeapReportEnd (HeapSnapshot snapshot) {
			snapshot.InitializeBackReferences ();
		}
		
		List<AllocationSummary> allocationSummaries;
		public AllocationSummary [] AllocationSummaries {
			get {
				return allocationSummaries.ToArray ();
			}
		}
		AllocationSummary currentAllocationSummary;
		
		public override void AllocationSummaryStart (uint collection, ulong startCounter, DateTime startTime) {
			currentAllocationSummary = new AllocationSummary (collection, startCounter, startTime);
		}
		public override void ClassAllocationSummary (LoadedClass c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes) {
			if (currentAllocationSummary != null) {
				currentAllocationSummary.RecordData (c, reachableInstances, reachableBytes, unreachableInstances, unreachableBytes);
			}
		}
		public override void AllocationSummaryEnd (uint collection, ulong endCounter, DateTime endTime) {
			if ((currentAllocationSummary != null) && (currentAllocationSummary.Collection == collection)) {
				currentAllocationSummary.EndCounter = endCounter;
				currentAllocationSummary.EndTime = endTime;
				allocationSummaries.Add (currentAllocationSummary);
				currentAllocationSummary = null;
			}
		}
		
		public ProfilerEventHandler () : base (new LoadedElementHandler<LoadedClass,LoadedMethod,UnmanagedFunctionFromRegion,UnmanagedFunctionFromID,ExecutableMemoryRegion,HeapObject,HeapSnapshot> (new LoadedElementFactory ())) {
			perThreadStacks = new Dictionary<ulong,CallStack> ();
			stack = null;
			stackTraceFactory = new StackTrace.Factory ();
			unknownStatisticalHitsCollector = new UnknownStatisticalHitsCollector ();
			gcStatistics = new List<GcStatistics> ();
			pendingGcStatistics = new List<GcStatistics> ();
			currentGcStatistics = null;
			allocationSummaries = new List<AllocationSummary> ();
			currentAllocationSummary = null;
			globalMonitorStatistics = new GlobalMonitorStatistics ();
		}
	}
}
