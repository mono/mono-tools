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

namespace Mono.Profiler {
	public enum MonitorEvent {
		CONTENTION = 1,
		DONE = 2,
		FAIL = 3
	}
	
	public class LoadedAssembly {
		uint id;
		public uint ID {
			get {
				return id;
			}
		}
		
		string name;
		public string Name {
			get {
				return name;
			}
		}
		string baseName;
		public string BaseName {
			get {
				return baseName;
			}
		}
		uint major;
		public uint Major {
			get {
				return major;
			}
		}
		uint minor;
		public uint Minor {
			get {
				return minor;
			}
		}
		uint build;
		public uint Build {
			get {
				return build;
			}
		}
		uint revision;
		public uint Revision {
			get {
				return revision;
			}
		}
		string culture;
		public string Culture {
			get {
				return culture;
			}
		}
		string publicKeyToken;
		public string PublicKeyToken {
			get {
				return publicKeyToken;
			}
		}
		bool retargetable;
		public bool Retargetable {
			get {
				return retargetable;
			}
		}
		
		public static readonly LoadedAssembly Unavailable = new LoadedAssembly (0, "UNAVAILABLE", "UNAVAILABLE", 0, 0, 0, 0, "neutral", "null", false);
		
		public LoadedAssembly (uint id, string name, string baseName, uint major, uint minor, uint build, uint revision, string culture, string publicKeyToken, bool retargetable) {
			this.id = id;
			this.name = name;
			this.baseName = baseName;
			this.major = major;
			this.minor = minor;
			this.build = build;
			this.revision = revision;
			this.culture = culture;
			this.publicKeyToken = publicKeyToken;
			this.retargetable = retargetable;
		}
	}
	
	public interface ILoadedElement {
		uint ID {get;}
		string Name {get;}
	}
	public interface ILoadedClass : ILoadedElement {
		uint Size {get;}
		LoadedAssembly Assembly {get;}
	}
	public interface ILoadedMethod<LC> : ILoadedElement where LC : ILoadedClass {
		LC Class {get;}
		bool IsWrapper {get;}
	}
	public interface IUnmanagedFunctionFromID<MR,UFR> : ILoadedElement where UFR : IUnmanagedFunctionFromRegion<UFR> where MR : IExecutableMemoryRegion<UFR> {
		MR Region {get;}
	}
	public interface IUnmanagedFunctionFromRegion<UFR> where UFR : IUnmanagedFunctionFromRegion<UFR> {
		IExecutableMemoryRegion<UFR> Region {get; set;}
		string Name {get; set;}
		uint StartOffset {get; set;}
		uint EndOffset {get; set;}
	}
	public struct StackSectionElement<LC,LM> where LC : ILoadedClass where LM : ILoadedMethod<LC> {
		LM method;
		public LM Method {
			get {
				return method;
			}
			set {
				method = value;
			}
		}
		bool isBeingJitted;
		public bool IsBeingJitted {
			get {
				return isBeingJitted;
			}
			set {
				isBeingJitted = value;
			}
		}
	}
	public interface IExecutableMemoryRegion<UFR> : ILoadedElement where UFR : IUnmanagedFunctionFromRegion<UFR> {
		ulong StartAddress {get;}
		ulong EndAddress {get;}
		uint FileOffset {get;}
		UFR NewFunction (string name, uint offset);
		UFR GetFunction (uint offset);
		UFR[] Functions {get;}
		void SortFunctions ();
	}
	public interface IAllocatedObject<LC> where LC : ILoadedClass {
		ulong ID {get;}
		LC Class {get;}
		uint Size {get;}
	}
	public interface IHeapObject<HO,LC> : IAllocatedObject<LC> where HO: IHeapObject<HO,LC> where LC : ILoadedClass {
		HO[] References {get;}
		HO[] BackReferences {get;}
	}
	public delegate HO HeapObjectFactory<HO,LC> (ulong ID) where HO : IHeapObject<HO,LC> where LC : ILoadedClass;
	public interface IHeapSnapshot<HO,LC> where HO: IHeapObject<HO,LC> where LC : ILoadedClass {
		uint Collection {get;}
		ulong StartCounter {get;}
		DateTime StartTime {get;}
		ulong EndCounter {get;}
		DateTime EndTime {get;}
		TimeSpan HeaderStartTime {get;}
		HO NewHeapObject (ulong id, LC c, uint size, ulong[] referenceIds, int referencesCount);
		HO GetHeapObject (ulong id);
		HO[] HeapObjects {get;}
		bool RecordSnapshot {get;}
	}
	
	public interface ILoadedElementFactory<LC,LM,UFR,UFI,MR,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		LoadedAssembly NewAssembly (uint id, string name, string baseName, uint major, uint minor, uint build, uint revision, string culture, string publicKeyToken, bool retargetable);
		LC NewClass (uint id, LoadedAssembly assembly, string name, uint size);
		LM NewMethod (uint id, LC c, bool isWrapper, string name);
		MR NewExecutableMemoryRegion (uint id, string fileName, uint fileOffset, ulong startAddress, ulong endAddress);
		UFI NewUnmanagedFunction (uint id, string name, MR region);
		HS NewHeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, TimeSpan headerStartTime, LC[] initialAllocations, bool recordSnapshot);
		bool RecordHeapSnapshots {get; set;}
	}
	
	public interface ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> : ILoadedElementFactory<LC,LM,UFR,UFI,MR,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		LoadedAssembly[] Assemblies {get;}
		LoadedAssembly GetAssembly (uint id);
		LC[] Classes {get;}
		LC GetClass (uint id);
		LM[] Methods {get;}
		LM GetMethod (uint id);
		MR[] ExecutableMemoryRegions {get;}
		MR GetExecutableMemoryRegion (uint id);
		MR GetExecutableMemoryRegion (ulong address);
		void InvalidateExecutableMemoryRegion (uint id);
		void SortExecutableMemoryRegions ();
		UFI[] UnmanagedFunctionsByID {get;}
		UFI GetUnmanagedFunctionByID (uint id);
		HS[] HeapSnapshots {get;}
	}
	public interface IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		DirectivesHandler Directives {get;}
		
		EH LoadedElements {get;}
		Double TicksPerCounterUnit {get;}
		TimeSpan ClicksToTimeSpan (ulong clicks);
		
		/* Properties that state what kind of data has been processed */
		bool HasAllocationData {get;}
		void AllocationDataProcessed ();
		bool HasCallData {get;}
		void CallDataProcessed ();
		bool HasStatisticalData {get;}
		void StatisticalDataProcessed ();
		bool HasHeapSnapshotData {get;}
		void HeapSnapshotDataProcessed ();
		bool HasJitTimeData {get;}
		void JitTimeDataProcessed ();
		bool HasGcTimeData {get;}
		void GcTimeDataProcessed ();
		bool HasHeapSummaryData {get;}
		void HeapSummaryDataProcessed ();
		bool HasLockContentionData {get;}
		void LockContentionDataProcessed ();
		
		void Start (uint version, string runtimeFile, ProfilerFlags flags, ulong startCounter, DateTime startTime);
		void End (uint version, ulong endCounter, DateTime endTime);
		
		void StartBlock (ulong startCounter, DateTime startTime, ulong threadId);
		void EndBlock (ulong endCounter, DateTime endTime, ulong threadId);
		
		void ModuleLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success);
		void ModuleUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name);
		void AssemblyLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success);
		void AssemblyUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name);
		void ApplicationDomainLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success);
		void ApplicationDomainUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name);
		
		void SetCurrentThread (ulong threadId);
		
		void ClassStartLoad (LC c, ulong counter);
		void ClassEndLoad (LC c, ulong counter, bool success);
		void ClassStartUnload (LC c, ulong counter);
		void ClassEndUnload (LC c, ulong counter);
		
		void Allocation (LC c, uint size, LM caller, bool jitTime, ulong objectId, ulong counter);
		void Exception (LC c, ulong counter);
		
		void MethodEnter (LM m, ulong counter);
		void MethodExit (LM m, ulong counter);
		void MethodJitStart (LM m, ulong counter);
		void MethodJitEnd (LM m, ulong counter, bool success);
		void MethodFreed (LM m, ulong counter);
		
		void AdjustStack (uint lastValidFrame, uint topSectionSize, StackSectionElement<LC,LM>[] topSection);
		
		void MethodStatisticalHit (LM m);
		void UnknownMethodStatisticalHit ();
		void UnmanagedFunctionStatisticalHit (UFR f);
		void UnmanagedFunctionStatisticalHit (UFI f);
		void UnknownUnmanagedFunctionStatisticalHit (MR region, uint offset);
		void UnknownUnmanagedFunctionStatisticalHit (ulong address);
		void StatisticalCallChainStart (uint chainDepth);
		
		void ThreadStart (ulong threadId, ulong counter);
		void ThreadEnd (ulong threadId, ulong counter);
		
		void GarbageCollectionStart (uint collection, uint generation, ulong counter);
		void GarbageCollectionEnd (uint collection, uint generation, ulong counter);
		void GarbageCollectionMarkStart (uint collection, uint generation, ulong counter);
		void GarbageCollectionMarkEnd (uint collection, uint generation, ulong counter);
		void GarbageCollectionSweepStart (uint collection, uint generation, ulong counter);
		void GarbageCollectionSweepEnd (uint collection, uint generation, ulong counter);
		void GarbageCollectionResize (uint collection, ulong newSize);
		void GarbageCollectionStopWorldStart (uint collection, uint generation, ulong counter);
		void GarbageCollectionStopWorldEnd (uint collection, uint generation, ulong counter);
		void GarbageCollectionStartWorldStart (uint collection, uint generation, ulong counter);
		void GarbageCollectionStartWorldEnd (uint collection, uint generation, ulong counter);
		
		void HeapReportStart (HS snapshot);
		void HeapObjectUnreachable (LC c, uint size);
		void HeapObjectReachable (HO o);
		void HeapReportEnd (HS snapshot);
		
		void AllocationSummaryStart (uint collection, ulong startCounter, DateTime startTime);
		void ClassAllocationSummary (LC c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes);
		void AllocationSummaryEnd (uint collection, ulong endCounter, DateTime endTime);
		
		void MonitorEvent (MonitorEvent eventCode, LC loadedClass, ulong objectId, ulong counter);
		
		void InitializeData (byte[] data, uint currentOffset);
		void DataProcessed (uint offset);
	}
	
	public class BaseLoadedElement : ILoadedElement {
		uint id;
		public uint ID {
			get {
				return id;
			}
		}
		
		string name;
		public string Name {
			get {
				return name;
			}
		}
		
		public BaseLoadedElement (uint id, string name) {
			this.id = id;
			this.name = name;
		}
	}
	public class BaseLoadedClass : BaseLoadedElement, ILoadedClass {
		LoadedAssembly assembly;
		public LoadedAssembly Assembly {
			get {
				return assembly;
			}
		}
		
		uint size;
		public uint Size {
			get {
				return size;
			}
		}
		
		public BaseLoadedClass (uint id, LoadedAssembly assembly, string name, uint size) : base (id, name) {
			this.assembly = assembly;
			this.size = size;
		}
	}
	public class BaseLoadedMethod<LC> : BaseLoadedElement, ILoadedMethod<LC> where LC : ILoadedClass {
		LC c;
		public LC Class {
			get {
				return c;
			}
		}
		
		bool isWrapper;
		public bool IsWrapper {
			get {
				return isWrapper;
			}
		}
		
		public BaseLoadedMethod (uint id, LC c, bool isWrapper, string name) : base (id, name) {
			this.c = c;
			this.isWrapper = isWrapper;
		}
	}
	public class BaseUnmanagedFunctionFromRegion<UFR> : IUnmanagedFunctionFromRegion<UFR> where UFR : IUnmanagedFunctionFromRegion<UFR> {
		IExecutableMemoryRegion<UFR> region;
		public IExecutableMemoryRegion<UFR> Region {
			get {
				return region;
			}
			set {
				region = value;
			}
		}
		string name;
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
		uint startOffset;
		public uint StartOffset {
			get {
				return startOffset;
			}
			set {
				startOffset = value;
			}
		}
		uint endOffset;
		public uint EndOffset {
			get {
				return endOffset;
			}
			set {
				endOffset = value;
			}
		}
		public BaseUnmanagedFunctionFromRegion () {
			this.region = null;
			this.name = null;
			this.startOffset = 0;
			this.endOffset = 0;
		}
		public BaseUnmanagedFunctionFromRegion (IExecutableMemoryRegion<UFR> region, string name, uint startOffset, uint endOffset) {
			this.region = region;
			this.name = name;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}
	}
	public class BaseUnmanagedFunctionFromID<MR,UFR>: BaseLoadedElement, IUnmanagedFunctionFromID<MR,UFR>  where UFR : IUnmanagedFunctionFromRegion<UFR> where MR : IExecutableMemoryRegion<UFR> {
		MR region;
		public MR Region {
			get {
				return region;
			}
		}
		
		public BaseUnmanagedFunctionFromID (uint id, string name, MR region) : base (id, name) {
			this.region = region;
		}
	}
	
	public class BaseHeapObject<HO,LC> : IHeapObject<HO,LC> where LC : ILoadedClass where HO : BaseHeapObject<HO,LC> {
		ulong id;
		public ulong ID {
			get {
				return id;
			}
		}
		LC c;
		public LC Class {
			get {
				return c;
			}
			internal set {
				c = value;
			}
		}
		
		uint size;
		public uint Size {
			get {
				return size;
			}
			internal set {
				size = value;
			}
		}
		
		static HO[] emptyReferences = new HO [0];
		public static HO[] EmptyReferences {
			get {
				return emptyReferences;
			}
		}
		
		HO[] references;
		public HO[] References {
			get {
				return references;
			}
			internal set {
				references = value;
			}
		}
		
		HO[] backReferences;
		public HO[] BackReferences {
			get {
				return backReferences;
			}
			internal set {
				backReferences = value;
			}
		}
		
		int backReferencesCounter;
		internal void IncrementBackReferences () {
			backReferencesCounter ++;
		}
		internal void AllocateBackReferences () {
			if (references != null) {
				int referencesCount = 0;
				foreach (HO reference in references) {
					if (reference != null) {
						referencesCount ++;
					}
				}
				if (referencesCount != references.Length) {
					if (referencesCount > 0) {
						HO[] newReferences = new HO [referencesCount];
						referencesCount = 0;
						foreach (HO reference in references) {
							if (reference != null) {
								newReferences [referencesCount] = reference;
								referencesCount ++;
							}
						}
						references = newReferences;
					} else {
						references = emptyReferences;
					} 
				}
			} else {
				references = emptyReferences;
			}
			
			if (backReferencesCounter > 0) {
				backReferences = new HO [backReferencesCounter];
				backReferencesCounter = 0;
			} else {
				backReferences = emptyReferences;
			}
		}
		internal void AddBackReference (HO heapObject) {
			backReferences [backReferencesCounter] = heapObject;
			backReferencesCounter ++;
		}
		
		public BaseHeapObject (ulong id) {
			this.id = id;
			this.c = default(LC);
			this.size = 0;
			this.references = null;
			this.backReferences = null;
			this.backReferencesCounter = 0;
		}
	}
	
	public abstract class BaseHeapSnapshot<HO,LC> : IHeapSnapshot<HO,LC> where LC : ILoadedClass where HO : BaseHeapObject<HO,LC> {
		Dictionary<ulong,HO> heap;
		bool backReferencesInitialized;
		HeapObjectFactory<HO,LC> heapObjectFactory;
		
		uint collection;
		public uint Collection {
			get {
				return collection;
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
		TimeSpan headerStartTime;
		public TimeSpan HeaderStartTime {
			get {
				return headerStartTime;
			}
		}
			
		public HO NewHeapObject (ulong id, LC c, uint size, ulong[] referenceIds, int referencesCount) {
			if (backReferencesInitialized) {
				throw new Exception ("Cannot create heap objects after backReferencesInitialized is true");
			}
			
			if (recordSnapshot) {
				HO[] references = new HO [referencesCount];
				HO result = GetOrCreateHeapObject (id);
				for (int i = 0; i < references.Length; i++) {
					references [i] = GetOrCreateHeapObject (referenceIds [i]);
					references [i].IncrementBackReferences ();
				}
				result.References = references;
				result.Size = size;
				result.Class = c;
				return result;
			} else {
				return null;
			}
		}
		
		public void InitializeBackReferences () {
			if (backReferencesInitialized) {
				throw new Exception ("Cannot call InitializeBackReferences twice");
			}
			
			//FIXME: Bad objects should not happen anymore...
			Dictionary<ulong,HO> badObjects = new Dictionary<ulong,HO> ();
			
			foreach (HO heapObject in heap.Values) {
				if (heapObject.Class != null) {
					heapObject.AllocateBackReferences ();
				} else {
					badObjects.Add (heapObject.ID, heapObject);
				}
			}
			
			foreach (ulong id in badObjects.Keys) {
				heap.Remove (id);
			}
			
			foreach (HO heapObject in heap.Values) {
				foreach (HO reference in heapObject.References) {
					reference.AddBackReference (heapObject);
				}
			}
			
			backReferencesInitialized = true;
		}
		
		HO GetOrCreateHeapObject (ulong id) {
			if (recordSnapshot) {
				if (heap.ContainsKey (id)) {
					return heap [id];
				} else {
					HO result = heapObjectFactory (id);
					heap [id] = result;
					return result;
				}
			} else {
				return null;
			}
		}
		
		public HO GetHeapObject (ulong id) {
			return heap [id];
		}
		
		public HO[] HeapObjects {
			get {
				HO[] result = new HO [heap.Values.Count];
				heap.Values.CopyTo (result, 0);
				return result;
			}
		}
		
		bool recordSnapshot;
		public bool RecordSnapshot {
			get {
				return recordSnapshot;
			}
		}
		
		public BaseHeapSnapshot (HeapObjectFactory<HO,LC> heapObjectFactory, uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, TimeSpan headerStartTime, bool recordSnapshot) {
			this.heapObjectFactory = heapObjectFactory;
			this.collection = collection;
			this.startCounter = startCounter;
			this.startTime = startTime;
			this.endCounter = endCounter;
			this.endTime = endTime;
			this.headerStartTime = headerStartTime;
			this.recordSnapshot = recordSnapshot;
			heap = new Dictionary<ulong,HO> ();
			backReferencesInitialized = false;
		}
	}
	
	public struct AllocationClassData<LC> where LC : ILoadedClass  {
		LC c;
		public LC Class {
			get {
				return c;
			}
		}
		uint reachableInstances;
		public uint ReachableInstances {
			get {
				return reachableInstances;
			}
		}
		uint reachableBytes;
		public uint ReachableBytes {
			get {
				return reachableBytes;
			}
		}
		uint unreachableInstances;
		public uint UnreachableInstances {
			get {
				return unreachableInstances;
			}
		}
		uint unreachableBytes;
		public uint UnreachableBytes {
			get {
				return unreachableBytes;
			}
		}
		
		public static Comparison<AllocationClassData<LC>> CompareByReachableBytes = delegate (AllocationClassData<LC> a, AllocationClassData<LC> b) {
			return a.ReachableBytes.CompareTo (b.ReachableBytes);
		};
		public static Comparison<AllocationClassData<LC>> CompareByReachableInstances = delegate (AllocationClassData<LC> a, AllocationClassData<LC> b) {
			return a.ReachableInstances.CompareTo (b.ReachableInstances);
		};
		
		public AllocationClassData (LC c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes) {
			this.c = c;
			this.reachableInstances = reachableInstances;
			this.reachableBytes = reachableBytes;
			this.unreachableInstances = unreachableInstances;
			this.unreachableBytes = unreachableBytes;
		}
	}
	
	public class BaseAllocationSummary<LC> where LC : ILoadedClass {
		uint collection;
		public uint Collection {
			get {
				return collection;
			}
		}
		List<AllocationClassData<LC>> data;
		public AllocationClassData<LC>[] Data {
			get {
				AllocationClassData<LC>[] result = data.ToArray ();
				Array.Sort (result, AllocationClassData<LC>.CompareByReachableBytes);
				Array.Reverse (result);
				return result;
			}
		}
		
		internal void RecordData (LC c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes) {
			data.Add (new AllocationClassData<LC> (c, reachableInstances, reachableBytes, unreachableInstances, unreachableBytes));
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
			internal set {
				endCounter = value;
			}
		}
		DateTime endTime;
		public DateTime EndTime {
			get {
				return endTime;
			}
			internal set {
				endTime = value;
			}
		}
		
		public BaseAllocationSummary (uint collection, ulong startCounter, DateTime startTime) {
			this.collection = collection;
			this.startCounter = startCounter;
			this.startTime = startTime;
			this.endCounter = startCounter;
			this.endTime = startTime;
			data = new List<AllocationClassData<LC>> ();
		}
	}
	
	public enum DirectiveCodes {
		END = 0,
		ALLOCATIONS_CARRY_CALLER = 1,
		ALLOCATIONS_HAVE_STACK = 2,
		ALLOCATIONS_CARRY_ID = 3,
		LOADED_ELEMENTS_CARRY_ID = 4,
		CLASSES_CARRY_ASSEMBLY_ID = 5,
		METHODS_CARRY_WRAPPER_FLAG = 6,
		LAST
	}
	
	public class DirectivesHandler {
		bool allocationsCarryCallerMethod;
		public bool AllocationsCarryCallerMethod {
			get {
				return allocationsCarryCallerMethod;
			}
		}
		public void AllocationsCarryCallerMethodReceived () {
			allocationsCarryCallerMethod = true;
		}
		
		bool allocationsHaveStackTrace;
		public bool AllocationsHaveStackTrace {
			get {
				return allocationsHaveStackTrace;
			}
		}
		public void AllocationsHaveStackTraceReceived () {
			allocationsHaveStackTrace = true;
		}
		
		bool allocationsCarryId;
		public bool AllocationsCarryId {
			get {
				return allocationsCarryId;
			}
		}
		public void AllocationsCarryIdReceived () {
			allocationsCarryId = true;
		}
		
		bool loadedElementsCarryId;
		public bool LoadedElementsCarryId {
			get {
				return loadedElementsCarryId;
			}
		}
		public void LoadedElementsCarryIdReceived () {
			loadedElementsCarryId = true;
		}
		
		bool classesCarryAssemblyId;
		public bool ClassesCarryAssemblyId {
			get {
				return classesCarryAssemblyId;
			}
		}
		public void ClassesCarryAssemblyIdReceived () {
			classesCarryAssemblyId = true;
		}
		
		bool methodsCarryWrapperFlag;
		public bool MethodsCarryWrapperFlag {
			get {
				return methodsCarryWrapperFlag;
			}
		}
		public void MethodsCarryWrapperFlagReceived () {
			methodsCarryWrapperFlag = true;
		}
		
		public DirectivesHandler () {
			allocationsCarryCallerMethod = false;
			allocationsHaveStackTrace = false;
			allocationsCarryId = false;
		}
	}
	
	public class BaseProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> : IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		DirectivesHandler directives;
		public DirectivesHandler Directives {
			get {
				return directives;
			}
		}
		
		EH loadedElements;
		public EH LoadedElements {
			get {
				return loadedElements;
			}
		}
		public virtual Double TicksPerCounterUnit {
			get {
				return 0;
			}
		}
		public virtual TimeSpan ClicksToTimeSpan (ulong clicks) {
			return TimeSpan.FromTicks (0);
		}
		
		public BaseProfilerEventHandler (EH loadedElements) {
			this.loadedElements = loadedElements;
			this.directives = new DirectivesHandler ();
		}
		protected BaseProfilerEventHandler (IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> baseHandler) {
			this.loadedElements = baseHandler.LoadedElements;
			this.directives = baseHandler.Directives;
		}
		
		bool hasAllocationData;
		public bool HasAllocationData {
			get {
				return hasAllocationData;
			}
		}
		public void AllocationDataProcessed () {
			hasAllocationData = true;
		}
		
		bool hasCallData;
		public bool HasCallData {
			get {
				return hasCallData;
			}
		}
		public void CallDataProcessed () {
			hasCallData = true;
		}
		
		bool hasStatisticalData;
		public bool HasStatisticalData {
			get {
				return hasStatisticalData;
			}
		}
		public void StatisticalDataProcessed () {
			hasStatisticalData = true;
		}
		
		bool hasHeapSnapshotData;
		public bool HasHeapSnapshotData {
			get {
				return hasHeapSnapshotData;
			}
		}
		public void HeapSnapshotDataProcessed () {
			hasHeapSnapshotData = true;
		}
		
		bool hasJitTimeData;
		public bool HasJitTimeData {
			get {
				return hasJitTimeData;
			}
		}
		public void JitTimeDataProcessed () {
			hasJitTimeData = true;
		}
		
		bool hasGcTimeData;
		public bool HasGcTimeData {
			get {
				return hasGcTimeData;
			}
		}
		public void GcTimeDataProcessed () {
			hasGcTimeData = true;
		}
		
		bool hasHeapSummaryData;
		public bool HasHeapSummaryData {
			get {
				return hasHeapSummaryData;
			}
		}
		public void HeapSummaryDataProcessed () {
			hasHeapSummaryData = true;
		}
		
		bool hasLockContentionData;
		public bool HasLockContentionData {
			get {
				return hasLockContentionData;
			}
		}
		public void LockContentionDataProcessed () {
			hasLockContentionData = true;
		}
		
		public virtual void Start (uint version, string runtimeFile, ProfilerFlags flags, ulong startCounter, DateTime startTime) {}
		public virtual void End (uint version, ulong endCounter, DateTime endTime) {}
		
		public virtual void StartBlock (ulong startCounter, DateTime startTime, ulong threadId) {}
		public virtual void EndBlock (ulong endCounter, DateTime endTime, ulong threadId) {}
		
		public virtual void ModuleLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public virtual void ModuleUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		public virtual void AssemblyLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public virtual void AssemblyUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		public virtual void ApplicationDomainLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {}
		public virtual void ApplicationDomainUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {}
		
		public virtual void SetCurrentThread (ulong threadId) {}
		
		public virtual void ClassStartLoad (LC c, ulong counter) {}
		public virtual void ClassEndLoad (LC c, ulong counter, bool success) {}
		public virtual void ClassStartUnload (LC c, ulong counter) {}
		public virtual void ClassEndUnload (LC c, ulong counter) {}
		
		public virtual void Allocation (LC c, uint size, LM caller, bool jitTime, ulong objectId, ulong counter) {}
		public virtual void Exception (LC c, ulong counter) {}
		
		public virtual void MethodEnter (LM m, ulong counter) {}
		public virtual void MethodExit (LM m, ulong counter) {}
		public virtual void MethodJitStart (LM m, ulong counter) {}
		public virtual void MethodJitEnd (LM m, ulong counter, bool success) {}
		public virtual void MethodFreed (LM m, ulong counter) {}
		
		public virtual void AdjustStack (uint lastValidFrame, uint topSectionSize, StackSectionElement<LC,LM>[] topSection) {}
		
		public virtual void MethodStatisticalHit (LM m) {}
		public virtual void UnknownMethodStatisticalHit () {}
		public virtual void UnmanagedFunctionStatisticalHit (UFR f) {}
		public virtual void UnmanagedFunctionStatisticalHit (UFI f) {}
		public virtual void UnknownUnmanagedFunctionStatisticalHit (MR region, uint offset) {}
		public virtual void UnknownUnmanagedFunctionStatisticalHit (ulong address) {}
		public virtual void StatisticalCallChainStart (uint chainDepth) {}
		
		public virtual void ThreadStart (ulong threadId, ulong counter) {}
		public virtual void ThreadEnd (ulong threadId, ulong counter) {}
		
		public virtual void GarbageCollectionStart (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionEnd (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionMarkStart (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionMarkEnd (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionSweepStart (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionSweepEnd (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionResize (uint collection, ulong newSize) {}
		public virtual void GarbageCollectionStopWorldStart (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionStopWorldEnd (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionStartWorldStart (uint collection, uint generation, ulong counter) {}
		public virtual void GarbageCollectionStartWorldEnd (uint collection, uint generation, ulong counter) {}
		
		public virtual void HeapReportStart (HS snapshot) {}
		public virtual void HeapObjectUnreachable (LC c, uint size) {}
		public virtual void HeapObjectReachable (HO o) {}
		public virtual void HeapReportEnd (HS snapshot) {}
		
		public virtual void AllocationSummaryStart (uint collection, ulong startCounter, DateTime startTime) {}
		public virtual void ClassAllocationSummary (LC c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes) {}
		public virtual void AllocationSummaryEnd (uint collection, ulong endCounter, DateTime endTime) {}
		
		public virtual void MonitorEvent (MonitorEvent eventCode, LC loadedClass, ulong objectId, ulong counter) {}
		
		public virtual void InitializeData (byte[] data, uint currentOffset) {}
		public virtual void DataProcessed (uint offset) {}
	}

	public class DebugProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> : BaseProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		uint currentOffset;
		public uint CurrentOffset {
			get {
				return currentOffset;
			}
		}
		
		byte[] data;
		TextWriter output;
		
		public DebugProfilerEventHandler (IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> baseHandler, TextWriter output) : base (baseHandler) {
			this.output = output;
			this.data = null;
			this.currentOffset = 0;
		}
		
		public override void Start (uint version, string runtimeFile, ProfilerFlags flags, ulong startCounter, DateTime startTime) {
			output.WriteLine ("");
		}
		public override void End (uint version, ulong endCounter, DateTime endTime) {
			output.WriteLine ("");
		}
		
		public override void StartBlock (ulong startCounter, DateTime startTime, ulong threadId) {
			output.WriteLine ("StartBlock: startCounter {0}, startTime {1}, threadId {2}", startCounter, startTime, threadId);
		}
		public override void EndBlock (ulong endCounter, DateTime endTime, ulong threadId) {
			output.WriteLine ("StartBlock: endCounter {0}, endTime {1}, threadId {2}", endCounter, endTime, threadId);
		}
		
		public override void ModuleLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {
			output.WriteLine ("ModuleLoaded");
		}
		public override void ModuleUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {
			output.WriteLine ("ModuleUnloaded");
		}
		public override void AssemblyLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {
			output.WriteLine ("AssemblyLoaded");
		}
		public override void AssemblyUnloaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name) {
			output.WriteLine ("AssemblyUnloaded");
		}
		public override void ApplicationDomainLoaded (ulong threadId, uint id, ulong startCounter, ulong endCounter, string name, bool success) {
			output.WriteLine ("ApplicationDomainLoaded");
		}
		public override void ApplicationDomainUnloaded (ulong threadId,uint id,  ulong startCounter, ulong endCounter, string name) {
			output.WriteLine ("ApplicationDomainUnloaded");
		}
		
		public override void SetCurrentThread (ulong threadId) {
			output.WriteLine ("SetCurrentThread");
		}
		
		public override void ClassStartLoad (LC c, ulong counter) {
			output.WriteLine ("ClassStartLoad");
		}
		public override void ClassEndLoad (LC c, ulong counter, bool success) {
			output.WriteLine ("ClassEndLoad");
		}
		public override void ClassStartUnload (LC c, ulong counter) {
			output.WriteLine ("ClassStartUnload");
		}
		public override void ClassEndUnload (LC c, ulong counter) {
			output.WriteLine ("ClassEndUnload");
		}
		
		public override void Allocation (LC c, uint size, LM caller, bool jitTime, ulong objectId, ulong counter) {
			output.WriteLine ("Allocation [classId {0}({1}), size {2}, callerId {3}), jitTime {4}, counter {5}]", c.ID, c.Name, size, caller != null ? caller.ID : 0, jitTime, counter);
		}
		public override void Exception (LC c, ulong counter) {
			output.WriteLine ("Exception");
		}
		
		public override void MethodEnter (LM m, ulong counter) {
			output.WriteLine ("MethodEnter");
		}
		public override void MethodExit (LM m, ulong counter) {
			output.WriteLine ("MethodExit");
		}
		public override void MethodJitStart (LM m, ulong counter) {
			output.WriteLine ("MethodJitStart");
		}
		public override void MethodJitEnd (LM m, ulong counter, bool success) {
			output.WriteLine ("MethodJitEnd");
		}
		public override void MethodFreed (LM m, ulong counter) {
			output.WriteLine ("MethodFreed");
		}
		
		public override void AdjustStack (uint lastValidFrame, uint topSectionSize, StackSectionElement<LC,LM>[] topSection) {
			output.WriteLine ("AdjustStack");
		}
		
		public override void MethodStatisticalHit (LM m) {
			output.WriteLine ("MethodStatisticalHit");
		}
		public override void UnknownMethodStatisticalHit () {
			output.WriteLine ("UnknownMethodStatisticalHit");
		}
		public override void UnmanagedFunctionStatisticalHit (UFR f) {
			output.WriteLine ("UnmanagedFunctionStatisticalHit");
		}
		public override void UnmanagedFunctionStatisticalHit (UFI f) {
			output.WriteLine ("UnmanagedFunctionStatisticalHit");
		}
		public override void UnknownUnmanagedFunctionStatisticalHit (MR region, uint offset) {
			output.WriteLine ("UnknownUnmanagedFunctionStatisticalHit");
		}
		public override void UnknownUnmanagedFunctionStatisticalHit (ulong address) {
			output.WriteLine ("UnknownUnmanagedFunctionStatisticalHit");
		}
		public override void StatisticalCallChainStart (uint chainDepth) {
			output.WriteLine ("StatisticalCallChainStart");
		}
		
		public override void ThreadStart (ulong threadId, ulong counter) {
			output.WriteLine ("ThreadStart");
		}
		public override void ThreadEnd (ulong threadId, ulong counter) {
			output.WriteLine ("ThreadEnd");
		}
		
		public override void GarbageCollectionStart (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionStart");
		}
		public override void GarbageCollectionEnd (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionEnd");
		}
		public override void GarbageCollectionMarkStart (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionMarkStart");
		}
		public override void GarbageCollectionMarkEnd (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionMarkEnd");
		}
		public override void GarbageCollectionSweepStart (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionSweepStart");
		}
		public override void GarbageCollectionSweepEnd (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionSweepEnd");
		}
		public override void GarbageCollectionResize (uint collection, ulong newSize) {
			output.WriteLine ("GarbageCollectionResize");
		}
		public override void GarbageCollectionStopWorldStart (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionStopWorldStart");
		}
		public override void GarbageCollectionStopWorldEnd (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionStopWorldEnd");
		}
		public override void GarbageCollectionStartWorldStart (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionStartWorldStart");
		}
		public override void GarbageCollectionStartWorldEnd (uint collection, uint generation, ulong counter) {
			output.WriteLine ("GarbageCollectionStartWorldEnd");
		}
		
		public override void HeapReportStart (HS snapshot) {
			output.WriteLine ("HeapReportStart");
		}
		public override void HeapObjectUnreachable (LC c, uint size) {
			output.WriteLine ("HeapObjectUnreachable");
		}
		public override void HeapObjectReachable (HO o) {
			output.WriteLine ("HeapObjectReachable");
		}
		public override void HeapReportEnd (HS snapshot) {
			output.WriteLine ("HeapReportEnd");
		}
		
		public override void AllocationSummaryStart (uint collection, ulong startCounter, DateTime startTime) {
			output.WriteLine ("AllocationSummaryStart");
		}
		public override void ClassAllocationSummary (LC c, uint reachableInstances, uint reachableBytes, uint unreachableInstances, uint unreachableBytes) {
			output.WriteLine ("ClassAllocationSummary");
		}
		public override void AllocationSummaryEnd (uint collection, ulong endCounter, DateTime endTime) {
			output.WriteLine ("AllocationSummaryEnd");
		}
		
		public override void InitializeData (byte[] data, uint currentOffset) {
			this.data = data;
			this.currentOffset = currentOffset;
		}
		public override void DataProcessed (uint offset) {
			BlockData.DumpData (data, output, currentOffset, offset);
			currentOffset = offset;
		}
	}
	
	public class BaseExecutableMemoryRegion<UFR> : BaseLoadedElement, IExecutableMemoryRegion<UFR> where UFR : IUnmanagedFunctionFromRegion<UFR>, new() {
		uint fileOffset;
		public uint FileOffset {
			get {
				return fileOffset;
			}
		}
		
		ulong startAddress;
		public ulong StartAddress {
			get {
				return startAddress;
			}
		}
		
		ulong endAddress;
		public ulong EndAddress {
			get {
				return endAddress;
			}
		}
		
		List<UFR> functions;
		
		public UFR NewFunction (string name, uint offset) {
			UFR result = new UFR ();
			result.Name = name;
			result.StartOffset = offset;
			result.EndOffset = offset;
			result.Region = this;
			functions.Add (result);
			return result;
		}
		
		public UFR GetFunction (uint offset) {
			int lowIndex = 0;
			int highIndex = functions.Count;
			int middleIndex = lowIndex + ((highIndex - lowIndex) / 2);
			UFR middleFunction = (middleIndex < functions.Count) ? functions [middleIndex] : default (UFR);
			
			while (lowIndex != highIndex) {
				if (middleFunction.StartOffset > offset) {
					if (middleIndex > 0) {
						highIndex = middleIndex;
					} else {
						return default (UFR);
					}
				} else if (middleFunction.EndOffset < offset) {
					if (middleIndex < functions.Count - 1) {
						lowIndex = middleIndex;
					} else {
						return default (UFR);
					}
				} else {
					return middleFunction;
				}
				
				middleIndex = lowIndex + ((highIndex - lowIndex) / 2);
				middleFunction = functions [middleIndex];
			}
			
			if ((middleFunction == null) || (middleFunction.StartOffset > offset) || (middleFunction.EndOffset < offset)) {
				return default (UFR);
			} else {
				return middleFunction;
			}
		}
		
		public UFR[] Functions {
			get {
				UFR[] result = new UFR [functions.Count];
				functions.CopyTo (result);
				return result;
			}
		}
		
		public static Comparison<UFR> CompareByStartOffset = delegate (UFR a, UFR b) {
			return a.StartOffset.CompareTo (b.StartOffset);
		};
		public void SortFunctions () {
			functions.Sort (CompareByStartOffset);
			if (functions.Count > 0) {
				UFR previousFunction = functions [0];
				for (int i = 1; i < functions.Count; i++) {
					UFR currentFunction = functions [i];
					previousFunction.EndOffset = currentFunction.StartOffset - 1;
					previousFunction = currentFunction;
				}
				previousFunction.EndOffset = (uint) (EndAddress - StartAddress);
			}
		}
		
		public BaseExecutableMemoryRegion (uint id, string name, uint fileOffset, ulong startAddress, ulong endAddress) : base (id, name) {
			this.fileOffset = fileOffset;
			this.startAddress = startAddress;
			this.endAddress = endAddress;
			functions = new List<UFR> ();
			
			NativeLibraryReader.FillFunctions<BaseExecutableMemoryRegion<UFR>,UFR> (this);
		}
	}
	
	public class LoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
		ILoadedElementFactory<LC,LM,UFR,UFI,MR,HO,HS> factory;
		
		int loadedAssembliesCount;
		LoadedAssembly[] loadedAssemblies;
		public LoadedAssembly[] Assemblies {
			get {
				LoadedAssembly[] result = new LoadedAssembly [loadedAssembliesCount];
				int resultIndex = 0;
				for (int i = 0; i < loadedAssemblies.Length; i++) {
					LoadedAssembly c = loadedAssemblies [i];
					if (c != null) {
						result [resultIndex] = c;
						resultIndex ++;
					}
				}
				return result;
			}
		}
		public LoadedAssembly GetAssembly (uint id) {
			LoadedAssembly result = loadedAssemblies [(int) id];
			return (result != null) ? result : LoadedAssembly.Unavailable;
		}
		
		int loadedClassesCount;
		LC[] loadedClasses;
		public LC[] Classes {
			get {
				LC[] result = new LC [loadedClassesCount];
				int resultIndex = 0;
				for (int i = 0; i < loadedClasses.Length; i++) {
					LC c = loadedClasses [i];
					if (c != null) {
						result [resultIndex] = c;
						resultIndex ++;
					}
				}
				return result;
			}
		}
		public LC GetClass (uint id) {
			return loadedClasses [(int) id];
		}
		
		int loadedMethodsCount;
		LM[] loadedMethods;
		public LM[] Methods {
			get {
				LM[] result = new LM [loadedMethodsCount];
				int resultIndex = 0;
				for (int i = 0; i < loadedMethods.Length; i++) {
					LM m = loadedMethods [i];
					if (m != null) {
						result [resultIndex] = m;
						resultIndex ++;
					}
				}
				return result;
			}
		}
		public LM GetMethod (uint id) {
			return loadedMethods [(int) id];
		}
		
		Dictionary<uint,MR> memoryRegions;
		List<MR> sortedMemoryRegions;
		public MR[] ExecutableMemoryRegions {
			get {
				MR[] result = new MR [memoryRegions.Count];
				memoryRegions.Values.CopyTo (result, 0);
				return result;
			}
		}
		public MR GetExecutableMemoryRegion (uint id) {
			return memoryRegions [id];
		}
		public MR GetExecutableMemoryRegion (ulong address) {
			int lowIndex = 0;
			int highIndex = sortedMemoryRegions.Count;
			int middleIndex = lowIndex + ((highIndex - lowIndex) / 2);
			MR middleRegion = (middleIndex < sortedMemoryRegions.Count) ? sortedMemoryRegions [middleIndex] : default (MR);
			
			while (lowIndex != highIndex) {
				if (middleRegion.StartAddress > address) {
					if (middleIndex > 0) {
						highIndex = middleIndex;
					} else {
						return default (MR);
					}
				} else if (middleRegion.EndAddress < address) {
					if (middleIndex < sortedMemoryRegions.Count - 1) {
						lowIndex = middleIndex;
					} else {
						return default (MR);
					}
				} else {
					return middleRegion;
				}
				
				middleIndex = lowIndex + ((highIndex - lowIndex) / 2);
				middleRegion = sortedMemoryRegions [middleIndex];
			}
			
			if ((middleRegion == null) || (middleRegion.StartAddress > address) || (middleRegion.EndAddress < address)) {
				return default (MR);
			} else {
				return middleRegion;
			}
		}
		public void InvalidateExecutableMemoryRegion (uint id) {
			MR region = GetExecutableMemoryRegion (id);
			if (region != null) {
				sortedMemoryRegions.Remove (region);
			}
		}
		static Comparison<MR> CompareByStartAddress = delegate (MR a, MR b) {
			return a.StartAddress.CompareTo (b.StartAddress);
		};
		public void SortExecutableMemoryRegions () {
				sortedMemoryRegions.Sort (CompareByStartAddress);
		}
		
		public LoadedAssembly NewAssembly (uint id, string name, string baseName, uint major, uint minor, uint build, uint revision, string culture, string publicKeyToken, bool retargetable) {
			LoadedAssembly result = factory.NewAssembly (id, name, baseName, major, minor, build, revision, culture, publicKeyToken, retargetable);
			if (loadedAssemblies.Length <= id) {
				LoadedAssembly[] newLoadedAssemblies = new LoadedAssembly [((int) id + 1) * 2];
				loadedAssemblies.CopyTo (newLoadedAssemblies, 0);
				loadedAssemblies = newLoadedAssemblies;
			}
			loadedAssemblies [(int) id] = result;
			loadedAssembliesCount ++;
			return result;
		}
		
		public LC NewClass (uint id, LoadedAssembly assembly, string name, uint size) {
			LC result = factory.NewClass (id, assembly, name, size);
			if (loadedClasses.Length <= id) {
				LC[] newLoadedClasses = new LC [((int) id + 1) * 2];
				loadedClasses.CopyTo (newLoadedClasses, 0);
				loadedClasses = newLoadedClasses;
			}
			loadedClasses [(int) id] = result;
			loadedClassesCount ++;
			return result;
		}
		
		public LM NewMethod (uint id, LC c, bool isWrapper, string name) {
			LM result = factory.NewMethod (id, c, isWrapper, name);
			if (loadedMethods.Length <= id) {
				LM[] newLoadedMethods = new LM [((int) id + 1) * 2];
				loadedMethods.CopyTo (newLoadedMethods, 0);
				loadedMethods = newLoadedMethods;
			}
			loadedMethods [(int) id] = result;
			loadedMethodsCount ++;
			return result;
		}
		
		public MR NewExecutableMemoryRegion (uint id, string fileName, uint fileOffset, ulong startAddress, ulong endAddress) {
			MR result = factory.NewExecutableMemoryRegion (id, fileName, fileOffset, startAddress, endAddress);
			memoryRegions.Add (id, result);
			sortedMemoryRegions.Add (result);
			return result;
		}
		
		List<HS> heapSnapshots;
		public HS NewHeapSnapshot (uint collection, ulong startCounter, DateTime startTime, ulong endCounter, DateTime endTime, TimeSpan headerStartTime, LC[] initialAllocations, bool recordSnapshot) {
			HS result = factory.NewHeapSnapshot (collection, startCounter, startTime, endCounter, endTime, headerStartTime, initialAllocations, recordSnapshot);
			heapSnapshots.Add (result);
			return result;
		}
		public HS[] HeapSnapshots {
			get {
				HS[] result = new HS [heapSnapshots.Count];
				heapSnapshots.CopyTo (result);
				return result;
			}
		}
		
		int unmanagedFunctionsByIDCount;
		UFI[] unmanagedFunctionsByID;
		public UFI[] UnmanagedFunctionsByID {
			get {
				UFI[] result = new UFI [unmanagedFunctionsByIDCount];
				int resultIndex = 0;
				for (int i = 0; i < unmanagedFunctionsByID.Length; i++) {
					UFI f = unmanagedFunctionsByID [i];
					if (f != null) {
						result [resultIndex] = f;
						resultIndex ++;
					}
				}
				return result;
			}
		}
		public UFI GetUnmanagedFunctionByID (uint id) {
			return unmanagedFunctionsByID [(int) id];
		}
		public UFI NewUnmanagedFunction (uint id, string name, MR region) {
			UFI result = factory.NewUnmanagedFunction (id, name, region);
			if (unmanagedFunctionsByID.Length <= id) {
				UFI[] newUnmanagedFunctionsByID = new UFI [((int) id + 1) * 2];
				unmanagedFunctionsByID.CopyTo (newUnmanagedFunctionsByID, 0);
				unmanagedFunctionsByID = newUnmanagedFunctionsByID;
			}
			unmanagedFunctionsByID [(int) id] = result;
			unmanagedFunctionsByIDCount ++;
			return result;
		}
		
		public bool RecordHeapSnapshots {
			get {
				return factory.RecordHeapSnapshots;
			}
			set {
				factory.RecordHeapSnapshots = value;
			}
		}
		
		public LoadedElementHandler (ILoadedElementFactory<LC,LM,UFR,UFI,MR,HO,HS> factory) {
			this.factory = factory;
			loadedAssemblies = new LoadedAssembly [10];
			loadedClasses = new LC [1000];
			loadedClassesCount = 0;
			loadedMethods = new LM [5000];
			loadedMethodsCount = 0;
			memoryRegions = new Dictionary<uint,MR> ();
			sortedMemoryRegions = new List<MR> ();
			heapSnapshots = new List<HS> ();
			unmanagedFunctionsByID = new UFI [1000];
			unmanagedFunctionsByIDCount = 0;
		}
	}
}
