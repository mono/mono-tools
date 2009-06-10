//
// Gendarme.Rules.Concurrency.DecorateThreadsRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Gendarme.Rules.Concurrency {
	
	/// <summary>
	/// This rule is designed to help you precisely specify the threading semantics supported
	/// by your code. This is valuable because it forces you to think clearly about the semantics
	/// required of the code, the semantics are explicitly visible in the code, and the rule verifies
	///  that the specification remains consistent.
	///
	/// In order to do this the rule relies on an attribute which allows you to declare that your
	/// code can only run under the main thread, that it can run under an arbitrary thread,
	/// that it can run under multiple threads if the execution is serialized, or that the code
	/// is fully concurrent.
	///
	/// The rule enforces the following constraints:
	/// <list>
	/// <item>Thread entry points cannot be main thread.</item>
	/// <item>MainThread code can call everything, AllowEveryCaller code can be called by 
	/// everything, SingleThread can call SingleThread/Serializable/Concurrent, and Serializable/
	/// Concurrent can call Serializable/Concurrent.</item>
	/// <item>Delegates must be able to call the methods they are bound to.</item>
	/// <item>An override of a base method or an implementation of an interface method must 
	/// use the same threading model as the original method.</item>
	/// <item>A delegate used with a threaded event must use the same threading model as the 
	/// event.</item>
	/// <item>Serializable cannot be applied to static methods and static methods of serializeable 
	/// types do not inherit it from their types. (The rationale here is that there is normally nothing 
	/// that can be used to serialize access to static methods other  than the type which is a bad 
	/// idea, see [http://bytes.com/groups/net-c/249277-dont-lock-type-objects]).</item>
	/// </list>
	///
	/// When adding the attributes to a non-trivial amount of threaded code it seems best to focus
	/// on one thread at a time so that it is easier to understand how the methods interact and which
	/// threading model needs to be used by them. While doing this the defects for the other threads
	/// can be temporarily suppressed using gendarme's --ignore switch.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// internal sealed class Wrapper : IDisposable
	/// {
	/// 	// Finalizers execute from a worker thread so the rule will complain
	/// 	// if they are main thread.
	/// 	~Wrapper ()
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
	/// 	private void Dispose (bool disposing)
	/// 	{
	/// 		if (!Disposed) {
	/// 			Disposed = true;
	/// 		}
	/// 	}
	/// 	
	/// 	private bool Disposed { get; set; }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public enum ThreadModel {
	/// 	// The code may run safely only under the main thread (this is the 
	/// 	// default for code in the assemblies being checked).
	/// 	MainThread = 0x0000,
	/// 	
	/// 	// The code may run under a single arbitrary thread.
	/// 	SingleThread = 0x0001,
	/// 	
	/// 	// The code may run under multiple threads, but only if the 
	/// 	// execution is serialized (e.g. by user level locking).
	/// 	Serializable = 0x0002,
	/// 	
	/// 	// The code may run under multiple threads concurrently without user 
	/// 	// locking (this is the default for code in the System/Mono namespaces).
	/// 	Concurrent = 0x0003,
	/// 	
	/// 	// Or this with the above for the rare cases where the code cannot be
	/// 	// shown to be correct using a static analysis.
	/// 	AllowEveryCaller = 0x0008,
	/// }
	/// 
	/// // This is used to precisely specify the threading semantics of code. Note 
	/// // that Gendarme&apos;s DecorateThreadsRule will catch problematic code which 
	/// // uses these attributes (such as concurrent code calling main thread code).
	/// [Serializable]
	/// [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | 
	/// AttributeTargets.Interface | AttributeTargets.Delegate | 
	/// AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property,
	/// AllowMultiple = false, Inherited = false)]
	/// public sealed class ThreadModelAttribute : Attribute {
	/// 	public ThreadModelAttribute (ThreadModel model)
	/// 	{
	/// 		Model = model;
	/// 	}
	/// 	
	/// 	public ThreadModel Model { get; set; }
	/// }
	///
	/// internal sealed class Wrapper : IDisposable
	/// {
	/// 	[ThreadModel (ThreadModel.SingleThread)]
	/// 	~Wrapper ()
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
	/// 	// This is called from both the finalizer thread and the main thread
	/// 	// so it must be decorated. But it only executes under one thread
	/// 	// at a time so we can use SingleThread instead of Concurrent.
	/// 	[ThreadModel (ThreadModel.SingleThread)]
	/// 	private void Dispose (bool disposing)
	/// 	{
	/// 		if (!Disposed) {
	/// 			Disposed = true;
	/// 		}
	/// 	}
	/// 	
	/// 	// This is called from a threaded method so it must also be
	/// 	// threaded.
	/// 	[ThreadModel (ThreadModel.SingleThread)]
	/// 	private bool Disposed { get; set; }
	/// }
	/// </code>
	/// </example>
	[Problem ("Threaded code is not decorated as threaded, a threading attribute is improperly used, or the assembly does not use ThreadModelAttribute.")]
	[Solution ("Use the correct threading attribute or disable the defect.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DecorateThreadsRule : Rule, IMethodRule {
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);
			
			runner.AnalyzeAssembly += this.OnAssembly;
			
			DefectCount = 0;
			checked_entry_points.Clear ();
			anonymous_entry_points.Clear ();
		}
		
		public void OnAssembly (object sender, RunnerEventArgs e)
		{
			// If the assembly defines ThreadModelAttribute then we need to
			// check all of the methods within it.
			foreach (ModuleDefinition module in e.CurrentAssembly.Modules) {
				foreach (TypeDefinition type in module.Types) {
					if (type.Name == "ThreadModelAttribute") {
						Active = true;
						Log.WriteLine (this, "assembly defines ThreadModelAttribute");
						return;
					}
				}
				
				foreach (TypeReference type in module.TypeReferences) {
					if (type.Name == "ThreadModelAttribute") {
						Active = true;
						Log.WriteLine (this, "assembly references ThreadModelAttribute");
						return;
					}
				}
			}
			
			// If the assembly does not define ThreadModelAttribute then we don't
			// want to check methods but we do want to report (one) defect to inform
			// people about the rule.
			Active = false;
			
			if (!displayed_no_attribute_defect) {
				string mesg = "The assembly does not use ThreadModelAttribute (this defect will be reported only once).";
				Log.WriteLine (this, mesg);
				ReportDefect (e.CurrentAssembly, Severity.Medium, Confidence.Low, mesg);
				
				displayed_no_attribute_defect = true;
			}
		}
		
		public int DefectCount { get; private set; }
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (ThreadRocks.ThreadedNamespace (method.DeclaringType.Namespace))
				return RuleResult.DoesNotApply;
			
			Log.WriteLine (this);
			Log.WriteLine (this, "---------------------------------------");
			Log.WriteLine (this, method);
			
			// Finalizers need to be single threaded.
			ThreadModelAttribute model = method.ThreadingModel ();
			if (method.IsFinalizer ()) {
				if (model.Model != ThreadModel.SingleThread) {
					string mesg = "Finalizers should be decorated with [ThreadModel (ThreadModel.SingleThreaded)].";
					ReportDefect (method, Severity.High, Confidence.High, mesg);
				}
			}
			
			// Make sure all of the thread entry points are properly decorated and
			// that all calls are legit.
			if (method.HasBody && opcodes_mask.Intersect (OpCodeEngine.GetBitmask (method)))
				CheckMethodBody (method);
			
			// A delegate used with a threaded event must use the same threading model 
			// as the event.
			if (method.IsAddOn) {
				ParameterDefinition p = method.Parameters [0];
				TypeDefinition delegateType = p.ParameterType.Resolve ();
				if (delegateType != null && !ThreadRocks.ThreadedNamespace (delegateType.Namespace)) {
					ThreadModelAttribute delegateModel = delegateType.ThreadingModel ();
					if (model != delegateModel && !delegateModel.AllowsEveryCaller) {
						string mesg = string.Format ("{0} event must match {1} delegate.", model, delegateModel);
						ReportDefect (method, Severity.High, Confidence.High, mesg);
					}
				}
			}
			
			// An override of a base method or an implementation of an interface method
			// must use the same threading model as the original method.
			if (method.IsVirtual) {
				IEnumerable<TypeDefinition> superTypes = method.DeclaringType.AllSuperTypes ();
				if (method.IsNewSlot)
					superTypes = from s in superTypes where s.IsInterface select s;
				else
					superTypes = from s in superTypes where !s.IsInterface select s;
				
				foreach (TypeDefinition type in superTypes) {
					MethodDefinition superMethod = type.Methods.GetMethod (method.Name, method.Parameters);
					if (superMethod != null && !ThreadRocks.ThreadedNamespace (superMethod.DeclaringType.Namespace)) {
						ThreadModelAttribute superModel = superMethod.ThreadingModel ();
						if (model != superModel) {
							string mesg;
							if (method.IsNewSlot)
								mesg = string.Format ("{0} {1} must match {2} interface method.", model, method.Name, superModel);
							else
								mesg = string.Format ("{0} {1} must match {2} base method.", model, method.Name, superModel);
							ReportDefect (method, Severity.High, Confidence.High, mesg);
						}
					}
				}
			}
			
			// Serializable cannot be applied to static methods, but can be applied to
			// operators because they're just sugar for normal calls.
			if (method.IsStatic && model.Model == ThreadModel.Serializable && !method.Name.StartsWith ("op_")) {
				string mesg = "Static members cannot be decorated with Serializable.";
				ReportDefect (method, Severity.High, Confidence.High, mesg);
			}
			
			return Runner.CurrentRuleResult;
		}
		
		public override void TearDown ()
		{
			// We don't always know that an anonymous method was used as a thread
			// entry point when we check the method so we check them just before we
			// tear down.
			foreach (MethodDefinition caller in anonymous_entry_points) {
				foreach (Instruction ins in caller.Body.Instructions) {
					switch (ins.OpCode.Code) {
						case Code.Call:
						case Code.Callvirt:
							MethodDefinition target = ((MethodReference) ins.Operand).Resolve ();
							if (target != null) {
								ThreadModelAttribute targetModel = target.ThreadingModel ();
								if (targetModel.Model == ThreadModel.MainThread && !targetModel.AllowsEveryCaller) {
									string mesg = string.Format ("An anonymous thread entry point cannot call MainThread {0}.", target.Name);
									
									++DefectCount;
									Log.WriteLine (this, "Defect: {0}", mesg);
									Defect defect = new Defect (this, caller, caller, ins, Severity.High, Confidence.High, mesg);
									Runner.Report (defect);
								}
							}
							break;
					}
				}
			}
			
			base.TearDown ();
		}
		
		#region Private Methods
		private void CheckMethodBody (MethodDefinition method)
		{
			var synchronizedEvents = new Dictionary<MethodReference, List<MethodReference>> ();
			var thisSynchronized = new List<TypeReference> ();
			
			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference candidate = null;
				
				switch (ins.OpCode.Code) {
				case Code.Newobj:
					if (ins.Previous != null && ins.Previous.OpCode.Code == Code.Ldftn) {
						MethodReference ctor = (MethodReference) ins.Operand;
						
						if (ctor.DeclaringType.IsDelegate ()) {
							// ldftn entry-point
							// newobj System.Void System.Threading.XXX::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a System.Threading delegate
							if (ctor.DeclaringType.Namespace == "System.Threading") {
								if (ctor.DeclaringType.Name == "ThreadStart" ||
									ctor.DeclaringType.Name == "ParameterizedThreadStart" ||
									ctor.DeclaringType.Name == "WaitCallback" ||
									ctor.DeclaringType.Name == "WaitOrTimerCallback" ||
									ctor.DeclaringType.Name == "TimerCallback") {
										candidate = (MethodReference) ins.Previous.Operand;
								}
							
							// ldftn entry-point
							// newobj System.Void System.AsyncCallback::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a async delegate
							} else if (ctor.DeclaringType.Namespace == "System") {
								if (ctor.DeclaringType.Name == "AsyncCallback") {
									candidate = (MethodReference) ins.Previous.Operand;
								}
							
							// ldftn entry-point
							// newobj System.Void ThreadedDelegate::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a delegate which is decorated with a threading attribute
							} else if (!ThreadRocks.ThreadedNamespace (ctor.DeclaringType.Namespace)) {
								// Delegates must be able to call the methods they are bound to.
								MethodDefinition target = ((MethodReference) ins.Previous.Operand).Resolve ();
								if (target != null) {
									ThreadModelAttribute callerModel = ctor.DeclaringType.ThreadingModel ();
									if (!target.IsGeneratedCode () || target.IsProperty ()) {
										ThreadModelAttribute targetModel = target.ThreadingModel ();
										if (!IsValidCall (callerModel, targetModel)) {
											string mesg = string.Format ("{0} delegate cannot be bound to {1} {2} method.", callerModel, targetModel, target.Name);
											
											++DefectCount;
											Log.WriteLine (this, "Defect: {0}", mesg);
											Defect defect = new Defect (this, method, method, ins, Severity.High, Confidence.High, mesg);
											Runner.Report (defect);
										}
										
									} else if (callerModel.Model != ThreadModel.MainThread) {
										anonymous_entry_points.Add (target);
									}
								}
							}
						}
					}
					break;
				
				case Code.Call:
				case Code.Callvirt:
					if (!method.IsGeneratedCode () || method.IsProperty ())
						CheckForLegalCall (method, ins);
					
					// ldftn entry-point
					// newobj XXX
					// callvirt System.Void SynchronizedType::add_Name (XXX)	
					// i.e. adding a delegate to an event in a type which uses SynchronizingObject
					MethodReference call = (MethodReference) ins.Operand;
					if (ins.Previous != null && ins.Previous.Previous != null && ins.Previous.OpCode.Code == Code.Newobj && ins.Previous.Previous.OpCode.Code == Code.Ldftn) {
						// A few events are blacklisted because they do not use SynchronizingObject and
						// are therefore always threaded.
						if (non_synchronized_setters.Any (s => s.Matches (call))) {
							candidate = (MethodReference) ins.Previous.Previous.Operand;
						
						// But most events do use SynchronizingObject and therefore their threading
						// depends on whether and how SynchronizingObject is initialized.
						} else if (HasSynchronizingObject (call.DeclaringType)) {
							List<MethodReference> methods;
							if (!synchronizedEvents.TryGetValue (call, out methods)) {
								methods = new List<MethodReference> ();
								synchronizedEvents.Add (call, methods);
							}
							
							methods.AddIfNew ((MethodReference) ins.Previous.Previous.Operand);
						
						// Misc threaded events.
						} else if (call.DeclaringType.FullName == "System.ComponentModel.BackgroundWorker") {
							if (call.Name == "add_DoWork") {
								candidate = (MethodReference) ins.Previous.Previous.Operand;
							}
						}
					
					// callvirt System.Void System.Diagnostics.Process::set_SynchronizingObject (System.ComponentModel.ISynchronizeInvoke)
					} else if (SetSynchronizingObject.Matches (call)) {
						if (ins.Previous.OpCode.Code == Code.Ldarg_0) {
							thisSynchronized.Add (call.DeclaringType);
						}
					}
					break;
				}
				
				if (candidate != null) {
					Log.WriteLine (this, "{0} is a thread entry point", candidate);
					CheckEntryPoint (candidate);
				}
			}
			
			// For every method added to a threaded event,
			foreach (KeyValuePair<MethodReference, List<MethodReference>> entry in synchronizedEvents) {
				// if the event is synchronized on this then the target must have the same thread
				// as the current method's type or better and it should not be treated as a entry point.
				if (thisSynchronized.Contains (entry.Key.DeclaringType)) {
					ThreadModelAttribute callerModel = method.DeclaringType.ThreadingModel ();
					foreach (MethodReference mr in entry.Value) {
						MethodDefinition target = mr.Resolve ();
						if (target != null) {
							ThreadModelAttribute targetModel = target.ThreadingModel ();
							if (!IsValidCall (callerModel, targetModel)) {
								string mesg = string.Format ("{0} {1} cannot be bound to {2} {3} method.", callerModel, entry.Key, targetModel, target.Name);
								ReportDefect (method, Severity.High, Confidence.High, mesg);
							}
						}
					}
				
				// otherwise the method has to be treated as a thread entry point.
				} else {
					foreach (MethodReference mr in entry.Value) {
						Log.WriteLine (this, "{0} is a thread entry point", mr);
						CheckEntryPoint (mr);
					}
				}
			}
		}
		
		private bool HasSynchronizingObject (TypeReference tr)
		{
			foreach (TypeDefinition type in tr.AllSuperTypes ()) {
				if (type.GetMethod (SetSynchronizingObject) != null)
					return true;
			}
			
			return false;
		}
		
		// Thread entry points cannot be main thread or an anonymous
		// method (but not an auto-property).
		private void CheckEntryPoint (MethodReference mr)
		{
			if (!checked_entry_points.Contains (mr)) {
				checked_entry_points.Add (mr);
				
				MethodDefinition method = mr.Resolve ();
				if (method != null) {
					ThreadModelAttribute model = method.ThreadingModel ();
					
					if (method.IsGeneratedCode () && !method.IsProperty ()) {
						anonymous_entry_points.Add (method);
					
					} else if (model.Model == ThreadModel.MainThread && !model.AllowsEveryCaller) {
						string mesg = string.Format ("{0} is a thread entry point and so cannot be MainThread.", method.Name);
						ReportDefect (method, Severity.High, Confidence.High, mesg);
					}
				}
			}
		}
		
		private void CheckForLegalCall (MethodDefinition caller, Instruction ins)
		{
			MethodDefinition target = ((MethodReference) ins.Operand).Resolve ();
			if (target != null) {
				ThreadModelAttribute callerModel = caller.ThreadingModel ();
				ThreadModelAttribute targetModel = target.ThreadingModel ();
				if (!IsValidCall (callerModel, targetModel)) {
					string mesg = string.Format ("{0} {1} cannot call {2} {3}.", callerModel, caller.Name, targetModel, target.Name);
					
					++DefectCount;
					Log.WriteLine (this, "Defect: {0}", mesg);
					Defect defect = new Defect (this, caller, caller, ins, Severity.High, Confidence.High, mesg);
					Runner.Report (defect);
				}
			}
		}
		
		// MainThread code can call everything, AllowEveryCaller code can be called by 
		// everything, SingleThread can call SingleThread/Serializable/Concurrent, and Serializable/
		// Concurrent can call Serializable/Concurrent.
		private bool IsValidCall (ThreadModelAttribute caller, ThreadModelAttribute target)
		{
			bool valid = false;
			
			// MainThread code can call everything
			if (caller.Model == ThreadModel.MainThread)
				valid = true;
				
			// AllowEveryCaller code can be called by everything
			else if (target.AllowsEveryCaller)
				valid = true;
				
			// SingleThread can call SingleThread/Serializable/Concurrent
			else if (caller.Model == ThreadModel.SingleThread && (target.Model == ThreadModel.SingleThread || target.Model == ThreadModel.Serializable || target.Model == ThreadModel.Concurrent))
				valid = true;
			
			// Serializable/Concurrent can call Serializable/Concurrent
			else if ((caller.Model == ThreadModel.Serializable || caller.Model == ThreadModel.Concurrent) && 
				(target.Model == ThreadModel.Serializable || target.Model == ThreadModel.Concurrent))
				valid = true;
			
			return valid;
		}
		
		// We use this little helper so that we can report a better defect count to the test.
		// (The test itself can't quite manage this because it can't tell what happened
		// inside TearDown if CheckType or CheckMethod failed).
		private void ReportDefect (IMetadataTokenProvider metadata, Severity severity, Confidence confidence, string mesg)
		{
			++DefectCount;
			Log.WriteLine (this, "Defect: {0}", mesg);
			
			// We need to use the Defect Report overload because the runner's current
			// target won't be set if we land here via TearDown.
			Defect defect = new Defect (this, metadata, metadata, severity, confidence, mesg);
			Runner.Report (defect);
		}
		#endregion
		
		#region Private Types
		private struct MethodName {
			public MethodName (string ns, string type, string name)
			{
				Namespace = ns;
				Type = type;
				Name = name;
			}
			
			public string Namespace { get; private set;}
			
			public string Type { get; private set;}
			
			public string Name { get; private set;}
			
			public bool Matches (MethodReference method)
			{
				if (Namespace != null && Namespace != method.DeclaringType.Namespace)
					return false;
				
				if (Type != null && Type != method.DeclaringType.Name)
					return false;
				
				if (Name != null && Name != method.Name)
					return false;
				
				return true;
			}
		}
		
#if false
		private void Bitmask ()
		{
			OpCodeBitmask opcodes_mask = new OpCodeBitmask ();
			
			opcodes_mask.Set (Code.Ldftn);
			opcodes_mask.Set (Code.Call);
			opcodes_mask.Set (Code.Callvirt);
			
			Console.WriteLine (opcodes_mask);
		}
#endif
		#endregion
		
		#region Fields
		private HashSet<MethodReference> checked_entry_points = new HashSet<MethodReference> ();
		private HashSet<MethodDefinition> anonymous_entry_points = new HashSet<MethodDefinition> ();
		private bool displayed_no_attribute_defect;
		
		private static readonly OpCodeBitmask opcodes_mask = new OpCodeBitmask (0x8000000000, 0x400000000000, 0x0, 0x20);
		private static readonly MethodSignature SetSynchronizingObject = new MethodSignature ("set_SynchronizingObject", "System.Void", new string [] { "System.ComponentModel.ISynchronizeInvoke" });
		
		// Note that MSDN does not say that FileSystemWatcher::add_Error uses SynchronizingObject,
		// but mono does.
		private static readonly List<MethodName> non_synchronized_setters = new List<MethodName> {
			new MethodName (null, null, "add_Disposed"),
			
			// MSDN is ambiguous about whether these are supposed to work with SynchronizingObject
			// but mono doesn't.
			new MethodName ("System.Diagnostics", "Process", "add_ErrorDataReceived"),
			new MethodName ("System.Diagnostics", "Process", "add_OutputDataReceived"),
		};
		#endregion
	}
}
