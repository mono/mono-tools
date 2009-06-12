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

namespace Mono.Profiler
{
	public class ConsoleDecoder {
		static void PrintSeparator (TextWriter writer) {
			writer.WriteLine ("\n\n------------------------------------------------");
		}
		
		static void PrintMethodAllocationsPerClass (TextWriter writer, LoadedClass.AllocationsPerMethod allocationsPerMethod, bool JitTime, bool printStackTraces, double stackTraceTreshold) {
			if (! JitTime) {
				writer.WriteLine ("        {0} bytes ({1} instances) from {2}.{3}", allocationsPerMethod.AllocatedBytes, allocationsPerMethod.AllocatedInstances, allocationsPerMethod.Method.Class.Name, allocationsPerMethod.Method.Name);
			} else {
				writer.WriteLine ("                {0} bytes ({1} instances) at JIT time in {2}.{3}", allocationsPerMethod.AllocatedBytes, allocationsPerMethod.AllocatedInstances, allocationsPerMethod.Method.Class.Name, allocationsPerMethod.Method.Name);
			}
			
			if (printStackTraces) {
				LoadedClass.AllocationsPerStackTrace [] stackTraces = allocationsPerMethod.StackTraces;
				Array.Sort (stackTraces, LoadedClass.AllocationsPerStackTrace.CompareByAllocatedBytes);
				Array.Reverse (stackTraces);
				double cumulativeAllocatedBytesPerStackTrace = 0;
				
				foreach (LoadedClass.AllocationsPerStackTrace trace in stackTraces) {
					if (cumulativeAllocatedBytesPerStackTrace / allocationsPerMethod.AllocatedBytes < stackTraceTreshold) {
						writer.WriteLine ("                {0} bytes ({1} instances) inside", trace.AllocatedBytes, trace.AllocatedInstances);
						for (StackTrace frame = trace.Trace; frame != null; frame = frame.Caller) {
							writer.Write ("                        ");
							if (frame.MethodIsBeingJitted) {
								writer.Write ("[JIT time]:");
							}
							writer.WriteLine ("{0}.{1}", frame.TopMethod.Class.Name, frame.TopMethod.Name);
						}
					} else {
						break;
					}
					cumulativeAllocatedBytesPerStackTrace += (double)trace.AllocatedBytes;
				}
			}
		}
		
		static void PrintClassAllocationData (TextWriter writer, ProfilerEventHandler data, LoadedClass c, ulong totalAllocatedBytes) {
			double allocatedBytesPerClass = (double)c.AllocatedBytes;
			writer.WriteLine ("{0,5:F2}% ({1} bytes) {2}", ((allocatedBytesPerClass / totalAllocatedBytes) * 100), c.AllocatedBytes, c.Name);
			
			if (data.Directives.AllocationsHaveStackTrace) {
				LoadedClass.AllocationsPerMethod[] allocationsPerMethodArray = c.Methods;
				double cumulativeAllocatedBytesPerMethod = 0;
				
				if (c.MethodsAtJitTimeCount > 0) {
					LoadedClass.AllocationsPerMethod[] allocationsPerMethodAtJitTime = c.MethodsAtJitTime;
					LoadedClass.AllocationsPerMethod[] totalAllocationsPerMethod = new LoadedClass.AllocationsPerMethod [allocationsPerMethodArray.Length + allocationsPerMethodAtJitTime.Length];
					Array.Copy (allocationsPerMethodArray, totalAllocationsPerMethod, allocationsPerMethodArray.Length);
					Array.Copy (allocationsPerMethodAtJitTime, 0, totalAllocationsPerMethod, allocationsPerMethodArray.Length, allocationsPerMethodAtJitTime.Length);
					allocationsPerMethodArray = totalAllocationsPerMethod;
				}
				
				if (allocationsPerMethodArray.Length != 0) {
					Array.Sort (allocationsPerMethodArray, LoadedClass.AllocationsPerMethod.CompareByAllocatedBytes);
					Array.Reverse (allocationsPerMethodArray);
					
					foreach (LoadedClass.AllocationsPerMethod allocationsPerMethod in allocationsPerMethodArray) {
						PrintMethodAllocationsPerClass (writer, allocationsPerMethod, false, cumulativeAllocatedBytesPerMethod < allocatedBytesPerClass * 0.7, 0.7);
						cumulativeAllocatedBytesPerMethod += (double)allocationsPerMethod.AllocatedBytes;
					}
				}
			} else {
				LoadedClass.AllocationsPerMethod[] allocationsPerMethodArray = c.Methods;
				if (allocationsPerMethodArray.Length != 0) {
					Array.Sort (allocationsPerMethodArray, LoadedClass.AllocationsPerMethod.CompareByAllocatedBytes);
					Array.Reverse (allocationsPerMethodArray);
					
					foreach (LoadedClass.AllocationsPerMethod allocationsPerMethod in allocationsPerMethodArray) {
						PrintMethodAllocationsPerClass (writer, allocationsPerMethod, false, false, 0);
					}
				}
				if (c.MethodsAtJitTimeCount > 0) {
					allocationsPerMethodArray = c.MethodsAtJitTime;
					Array.Sort (allocationsPerMethodArray, LoadedClass.AllocationsPerMethod.CompareByAllocatedBytes);
					Array.Reverse (allocationsPerMethodArray);
					foreach (LoadedClass.AllocationsPerMethod allocationsPerMethod in allocationsPerMethodArray) {
						PrintMethodAllocationsPerClass (writer, allocationsPerMethod, true, false, 0);
					}
				}
			}
			
		}
		
		static void PrintExecutionTimeByCallStack (TextWriter writer, ProfilerEventHandler data, StackTrace stackFrame, double callerSeconds, int indentationLevel) {
			for (int i = 0; i < indentationLevel; i++) {
				writer.Write ("    ");
			}
			LoadedMethod currentMethod = stackFrame.TopMethod;
			double currentSeconds = data.ClicksToSeconds (stackFrame.Clicks);
			writer.WriteLine ("{0,5:F2}% ({1:F6}s, {2} calls) {3}.{4}", ((currentSeconds / callerSeconds) * 100), currentSeconds, stackFrame.Calls, currentMethod.Class.Name, currentMethod.Name);
			foreach (StackTrace calledFrame in stackFrame.CalledFrames) {
				PrintExecutionTimeByCallStack (writer, data, calledFrame, currentSeconds, indentationLevel + 1);
			}
		}
		
		static void PrintData (TextWriter writer, ProfilerEventHandler data) {
			LoadedClass[] classes = data.LoadedElements.Classes;
			LoadedMethod[] methods = data.LoadedElements.Methods;
			IStatisticalHitItem[] statisticalHitItems = data.StatisticalHitItems;
			
			if ((data.Flags & ProfilerFlags.CLASS_EVENTS) != 0) {
				Array.Sort (classes, LoadedClass.CompareByAllocatedBytes);
				Array.Reverse (classes);
				ulong totalAllocatedBytes = 0;
				foreach (LoadedClass c in classes) {
					totalAllocatedBytes += c.AllocatedBytes;
				}
				if (totalAllocatedBytes > 0) {
					PrintSeparator (writer);
					writer.WriteLine ("Reporting allocations (on {0} classes)", classes.Length);
					foreach (LoadedClass c in classes) {
						if (c.AllocatedBytes > 0) {
							PrintClassAllocationData (writer, data, c, totalAllocatedBytes);
						}
					}
				} else {
					writer.WriteLine ("No allocations reported (on {0} classes)", classes.Length);
				}
			}
			if ((data.Flags & ProfilerFlags.METHOD_EVENTS) != 0) {
				Array.Sort (methods, LoadedMethod.CompareByTotalClicks);
				Array.Reverse (methods);
				ulong totalExecutionClicks = 0;
				foreach (LoadedMethod m in methods) {
					totalExecutionClicks += m.Clicks;
				}
				if (totalExecutionClicks > 0) {
					PrintSeparator (writer);
					writer.WriteLine ("Reporting execution time (on {0} methods)", methods.Length);
					foreach (LoadedMethod m in methods) {
						if (m.Clicks > 0) {
							writer.WriteLine ("{0,5:F2}% ({1:F6}s) {2}.{3}", ((((double)m.Clicks) / totalExecutionClicks) * 100), data.ClicksToSeconds (m.Clicks), m.Class.Name, m.Name);
							LoadedMethod.CallsPerCallerMethod[] callsPerCallerMethodArray = m.Callers;
							if (callsPerCallerMethodArray.Length > 0) {
								Array.Sort (callsPerCallerMethodArray, LoadedMethod.CallsPerCallerMethod.CompareByCalls);
								Array.Reverse (callsPerCallerMethodArray);
								foreach (LoadedMethod.CallsPerCallerMethod callsPerCallerMethod in callsPerCallerMethodArray) {
									writer.WriteLine ("        {0} calls from {1}.{2}", callsPerCallerMethod.Calls, callsPerCallerMethod.Callees.Class.Name, callsPerCallerMethod.Callees.Name);
								}
							}
						}
					}
					
					PrintSeparator (writer);
					writer.WriteLine ("Reporting execution time by stack frame");
					foreach (StackTrace rootFrame in data.RootFrames) {
						PrintExecutionTimeByCallStack (writer, data, rootFrame, data.ClicksToSeconds (totalExecutionClicks), 0);
					}
				} else {
					writer.WriteLine ("No execution time reported (on {0} methods)", methods.Length);
				}
			}
			if ((data.Flags & ProfilerFlags.JIT_COMPILATION) != 0) {
				Array.Sort (methods, LoadedMethod.CompareByJitClicks);
				Array.Reverse (methods);
				ulong totalJitClicks = 0;
				foreach (LoadedMethod m in methods) {
					totalJitClicks += m.JitClicks;
				}
				if (totalJitClicks > 0) {
					PrintSeparator (writer);
					writer.WriteLine ("Reporting jit time (on {0} methods)", methods.Length);
					foreach (LoadedMethod m in methods) {
						if (m.JitClicks > 0) {
							writer.WriteLine ("{0,5:F2}% ({1:F3}ms) {2}.{3}", ((((double)m.JitClicks) / totalJitClicks) * 100), data.ClicksToSeconds (m.JitClicks) * 1000, m.Class.Name, m.Name);
						}
					}
				} else {
					writer.WriteLine ("No jit time reported (on {0} methods)", methods.Length);
				}
			}
			if ((data.Flags & ProfilerFlags.STATISTICAL) != 0) {
				Array.Sort (statisticalHitItems, StatisticalHitItemCallCounts.CompareByStatisticalHits);
				Array.Reverse (statisticalHitItems);
				ulong totalHits = 0;
				foreach (IStatisticalHitItem s in statisticalHitItems) {
					totalHits += s.StatisticalHits;
				}
				if (totalHits > 0) {
					PrintSeparator (writer);
					writer.WriteLine ("Reporting statistical hits ({0} hits recorded)", totalHits);
					foreach (IStatisticalHitItem s in statisticalHitItems) {
						if ((s.StatisticalHits > 0) || s.HasCallCounts) {
							writer.WriteLine ("{0,5:F2}% ({1}) {2}", ((((double)s.StatisticalHits) / totalHits) * 100), s.StatisticalHits, s.Name);
							if (s.HasCallCounts) {
								StatisticalHitItemCallCounts callCounts = s.CallCounts;
								if (callCounts.CallersCount > 0) {
									StatisticalHitItemCallInformation[] calls = callCounts.Callers;
									foreach (StatisticalHitItemCallInformation call in calls) {
										writer.WriteLine ("    {0} calls from {1}", call.Calls, call.Item.Name);
									}
								}
								if (callCounts.CalleesCount > 0) {
									StatisticalHitItemCallInformation[] calls = callCounts.Callees;
									foreach (StatisticalHitItemCallInformation call in calls) {
										writer.WriteLine ("    {0} calls to {1}", call.Calls, call.Item.Name);
									}
								}
							}
						}
					}
				} else {
					writer.WriteLine ("No statistical hits reported (on {0} items)", statisticalHitItems.Length);
				}
			}
			
			if (data.GlobalMonitorStatistics.ContainsData) {
				PrintSeparator (writer);
				writer.WriteLine ("Reporting monitor statistics.");
				data.GlobalMonitorStatistics.WriteStatistics (writer, data);
			}
			
			ProfilerEventHandler.GcStatistics[] gcStatistics = data.GarbageCollectioncStatistics;
			if (gcStatistics.Length > 0) {
				double totalTime = data.ClicksToSeconds (data.EndCounter - data.StartCounter);
				double gcTime = 0;
				double gcMarkTime = 0;
				double gcSweepTime = 0;
				int collections = 0;
				foreach (ProfilerEventHandler.GcStatistics gcs in gcStatistics) {
					if (gcs.NewHeapSize == null) {
						collections ++;
						gcTime += gcs.Duration;
						gcMarkTime += gcs.MarkDuration;
						gcSweepTime += gcs.SweepDuration;
					}
				}
				
				PrintSeparator (writer);
				writer.WriteLine ("Reporting GC statistics for {0} collections (total {1:F3}ms, {2,5:F2}% of total time, mark {3,5:F2}%, sweep {4,5:F2}%)",
				                  collections,
				                  gcTime * 1000,
				                  (gcTime / totalTime) * 100,
				                  (gcMarkTime / gcTime) * 100,
				                  (gcSweepTime / gcTime) * 100);
				foreach (ProfilerEventHandler.GcStatistics gcs in gcStatistics) {
					if (gcs.NewHeapSize == null) {
						ulong gcStartClicks = gcs.StartCounter - data.StartCounter;
						writer.WriteLine ("[{0}] Collection starting at {1:F3}s (generation {2}): duration {3:F3}ms, mark {4:F3}ms, sweep {5:F3}ms",
						                  gcs.Collection,
						                  data.ClicksToSeconds (gcStartClicks),
						                  gcs.Generation,
						                  gcs.Duration * 1000,
						                  gcs.MarkDuration * 1000,
						                  gcs.SweepDuration * 1000);
					} else {
						writer.WriteLine ("[{0}] Heap resized to {1} bytes", gcs.Collection, gcs.NewHeapSize);
					}
				}
			}
			
			AllocationSummary [] allocationSummaries = data.AllocationSummaries;
			if (allocationSummaries.Length > 0) {
				PrintSeparator (writer);
				writer.WriteLine ("Reporting allocation summaries for {0} collections", allocationSummaries.Length);
				foreach (AllocationSummary allocationSummary in allocationSummaries) {
					writer.WriteLine ("Data for collection {0} written {1:F3}s since the application started",
					                  allocationSummary.Collection, ((double) (allocationSummary.StartTime - data.StartTime).Milliseconds) / 1000);
					AllocationClassData<LoadedClass>[] classData = allocationSummary.Data;
					foreach (AllocationClassData<LoadedClass> cData in classData) {
						writer.WriteLine ("  Class {0}: {1} bytes in {2} instances (freed: {3} bytes in {4} instances)",
						                  cData.Class.Name,
						                  cData.ReachableBytes,
						                  cData.ReachableInstances,
						                  cData.UnreachableBytes,
						                  cData.UnreachableInstances);
					}
				}
			}
			
			HeapSnapshot[] heapSnapshots = data.LoadedElements.HeapSnapshots;
			if (heapSnapshots.Length > 0) {
				PrintSeparator (writer);
				writer.WriteLine ("Reporting heap data for {0} collections", heapSnapshots.Length);
				foreach (HeapSnapshot heapSnapshot in heapSnapshots) {
					HeapSnapshot.AllocationStatisticsPerClass [] allocationStatistics = heapSnapshot.AllocationStatistics;
					writer.WriteLine ("Heap data collection {0} started at {1} (duration {2:F3}ms)",
					                  heapSnapshot.Collection,
					                  data.CounterToDateTime (heapSnapshot.StartCounter),
					                  data.ClicksToSeconds (heapSnapshot.EndCounter - heapSnapshot.StartCounter) * 1000);
					if (allocationStatistics.Length > 0) {
						Array.Sort (allocationStatistics, HeapSnapshot.AllocationStatisticsPerClass.CompareByAllocatedBytes);
						Array.Reverse (allocationStatistics);
						uint totalAllocatedBytes = 0;
						foreach (HeapSnapshot.AllocationStatisticsPerClass s in allocationStatistics) {
							totalAllocatedBytes += s.AllocatedBytes;
						}
						foreach (HeapSnapshot.AllocationStatisticsPerClass s in allocationStatistics) {
							if (s.AllocatedBytes > 0) {
								writer.WriteLine ("    {0,5:F2}% {1} {2} bytes ({3} freed)", ((((double)s.AllocatedBytes) / totalAllocatedBytes) * 100), s.Class.Name, s.AllocatedBytes, s.FreedBytes);
							}
						}
					} else {
						writer.WriteLine ("No allocation statistics for this collection)", statisticalHitItems.Length);
					}
				}
			}
		}
		
		static void Main (string[] argv) {
			BlockData.DebugLog = Console.Out;
			
			if (argv.Length != 1) {
				Console.WriteLine ("Please specify one input file");
				return;
			}
			SyncLogFileReader reader = new SyncLogFileReader (argv [0]);
			ProfilerEventHandler data = new ProfilerEventHandler ();
			data.LoadedElements.RecordHeapSnapshots = false;
			while (! reader.HasEnded) {
				BlockData currentBlock = null;
				try {
					currentBlock = reader.ReadBlock ();
					currentBlock.Decode (data, reader);
				} catch (DecodingException e) {
					Console.WriteLine ("Stopping decoding after a DecodingException in block of code {0}, length {1}, file offset {2}, block offset {3}: {4}", e.FailingData.Code, e.FailingData.Length, e.FailingData.FileOffset, e.OffsetInBlock, e.Message);
					break;
				}
			}
			
			PrintData (Console.Out, data);
		}
	}
}
