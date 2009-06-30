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

namespace  Mono.Profiler {	
	[FlagsAttribute]
	public enum ProfilerFlags {
		APPDOMAIN_EVENTS = 1 << 0,
		ASSEMBLY_EVENTS  = 1 << 1,
		MODULE_EVENTS    = 1 << 2,
		CLASS_EVENTS     = 1 << 3,
		JIT_COMPILATION  = 1 << 4,
		INLINING         = 1 << 5,
		EXCEPTIONS       = 1 << 6,
		ALLOCATIONS      = 1 << 7,
		GC               = 1 << 8,
		THREADS          = 1 << 9,
		REMOTING         = 1 << 10,
		TRANSITIONS      = 1 << 11,
		ENTER_LEAVE      = 1 << 12,
		COVERAGE         = 1 << 13,
		INS_COVERAGE     = 1 << 14,
		STATISTICAL      = 1 << 15,
		METHOD_EVENTS    = 1 << 16
	}
	
	public enum BlockCode {
		INTRO = 1,
		END = 2,
		MAPPING = 3,
		LOADED = 4,
		UNLOADED = 5,
		EVENTS = 6,
		STATISTICAL = 7,
		HEAP_DATA = 8,
		HEAP_SUMMARY = 9,
		DIRECTIVES = 10
	}
	
	public class BlockData {
		static TextWriter debugLog = null;
		public static TextWriter DebugLog {
			get {
				return debugLog;
			}
			set {
				debugLog = value;
			}
		}
		
		public void LogLine (string format, params object[] args) {
			if (debugLog != null) {
				debugLog.WriteLine (format, args);
			}
		}
		
		static readonly DateTime epoch = new DateTime (1970, 1, 1, 0, 0, 0, 0);
		static TimeSpan microsecondsToTimeSpan (ulong microseconds) {
			return TimeSpan.FromTicks ((long) microseconds * 10);
		}
		static DateTime microsecondsFromEpochToDateTime (ulong microseconds) {
			return epoch + microsecondsToTimeSpan (microseconds);
		}
		
		public readonly static int BLOCK_HEADER_SIZE = 10;
		
		public static BlockCode DecodeHeaderBlockCode (byte[] header) {
			if (header.Length == BLOCK_HEADER_SIZE) {
				return (BlockCode) (header [0] | (header [1] << 8));
			} else {
				throw new Exception (String.Format ("Wrong header length (0)", header.Length));
			}
		}
		public static int DecodeHeaderBlockLength (byte[] header) {
			if (header.Length == BLOCK_HEADER_SIZE) {
				return header [2] | (header [3] << 8) | (header [4] << 16) | (header [5] << 24);
			} else {
				throw new Exception (String.Format ("Wrong header length (0)", header.Length));
			}
		}
		public static uint DecodeHeaderBlockCounterDelta (byte[] header) {
			if (header.Length == BLOCK_HEADER_SIZE) {
				return (uint) (header [6] | (header [7] << 8) | (header [8] << 16) | (header [9] << 24));
			} else {
				throw new Exception (String.Format ("Wrong header length (0)", header.Length));
			}
		}
		
		uint fileOffset;
		public uint FileOffset {
			get {
				return fileOffset;
			}
		}
		
		BlockCode code;
		public BlockCode Code {
			get {
				return code;
			}
		}
		int length;
		public int Length {
			get {
				return length;
			}
		}
		ulong headerStartCounter;
		public ulong HeaderStartCounter {
			get {
				return headerStartCounter;
			}
		}
		
		
		
		byte[] data;
		public byte Data (uint index) {
			return data [index];
		}
		public byte this [uint index] {
			get {
				return data [index];
			}
		}
		
		const byte SEVEN_BITS_MASK = 0x7f;
		const byte EIGHT_BIT_MASK = 0x80;
		
		byte ReadByte (ref uint offsetInBlock) {
			byte result = data [offsetInBlock];
			offsetInBlock ++;
			return result;
		}
		
		uint ReadUint (ref uint offsetInBlock) {
			int factor = 0;
			uint r = 0;
			byte v;
			do {
				v = data [offsetInBlock];
				r |= (((uint)(v & SEVEN_BITS_MASK)) << factor);
				offsetInBlock ++;
				factor += 7;
			} while ((v & EIGHT_BIT_MASK) == 0);
			return r;
		}
		
		ulong ReadUlong (ref uint offsetInBlock) {
			int factor = 0;
			ulong r = 0;
			byte v;
			do {
				v = data [offsetInBlock];
				r |= (((ulong)(v & SEVEN_BITS_MASK)) << factor);
				offsetInBlock ++;
				factor += 7;
			} while ((v & EIGHT_BIT_MASK) == 0);
			return r;
		}
		
		string ReadString (ref uint offsetInBlock) {
			int count = 0;
			while (data [offsetInBlock + count] != 0) {
				//LogLine ("Read string: data [offsetInBlock + count] is {0}", (char) data [offsetInBlock + count]);
				count++;
			}
			
			//LogLine ("Read string: count is finally {0}", count);
			
			//result = System.Text.Encoding.UTF8.GetString (data, (int) offsetInBlock, count);
			//result = System.Text.Encoding.ASCII.GetString (data, (int) offsetInBlock, count);
			System.Text.StringBuilder builder = new System.Text.StringBuilder ();
			for (int i = 0; i < count; i++) {
				//LogLine ("Read string: putting data [offsetInBlock + i] ({0}, i = {1}) in builder [i]", (char) data [offsetInBlock + i], i);
				//builder [i] = (char) data [offsetInBlock + i];
				builder.Append ((char) data [offsetInBlock + i]);
			}
			offsetInBlock += (uint) (count + 1);
			return builder.ToString ();
		}
		
		enum LoadedItemInfo {
			MODULE = 1,
			ASSEMBLY = 2,
			APPDOMAIN = 4,
			SUCCESS = 8,
			FAILURE = 16
		}
		const int PACKED_EVENT_CODE_BITS = 3;
		const byte PACKED_EVENT_CODE_MASK = ((1 << PACKED_EVENT_CODE_BITS)-1);
		const int PACKED_EVENT_DATA_BITS = (8 - PACKED_EVENT_CODE_BITS);
		const byte PACKED_EVENT_DATA_MASK = ((1 << PACKED_EVENT_DATA_BITS)-1);
		enum PackedEventCode {
			METHOD_ENTER = 1,
			METHOD_EXIT_IMPLICIT = 2,
			METHOD_EXIT_EXPLICIT = 3,
			CLASS_ALLOCATION = 4,
			METHOD_EVENT = 5,
			CLASS_EVENT = 6,
			OTHER_EVENT = 7
		}
		enum MethodEvent {
			JIT = 0,
			FREED = 1,
			MASK = 1
		}
		MethodEvent MethodEventFromEventCode (int eventCode) {
			return (MethodEvent) (eventCode & (byte) MethodEvent.MASK);
		}
		enum ClassEvent {
			LOAD = 0,
			UNLOAD = 1,
			EXCEPTION = 2,
			LOCK = 3,
			MASK = 3
		}
		ClassEvent ClassEventFromEventCode (int eventCode) {
			return (ClassEvent) (eventCode & (byte) ClassEvent.MASK);
		}
		enum EventResult {
			SUCCESS = 0,
			FAILURE = 4
		}
		const int EVENT_RESULT_MASK = (int) EventResult.FAILURE;
		enum GenericEvent {
			THREAD = 1,
			GC_COLLECTION = 2,
			GC_MARK = 3,
			GC_SWEEP = 4,
			GC_RESIZE = 5,
			GC_STOP_WORLD = 6,
			GC_START_WORLD = 7,
			JIT_TIME_ALLOCATION = 8,
			STACK_SECTION = 9,
			MASK = 15
		}
		GenericEvent GenericEventFromEventCode (int eventCode) {
			return (GenericEvent) (eventCode & (byte) GenericEvent.MASK);
		}
		enum EventKind {
			START = 0,
			END = 1
		}
		EventKind EventKindFromEventCode (int eventCode) {
			return ((eventCode & (1<<4)) != 0) ? EventKind.END : EventKind.START;
		}
		bool EventSuccessFromEventCode (int eventCode) {
			return ((eventCode & EVENT_RESULT_MASK) == (int) EventResult.FAILURE) ? false : true;
		}
		enum StatisticalCode {
			END = 0,
			METHOD = 1,
			UNMANAGED_FUNCTION_ID = 2,
			UNMANAGED_FUNCTION_NEW_ID = 3,
			UNMANAGED_FUNCTION_OFFSET_IN_REGION = 4,
			CALL_CHAIN = 5,
			REGIONS = 7
		}
		enum HeapSnapshotCode {
			NONE = 0,
			OBJECT = 1,
			FREE_OBJECT_CLASS = 2,
			MASK = 3
		}
		
		static void DecodeGarbageCollectionEventValue (uint eventValue, out uint collection, out uint generation) {
			collection = eventValue >> 8;
			generation = eventValue & 0xff;
		}
		
		public void Decode<LC,LM,UFR,UFI,MR,EH,HO,HS> (IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> handler, IDataBlockRecycler blockRecycler) where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
			try {
				Decode<LC,LM,UFR,UFI,MR,EH,HO,HS> (handler);
			} finally {
				blockRecycler.RecycleData (data);
				data = null;
			}
		}
		
		public void Decode<LC,LM,UFR,UFI,MR,EH,HO,HS> (IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> handler) where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
			uint offsetInBlock = 0;
			StackSectionElement<LC,LM>[] stackSection = new StackSectionElement<LC,LM> [32];
			
			if (data == null) {
				throw new DecodingException (this, 0, "Decoding used block");
			}
			
			handler.InitializeData (data, 0);
			
			try {
				//LogLine ("   *** DECODING at offset {0} (code {1})", fileOffset, code);
				switch (code) {
				case BlockCode.INTRO : {
					uint version;
					string runtimeFile;
					uint flags;
					ulong startCounter;
					ulong startTime;
					
					version = ReadUint (ref offsetInBlock);
					runtimeFile = ReadString (ref offsetInBlock);
					flags = ReadUint (ref offsetInBlock);
					startCounter = ReadUlong (ref offsetInBlock);
					startTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK INTRO: version {0}, runtimeFile {1}, flags {2}, startCounter {3}, startTime {4}", version, runtimeFile, (ProfilerFlags) flags, startCounter, startTime);
					
					handler.Start (version, runtimeFile, (ProfilerFlags) flags, startCounter, microsecondsFromEpochToDateTime (startTime));
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.END : {
					uint version;
					ulong endCounter;
					ulong endTime;
					
					version = ReadUint (ref offsetInBlock);
					endCounter = ReadUlong (ref offsetInBlock);
					endTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK END: version {0}, endCounter {1}, endTime {2}", version, endCounter, endTime);
					
					handler.End (version, endCounter, microsecondsFromEpochToDateTime (endTime));
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.LOADED : {
					byte kind = ReadByte (ref offsetInBlock);
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong threadId = ReadUlong (ref offsetInBlock);
					uint id;
					if (handler.Directives.LoadedElementsCarryId) {
						id = ReadUint (ref offsetInBlock);
					} else {
						id = 0;
					}
					string itemName = ReadString (ref offsetInBlock);
					
					bool success = ((kind & (byte)LoadedItemInfo.SUCCESS) != 0);
					kind &= (byte) (LoadedItemInfo.APPDOMAIN|LoadedItemInfo.ASSEMBLY|LoadedItemInfo.MODULE);
					//LogLine ("BLOCK LOADED: kind {0}, startCounter {1}, endCounter {2}, threadId {3}, itemName {4}", (LoadedItemInfo) kind, startCounter, endCounter, threadId, itemName);
					
					switch ((LoadedItemInfo) kind) {
					case LoadedItemInfo.APPDOMAIN: {
						handler.ApplicationDomainLoaded (threadId, id, startCounter, endCounter, itemName, success);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					case LoadedItemInfo.ASSEMBLY: {
						string baseName;
						uint major;
						uint minor;
						uint build;
						uint revision;
						string culture;
						string publicKeyToken;
						bool retargetable;
						if (handler.Directives.ClassesCarryAssemblyId) {
							baseName = ReadString (ref offsetInBlock);
							major = ReadUint (ref offsetInBlock);
							minor = ReadUint (ref offsetInBlock);
							build = ReadUint (ref offsetInBlock);
							revision = ReadUint (ref offsetInBlock);
							culture = ReadString (ref offsetInBlock);
							publicKeyToken = ReadString (ref offsetInBlock);
							retargetable = (ReadUint (ref offsetInBlock) != 0);
						} else {
							int commaPosition = itemName.IndexOf (',');
							if (commaPosition > 0) {
								baseName = itemName.Substring (0, commaPosition);
							} else {
								baseName = "UNKNOWN";
							}
							major = 0;
							minor = 0;
							build = 0;
							revision = 0;
							culture = "neutral";
							publicKeyToken = "null";
							retargetable = false;
						}
						handler.LoadedElements.NewAssembly (id, itemName, baseName, major, minor, build, revision, culture, publicKeyToken, retargetable);
						handler.AssemblyLoaded (threadId, id, startCounter, endCounter, itemName, success);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					case LoadedItemInfo.MODULE: {
						handler.ModuleLoaded (threadId, id, startCounter, endCounter, itemName, success);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					default: {
						throw new DecodingException (this, offsetInBlock, String.Format ("unknown load event kind {0}", kind));
					}
					}
					break;
				}
				case BlockCode.UNLOADED : {
					byte kind = ReadByte (ref offsetInBlock);
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong threadId = ReadUlong (ref offsetInBlock);
					uint id;
					if (handler.Directives.LoadedElementsCarryId) {
						id = ReadUint (ref offsetInBlock);
					} else {
						id = 0;
					}
					string itemName = ReadString (ref offsetInBlock);
					
					//LogLine ("BLOCK UNLOADED: kind {0}, startCounter {1}, endCounter {2}, threadId {3}, itemName {4}", (LoadedItemInfo) kind, startCounter, endCounter, threadId, itemName);
					
					switch ((LoadedItemInfo) kind) {
					case LoadedItemInfo.APPDOMAIN: {
						handler.ApplicationDomainUnloaded (threadId, id, startCounter, endCounter, itemName);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					case LoadedItemInfo.ASSEMBLY: {
						handler.AssemblyUnloaded (threadId, id, startCounter, endCounter, itemName);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					case LoadedItemInfo.MODULE: {
						handler.ModuleUnloaded (threadId, id, startCounter, endCounter, itemName);
						handler.DataProcessed (offsetInBlock);
						break;
					}
					default: {
						throw new DecodingException (this, offsetInBlock, String.Format ("unknown unload event kind {0}", kind));
					}
					}
					break;
				}
				case BlockCode.MAPPING : {
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					ulong threadId = ReadUlong (ref offsetInBlock);
					
					//LogLine ("BLOCK MAPPING (START): startCounter {0}, startTime {1}, threadId {2}", startCounter, startTime, threadId);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), threadId);
					handler.SetCurrentThread (threadId);
					handler.DataProcessed (offsetInBlock);
					
					uint itemId;
					for (itemId = ReadUint (ref offsetInBlock); itemId != 0; itemId = ReadUint (ref offsetInBlock)) {
						uint assemblyId;
						if (handler.Directives.ClassesCarryAssemblyId) {
							assemblyId = ReadUint (ref offsetInBlock);
						} else {
							assemblyId = 0;
						}
						string itemName = ReadString (ref offsetInBlock);
						//LogLine ("BLOCK MAPPING (CLASS): itemId {0}, assemblyId = {1}, itemName {2}, size {3}", itemId, assemblyId, itemName, 0);
						handler.LoadedElements.NewClass (itemId, handler.LoadedElements.GetAssembly (assemblyId), itemName, 0);
					}
					
					for (itemId = ReadUint (ref offsetInBlock); itemId != 0; itemId = ReadUint (ref offsetInBlock)) {
						uint classId = ReadUint (ref offsetInBlock);
						uint wrapperValue;
						if (handler.Directives.MethodsCarryWrapperFlag) {
							wrapperValue = ReadUint (ref offsetInBlock);
						} else {
							wrapperValue = 0;
						}
						bool isWrapper = (wrapperValue != 0) ? true : false;
						string itemName = ReadString (ref offsetInBlock);
						//LogLine ("BLOCK MAPPING (METHOD): itemId {0}, classId {1}, itemName {2}, size {3}", itemId, classId, itemName, 0);
						handler.LoadedElements.NewMethod (itemId, handler.LoadedElements.GetClass (classId), isWrapper, itemName);
					}
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					
					//LogLine ("BLOCK MAPPING (END): endCounter {0}, endTime {1}", endCounter, endTime);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), threadId);
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.EVENTS : {
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					ulong threadId = ReadUlong (ref offsetInBlock);
					
					//LogLine ("BLOCK EVENTS (START): startCounter {0}, startTime {1}, threadId {2}", startCounter, startTime, threadId);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), threadId);
					handler.SetCurrentThread (threadId);
					handler.DataProcessed (offsetInBlock);
					
					ulong baseCounter = ReadUlong (ref offsetInBlock);
					
					byte eventCode;
					for (eventCode = ReadByte (ref offsetInBlock); eventCode != 0; eventCode = ReadByte (ref offsetInBlock)) {
						PackedEventCode packedCode = (PackedEventCode) (eventCode & PACKED_EVENT_CODE_MASK);
						int packedData = ((eventCode >> PACKED_EVENT_CODE_BITS) & PACKED_EVENT_DATA_MASK);
						
						switch (packedCode) {
						case PackedEventCode.CLASS_ALLOCATION: {
							handler.AllocationDataProcessed ();
							
							uint classId = ReadUint (ref offsetInBlock);
							uint classSize = ReadUint (ref offsetInBlock);
							classId <<= PACKED_EVENT_DATA_BITS;
							classId |= (uint) packedData;
							uint callerId = 0;
							if (handler.Directives.AllocationsCarryCallerMethod) {
								callerId = ReadUint (ref offsetInBlock);
							}
							ulong objectId = 0;
							if (handler.Directives.AllocationsCarryId) {
								objectId = ReadUlong (ref offsetInBlock);
							}
							//LogLine ("BLOCK EVENTS (PACKED:CLASS_ALLOCATION): classId {0}, classSize {1}, callerId {2}", classId, classSize, callerId);
							handler.Allocation (handler.LoadedElements.GetClass (classId), classSize, (callerId != 0) ? handler.LoadedElements.GetMethod (callerId) : default (LM), false, objectId, 0);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case PackedEventCode.CLASS_EVENT: {
							ClassEvent classEventCode = ClassEventFromEventCode (packedData);
							switch (classEventCode) {
							case ClassEvent.EXCEPTION: {
								uint classId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								//LogLine ("BLOCK EVENTS (CLASS:EXCEPTION): classId {0}, counterDelta {1}", classId, counterDelta);
								handler.Exception (handler.LoadedElements.GetClass (classId), baseCounter);
								handler.DataProcessed (offsetInBlock);
								break;
							}
							case ClassEvent.LOAD: {
								uint classId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (CLASS:LOAD): classId {0}, classSize {1}, kind {2}", classId, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.ClassStartLoad (handler.LoadedElements.GetClass (classId), baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.ClassEndLoad (handler.LoadedElements.GetClass (classId), baseCounter, EventSuccessFromEventCode (packedData));
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case ClassEvent.UNLOAD: {
								uint classId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (CLASS:UNLOAD): classId {0}, counterDelta {1}, kind {2}", classId, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.ClassStartUnload (handler.LoadedElements.GetClass (classId), baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.ClassEndUnload (handler.LoadedElements.GetClass (classId), baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case ClassEvent.LOCK: {
								handler.LockContentionDataProcessed ();
								
								uint classId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								uint lockEvent = ReadUint (ref offsetInBlock);
								ulong objectId = ReadUlong (ref offsetInBlock);
								
								handler.MonitorEvent ((MonitorEvent) lockEvent, handler.LoadedElements.GetClass (classId), objectId, baseCounter);
								
								break;
							}
							default: {
								throw new DecodingException (this, offsetInBlock, String.Format ("unknown class event {0}", classEventCode));
							}
							}
							break;
						}
						case PackedEventCode.METHOD_ENTER: {
							handler.CallDataProcessed ();
							
							uint methodId = ReadUint (ref offsetInBlock);
							ulong counterDelta = ReadUlong (ref offsetInBlock);
							baseCounter += counterDelta;
							methodId <<= PACKED_EVENT_DATA_BITS;
							methodId |= (uint) packedData;
							
							//LogLine ("BLOCK EVENTS (PACKED:METHOD_ENTER): methodId {0}, counterDelta {1}", methodId, counterDelta);
							handler.MethodEnter (handler.LoadedElements.GetMethod (methodId), baseCounter);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case PackedEventCode.METHOD_EXIT_EXPLICIT: {
							handler.CallDataProcessed ();
							
							uint methodId = ReadUint (ref offsetInBlock);
							ulong counterDelta = ReadUlong (ref offsetInBlock);
							baseCounter += counterDelta;
							methodId <<= PACKED_EVENT_DATA_BITS;
							methodId |= (uint) packedData;
							
							//LogLine ("BLOCK EVENTS (PACKED:METHOD_EXIT_EXPLICIT): methodId {0}, counterDelta {1}", methodId, counterDelta);
							handler.MethodExit (handler.LoadedElements.GetMethod (methodId), baseCounter);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case PackedEventCode.METHOD_EXIT_IMPLICIT: {
							handler.CallDataProcessed ();
							
							//LogLine ("BLOCK EVENTS (PACKED:METHOD_EXIT_IMPLICIT): counterDelta {0}", 0);
							throw new DecodingException (this, offsetInBlock, "PackedEventCode.METHOD_EXIT_IMPLICIT unsupported");
						}
						case PackedEventCode.METHOD_EVENT: {
							MethodEvent methodEventCode = MethodEventFromEventCode (packedData);
							switch (methodEventCode) {
							case MethodEvent.FREED: {
								uint methodId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								//LogLine ("BLOCK EVENTS (METHOD:FREED): methodId {0}, counterDelta {1}", methodId, counterDelta);
								handler.MethodFreed (handler.LoadedElements.GetMethod (methodId), baseCounter);
								handler.DataProcessed (offsetInBlock);
								break;
							}
							case MethodEvent.JIT: {
								handler.JitTimeDataProcessed ();
								
								uint methodId = ReadUint (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (METHOD:JIT): methodId {0}, counterDelta {1}, kind {2}", methodId, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.MethodJitStart (handler.LoadedElements.GetMethod (methodId), baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.MethodJitEnd (handler.LoadedElements.GetMethod (methodId), baseCounter, EventSuccessFromEventCode (packedData));
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							default: {
								throw new DecodingException (this, offsetInBlock, String.Format ("unknown method event {0}", methodEventCode));
							}
							}
							break;
						}
						case PackedEventCode.OTHER_EVENT: {
							GenericEvent genericEventCode = GenericEventFromEventCode (packedData);
							switch (genericEventCode) {
							case GenericEvent.GC_COLLECTION: {
								handler.GcTimeDataProcessed ();
								
								uint collection;
								uint generation;
								DecodeGarbageCollectionEventValue (ReadUint (ref offsetInBlock), out collection, out generation);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:GC_COLLECTION): generation {0}, counterDelta {1}, kind {2}", generation, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.GarbageCollectionStart (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.GarbageCollectionEnd (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.GC_MARK: {
								handler.GcTimeDataProcessed ();
								
								uint collection;
								uint generation;
								DecodeGarbageCollectionEventValue (ReadUint (ref offsetInBlock), out collection, out generation);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:GC_MARK): generation {0}, counterDelta {1}, kind {2}", generation, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.GarbageCollectionMarkStart (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.GarbageCollectionMarkEnd (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.GC_SWEEP: {
								handler.GcTimeDataProcessed ();
								
								uint collection;
								uint generation;
								DecodeGarbageCollectionEventValue (ReadUint (ref offsetInBlock), out collection, out generation);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:GC_SWEEP): generation {0}, counterDelta {1}, kind {2}", generation, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.GarbageCollectionSweepStart (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.GarbageCollectionSweepEnd (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.GC_RESIZE: {
								handler.GcTimeDataProcessed ();
								
								ulong newSize = ReadUlong (ref offsetInBlock);
								uint collection = ReadUint (ref offsetInBlock);
								//LogLine ("BLOCK EVENTS (OTHER:GC_RESIZE): newSize {0}, collection {1}", newSize, collection);
								handler.GarbageCollectionResize (collection, newSize);
								handler.DataProcessed (offsetInBlock);
								break;
							}
							case GenericEvent.GC_STOP_WORLD: {
								handler.GcTimeDataProcessed ();
								
								uint collection;
								uint generation;
								DecodeGarbageCollectionEventValue (ReadUint (ref offsetInBlock), out collection, out generation);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:GC_STOP_WORLD): generation {0}, counterDelta {1}, kind {2}", generation, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.GarbageCollectionStopWorldStart (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.GarbageCollectionStopWorldEnd (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.GC_START_WORLD: {
								handler.GcTimeDataProcessed ();
								
								uint collection;
								uint generation;
								DecodeGarbageCollectionEventValue (ReadUint (ref offsetInBlock), out collection, out generation);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:GC_START_WORLD): generation {0}, counterDelta {1}, kind {2}", generation, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.GarbageCollectionStartWorldStart (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.GarbageCollectionStartWorldEnd (collection, generation, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.THREAD: {
								ulong eventThreadId = ReadUlong (ref offsetInBlock);
								ulong counterDelta = ReadUlong (ref offsetInBlock);
								baseCounter += counterDelta;
								EventKind kind = EventKindFromEventCode (packedData);
								//LogLine ("BLOCK EVENTS (OTHER:THREAD): eventThreadId {0}, counterDelta {1}, kind {2}", eventThreadId, counterDelta, kind);
								if (kind == EventKind.START) {
									handler.ThreadStart (eventThreadId, baseCounter);
									handler.DataProcessed (offsetInBlock);
								} else {
									handler.ThreadEnd (eventThreadId, baseCounter);
									handler.DataProcessed (offsetInBlock);
								}
								break;
							}
							case GenericEvent.JIT_TIME_ALLOCATION: {
								handler.AllocationDataProcessed ();
								
								uint classId = ReadUint (ref offsetInBlock);
								uint classSize = ReadUint (ref offsetInBlock);
								uint callerId = 0;
								if (handler.Directives.AllocationsCarryCallerMethod) {
									callerId = ReadUint (ref offsetInBlock);
								}
								ulong objectId = 0;
								if (handler.Directives.AllocationsCarryId) {
									objectId = ReadUlong (ref offsetInBlock);
								}
								//LogLine ("BLOCK EVENTS (OTHER:JIT_TIME_ALLOCATION): classId {0}, classSize {1}, callerId {2}", classId, classSize, callerId);
								handler.Allocation (handler.LoadedElements.GetClass (classId), classSize, (callerId != 0) ? handler.LoadedElements.GetMethod (callerId) : default (LM), true, objectId, 0);
								handler.DataProcessed (offsetInBlock);
								break;
							}
							case GenericEvent.STACK_SECTION: {
								uint lastValidFrame = ReadUint (ref offsetInBlock);
								uint topSectionSize = ReadUint (ref offsetInBlock);
								
								if (stackSection.Length < topSectionSize) {
									stackSection = new StackSectionElement<LC,LM> [topSectionSize * 2];
								}
								
								for (int i = 0; i < topSectionSize; i++) {
									uint methodId = ReadUint (ref offsetInBlock);
									stackSection [i].IsBeingJitted = ((methodId & 1) != 0) ? true : false;
									methodId >>= 1;
									stackSection [i].Method = handler.LoadedElements.GetMethod (methodId);
								}
								
								handler.AdjustStack (lastValidFrame, topSectionSize, stackSection);
								handler.DataProcessed (offsetInBlock);
								break;
							}
							default: {
								throw new DecodingException (this, offsetInBlock, String.Format ("unknown generic event {0}", genericEventCode));
							}
							}
							break;
						}
						default: {
							throw new DecodingException (this, offsetInBlock, String.Format ("unknown packed event code {0}", packedCode));
						}
						}
					}
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK EVENTS (END): endCounter {0}, endTime {1}", endCounter, endTime);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), threadId);
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.STATISTICAL : {
					handler.StatisticalDataProcessed ();
					
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					
					//LogLine ("BLOCK STATISTICAL (START): startCounter {0}, startTime {1}", startCounter, startTime);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), 0);
					handler.DataProcessed (offsetInBlock);
					
					uint id;
					for (id = ReadUint (ref offsetInBlock); id != (uint) StatisticalCode.END; id = ReadUint (ref offsetInBlock)) {
						StatisticalCode statisticalCode = (StatisticalCode) (id & 7);
						switch (statisticalCode) {
						case StatisticalCode.METHOD: {
							uint methodId = id >> 3;
							//LogLine ("BLOCK STATISTICAL (METHOD): methodId {0}", methodId);
							if (methodId != 0) {
								handler.MethodStatisticalHit (handler.LoadedElements.GetMethod (methodId));
								handler.DataProcessed (offsetInBlock);
							} else {
								handler.UnknownMethodStatisticalHit ();
								handler.DataProcessed (offsetInBlock);
							}
							break;
						}
						case StatisticalCode.UNMANAGED_FUNCTION_ID: {
							uint functionId = id >> 3;
							UFI function = handler.LoadedElements.GetUnmanagedFunctionByID (functionId);
							handler.UnmanagedFunctionStatisticalHit (function);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case StatisticalCode.UNMANAGED_FUNCTION_NEW_ID: {
							uint regionId = id >> 3;
							uint functionId = ReadUint (ref offsetInBlock);
							string name = ReadString (ref offsetInBlock);
							MR region = handler.LoadedElements.GetExecutableMemoryRegion (regionId);
							UFI function = handler.LoadedElements.NewUnmanagedFunction (functionId, name, region);
							handler.UnmanagedFunctionStatisticalHit (function);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case StatisticalCode.UNMANAGED_FUNCTION_OFFSET_IN_REGION: {
							uint regionId = id >> 3;
							if (regionId != 0) {
								MR region = handler.LoadedElements.GetExecutableMemoryRegion (regionId);
								uint offset = ReadUint (ref offsetInBlock);
								UFR function = region.GetFunction (offset);
								if (function != null) {
									//LogLine ("BLOCK STATISTICAL (FUNCTION): regionId {0}, offset {1}", regionId, offset);
									handler.UnmanagedFunctionStatisticalHit (function);
									handler.DataProcessed (offsetInBlock);
								} else {
									//LogLine ("BLOCK STATISTICAL (FUNCTION): regionId {0}, unknown offset {1}", regionId, offset);
									handler.UnknownUnmanagedFunctionStatisticalHit (region, offset);
									handler.DataProcessed (offsetInBlock);
								}
							} else {
								ulong address = ReadUlong (ref offsetInBlock);
								//LogLine ("BLOCK STATISTICAL (FUNCTION): unknown address {0}", address);
								handler.UnknownUnmanagedFunctionStatisticalHit (address);
								handler.DataProcessed (offsetInBlock);
							}
							break;
						}
						case StatisticalCode.CALL_CHAIN: {
							uint chainDepth = id >> 3;
							//LogLine ("BLOCK STATISTICAL (CHAIN): starting chain of depth {0}", chainDepth);
							handler.StatisticalCallChainStart (chainDepth);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case StatisticalCode.REGIONS: {
							uint regionId;
							for (regionId = ReadUint (ref offsetInBlock); regionId != 0; regionId = ReadUint (ref offsetInBlock)) {
								//LogLine ("BLOCK STATISTICAL (REGION): invalidated regionId {0}", regionId);
								handler.LoadedElements.InvalidateExecutableMemoryRegion (regionId);
							}
							for (regionId = ReadUint (ref offsetInBlock); regionId != 0; regionId = ReadUint (ref offsetInBlock)) {
								ulong start = ReadUlong (ref offsetInBlock);
								uint size = ReadUint (ref offsetInBlock);
								uint regionFileOffset = ReadUint (ref offsetInBlock);
								string fileName = ReadString (ref offsetInBlock);
								
								//LogLine ("BLOCK STATISTICAL (REGION): added regionId {0} (fileName {1}, fileOffset {2}, start {3}, end {4}, size {5})", regionId, fileName, regionFileOffset, start, start + size, size);
								handler.LoadedElements.NewExecutableMemoryRegion (regionId, fileName, regionFileOffset, start, start + size);
								//MR region = handler.LoadedElements.NewExecutableMemoryRegion (regionId, fileName, fileOffset, start, start + size);
								//UF[] functions = region.Functions;
								//LogLine ("BLOCK STATISTICAL (REGION): in regionId {0}, got {1} functions", regionId, functions.Length);
								//foreach (UF function in functions) {
									//LogLine ("BLOCK STATISTICAL (REGION): in regionId {0}, got function [{1}-{2}] {3}", regionId, function.StartOffset, function.EndOffset, function.Name);
								//}
							}
							handler.LoadedElements.SortExecutableMemoryRegions ();
							handler.DataProcessed (offsetInBlock);
							break;
						}
						}
					}
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK STATISTICAL (END): endCounter {0}, endTime {1}", endCounter, endTime);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), 0);
					break;
				}
				case BlockCode.HEAP_DATA : {
					handler.HeapSnapshotDataProcessed ();
					
					ulong jobStartCounter = ReadUlong (ref offsetInBlock);
					ulong jobStartTime = ReadUlong (ref offsetInBlock);
					ulong jobEndCounter = ReadUlong (ref offsetInBlock);
					ulong jobEndTime = ReadUlong (ref offsetInBlock);
					uint collection = ReadUint (ref offsetInBlock);
					
					HS snapshot = handler.LoadedElements.NewHeapSnapshot (collection, jobStartCounter, microsecondsFromEpochToDateTime (jobStartTime), jobEndCounter, microsecondsFromEpochToDateTime (jobEndTime), handler.ClicksToTimeSpan (headerStartCounter), handler.LoadedElements.Classes, handler.LoadedElements.RecordHeapSnapshots);
					handler.HeapReportStart (snapshot);
					handler.DataProcessed (offsetInBlock);
					
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK HEAP_DATA (START): ({0}:{1}-{2}:{3}) startCounter {4}, startTime {5}", jobStartCounter, jobStartTime, jobEndCounter, jobEndTime, startCounter, startTime);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), 0);
					handler.DataProcessed (offsetInBlock);
					
					ulong item;
					ulong[] references = new ulong [50];
					for (item = ReadUlong (ref offsetInBlock); item != 0; item = ReadUlong (ref offsetInBlock)) {
						HeapSnapshotCode itemCode = (HeapSnapshotCode) (((int) item) & ((int) HeapSnapshotCode.MASK));
						//LogLine ("Got raw value {0} (code {1})", item, itemCode);
						switch (itemCode) {
						case HeapSnapshotCode.FREE_OBJECT_CLASS: {
							uint classId = (uint) (item >> 2);
							uint size = ReadUint (ref offsetInBlock);
							LC c = handler.LoadedElements.GetClass (classId);
							//LogLine ("  Class id {0}, size {1}", classId, size);
							handler.HeapObjectUnreachable (c, size);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						case HeapSnapshotCode.OBJECT: {
							uint classId = ReadUint (ref offsetInBlock);
							uint size = ReadUint (ref offsetInBlock);
							int referencesCount = (int) ReadUint (ref offsetInBlock);
							ulong objectId = item & (~ ((ulong) HeapSnapshotCode.MASK));
							//LogLine ("  Object id {0}, references {1}", objectId, referencesCount);
							if (references.Length < referencesCount) {
								references = new ulong [referencesCount + 50];
							}
							for (int i = 0; i < referencesCount; i++) {
								references [i] = ReadUlong (ref offsetInBlock);
								//LogLine ("    reference[{0}] {1}", i, references [i]);
							}
							LC c = handler.LoadedElements.GetClass (classId);
							HO o = snapshot.NewHeapObject (objectId, c, size, references, referencesCount);
							handler.HeapObjectReachable (o);
							handler.DataProcessed (offsetInBlock);
							break;
						}
						default: {
							throw new DecodingException (this, offsetInBlock, String.Format ("unknown item code {0}", itemCode));
						}
						}
					}
					handler.HeapReportEnd (snapshot);
					handler.DataProcessed (offsetInBlock);
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					//LogLine ("BLOCK HEAP_DATA (END): endCounter {0}, endTime {1}", endCounter, endTime);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), 0);
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.HEAP_SUMMARY : {
					handler.HeapSummaryDataProcessed ();
					
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					uint collection = ReadUint (ref offsetInBlock);
					
					//LogLine ("BLOCK HEAP_SUMMARY (START): ([]{0}:{1}) startCounter {4}, startTime {5}", collection, startCounter, startTime);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), 0);
					handler.AllocationSummaryStart (collection, startCounter, microsecondsFromEpochToDateTime (startTime));
					handler.DataProcessed (offsetInBlock);
					
					uint id;
					for (id = ReadUint (ref offsetInBlock); id != 0; id = ReadUint (ref offsetInBlock)) {
						uint reachableInstances = ReadUint (ref offsetInBlock);
						uint reachableBytes = ReadUint (ref offsetInBlock);
						uint unreachableInstances = ReadUint (ref offsetInBlock);
						uint unreachableBytes = ReadUint (ref offsetInBlock);
						LC c = handler.LoadedElements.GetClass (id);
						
						handler.ClassAllocationSummary (c, reachableInstances, reachableBytes, unreachableInstances, unreachableBytes);
						handler.DataProcessed (offsetInBlock);
					}
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					handler.AllocationSummaryEnd (collection, endCounter, microsecondsFromEpochToDateTime (endTime));
					handler.DataProcessed (offsetInBlock);
					//LogLine ("BLOCK HEAP_SUMMARY (END): endCounter {0}, endTime {1}", endCounter, endTime);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), 0);
					handler.DataProcessed (offsetInBlock);
					break;
				}
				case BlockCode.DIRECTIVES : {
					ulong startCounter = ReadUlong (ref offsetInBlock);
					ulong startTime = ReadUlong (ref offsetInBlock);
					handler.StartBlock (startCounter, microsecondsFromEpochToDateTime (startTime), 0);
					handler.DataProcessed (offsetInBlock);
					
					//LogLine ("BLOCK DIRECTIVES (START): startCounter {0}, startTime {1}", startCounter, startTime);
					DirectiveCodes directive = (DirectiveCodes) ReadUint (ref offsetInBlock);
					while (directive != DirectiveCodes.END) {
						switch (directive) {
						case DirectiveCodes.ALLOCATIONS_CARRY_CALLER:
							//LogLine ("BLOCK DIRECTIVES (START): ALLOCATIONS_CARRY_CALLER");
							handler.Directives.AllocationsCarryCallerMethodReceived ();
							break;
						case DirectiveCodes.ALLOCATIONS_HAVE_STACK:
							//LogLine ("BLOCK DIRECTIVES (START): ALLOCATIONS_HAVE_STACK");
							handler.Directives.AllocationsHaveStackTraceReceived ();
							break;
						case DirectiveCodes.ALLOCATIONS_CARRY_ID:
							//LogLine ("BLOCK DIRECTIVES (START): ALLOCATIONS_CARRY_ID");
							handler.Directives.AllocationsCarryIdReceived ();
							break;
						case DirectiveCodes.LOADED_ELEMENTS_CARRY_ID:
							//LogLine ("BLOCK DIRECTIVES (START): LOADED_ELEMENTS_CARRY_ID");
							handler.Directives.LoadedElementsCarryIdReceived ();
							break;
						case DirectiveCodes.CLASSES_CARRY_ASSEMBLY_ID:
							//LogLine ("BLOCK DIRECTIVES (START): CLASSES_CARRY_ASSEMBLY_ID");
							handler.Directives.ClassesCarryAssemblyIdReceived ();
							break;
						case DirectiveCodes.METHODS_CARRY_WRAPPER_FLAG:
							//LogLine ("BLOCK DIRECTIVES (START): METHODS_CARRY_WRAPPER_FLAG");
							handler.Directives.MethodsCarryWrapperFlagReceived ();
							break;
						default:
							throw new DecodingException (this, offsetInBlock, String.Format ("unknown directive {0}", directive));
						}
						
						directive = (DirectiveCodes) ReadUint (ref offsetInBlock);
					}
					
					ulong endCounter = ReadUlong (ref offsetInBlock);
					ulong endTime = ReadUlong (ref offsetInBlock);
					handler.EndBlock (endCounter, microsecondsFromEpochToDateTime (endTime), 0);
					handler.DataProcessed (offsetInBlock);
					//LogLine ("BLOCK DIRECTIVES (END): endCounter {0}, endTime {1}", endCounter, endTime);
					break;
				}
				default: {
					throw new DecodingException (this, offsetInBlock, String.Format ("unknown block code {0}", code));
				}
				}
				
				if (length != (int) offsetInBlock) {
					throw new DecodingException (this, offsetInBlock, String.Format ("Block ended at offset {0} but its declared length is {1}", offsetInBlock, length));
				}
			} catch (DecodingException e) {
				if (handleExceptions) {
					HandleDecodingException<LC,LM,UFR,UFI,MR,EH,HO,HS> (e, offsetInBlock, handler);
				}
				throw e;
			} catch (Exception e) {
				if (handleExceptions) {
					HandleRegularException<LC,LM,UFR,UFI,MR,EH,HO,HS> (e, offsetInBlock, handler);
				}
				throw new DecodingException(this, offsetInBlock, e.Message, e);
			}
		}
		
		bool handleExceptions = true;
		
		void DumpDebugInformation<LC,LM,UFR,UFI,MR,EH,HO,HS> (uint offsetInBlock, IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> handler) where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
			if (debugLog != null) {
				debugLog.WriteLine ("Attempting to decode data printing block contents...");
				handleExceptions = false;
				try {
					DebugProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> debugHandler =
						new DebugProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> (handler, debugLog);
					debugLog.WriteLine ("Current block of type {0} (file offset {1}, length {2})", Code, FileOffset, Length);
					Decode (debugHandler);
					
				} catch (Exception e) {
					debugLog.WriteLine ("While attempting decoding, got exception {0}", e.Message);
					DumpData (data, debugLog, offsetInBlock - 8, offsetInBlock + 24);
				} finally {
					handleExceptions = true;
				}
			}
		}
		void HandleDecodingException<LC,LM,UFR,UFI,MR,EH,HO,HS> (DecodingException e, uint offsetInBlock, IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> handler) where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
			if (debugLog != null) {
				debugLog.WriteLine ("ERROR: DecodingException in block of code {0}, length {1}, file offset {2}, block offset {3}: {4}", e.FailingData.Code, e.FailingData.Length, e.FailingData.FileOffset, e.OffsetInBlock, e.Message);
				debugLog.WriteLine (e.StackTrace);
				if (e.InnerException != null) {
					debugLog.WriteLine ("Original stack trace:");
					debugLog.WriteLine (e.InnerException.StackTrace);
				}
				DumpDebugInformation<LC,LM,UFR,UFI,MR,EH,HO,HS> (offsetInBlock, handler);
			}
			
			throw e;
		}
		void HandleRegularException<LC,LM,UFR,UFI,MR,EH,HO,HS> (Exception e, uint offsetInBlock, IProfilerEventHandler<LC,LM,UFR,UFI,MR,EH,HO,HS> handler) where LC : ILoadedClass where LM : ILoadedMethod<LC> where UFR : IUnmanagedFunctionFromRegion<UFR> where UFI : IUnmanagedFunctionFromID<MR,UFR> where MR : IExecutableMemoryRegion<UFR> where EH : ILoadedElementHandler<LC,LM,UFR,UFI,MR,HO,HS> where HO: IHeapObject<HO,LC> where HS: IHeapSnapshot<HO,LC> {
			if (debugLog != null) {
				debugLog.WriteLine ("ERROR: Exception in block of code {0}, length {1}, file offset {2}, block offset {3}: {4}", Code, Length, FileOffset, offsetInBlock, e.Message);
				debugLog.WriteLine (e.StackTrace);
				DumpDebugInformation<LC,LM,UFR,UFI,MR,EH,HO,HS> (offsetInBlock, handler);
			}
			
			throw e;
		}
		
		static public void DumpData (byte[] data, TextWriter output, uint startOffset, uint endOffset) {
			uint currentIndex = 0;
			while (startOffset < endOffset) {
				if (currentIndex % 8 == 0) {
					if ((currentIndex != 0)) {
						output.WriteLine ();
					}
					output.Write ("  [{0}-{1}]", startOffset, startOffset + 7);
				}
				output.Write (" ");
				output.Write (data [startOffset]);
				
				startOffset ++;
				currentIndex ++;
			}
			output.WriteLine ();
		}
		
		public BlockData (uint fileOffset, BlockCode code, int length, ulong headerStartCounter, byte[] data) {
			this.fileOffset = fileOffset;
			this.code = code;
			this.length = length;
			this.headerStartCounter = headerStartCounter;
			this.data = data;
		}
	}

	
	public class FileOperationException : System.Exception {
		public FileOperationException (string message) : base (message) {
		}
		public FileOperationException (System.Exception e) :
		base (String.Format ("Exception {0}: {1}", e.GetType().FullName, e.Message)) {
		}
	}
	
	public class DecodingException : System.Exception {
		BlockData data;
		public BlockData FailingData {
			get {
				return data;
			}
		}
		
		uint offsetInBlock;
		public uint OffsetInBlock {
			get {
				return offsetInBlock;
			}
		}
		
		public DecodingException (BlockData data, uint offsetInBlock, string message) : base (message) {
			this.data = data;
			this.offsetInBlock = offsetInBlock;
		}
		public DecodingException (BlockData data, uint offsetInBlock, string message, Exception cause) : base (message, cause) {
			this.data = data;
			this.offsetInBlock = offsetInBlock;
		}
	}
}
