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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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

		static bool LookForThreadModelAttribute (IEnumerable list)
		{
			foreach (TypeReference type in list) {
				if (type.Name == "ThreadModelAttribute") {
					return true;
				}
			}
			return false;
		}

		public void OnAssembly (object sender, RunnerEventArgs e)
		{
			// If the assembly defines ThreadModelAttribute then we need to
			// check all of the methods within it.
			foreach (ModuleDefinition module in e.CurrentAssembly.Modules) {
				if (LookForThreadModelAttribute (module.GetAllTypes ())) {
					Log.WriteLine (this, "assembly defines ThreadModelAttribute");
					Active = true;
					return;
				} else if (LookForThreadModelAttribute (module.GetTypeReferences ())) {
					Log.WriteLine (this, "assembly references ThreadModelAttribute");
					Active = true;
					return;
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

			string name = method.Name;
			IList<ParameterDefinition> pdc = method.HasParameters ? method.Parameters : null;

			// Finalizers need to be single threaded.
			ThreadModel model = method.ThreadingModel ();
			if (method.IsFinalizer ()) {
				if ((model & ~ThreadModel.AllowEveryCaller) != ThreadModel.SingleThread) {
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
				ParameterDefinition p = pdc [0];
				TypeDefinition delegateType = p.ParameterType.Resolve ();
				if (delegateType != null && !ThreadRocks.ThreadedNamespace (delegateType.Namespace)) {
					ThreadModel delegateModel = delegateType.ThreadingModel ();
					if (model != delegateModel && !delegateModel.AllowsEveryCaller ()) {
						string mesg = String.Format (CultureInfo.InvariantCulture, 
							"{0} event must match {1} delegate.", model, delegateModel);
						ReportDefect (method, Severity.High, Confidence.High, mesg);
					}
				}
			}

			// An override of a base method or an implementation of an interface method
			// must use the same threading model as the original method.
			if (method.IsVirtual) {
				IEnumerable<TypeDefinition> superTypes = method.DeclaringType.AllSuperTypes ();
				bool new_slot = method.IsNewSlot;
				superTypes = from s in superTypes where (s.IsInterface == new_slot) select s;
				string [] parameters = pdc != null
					? (from p in pdc.Cast<ParameterDefinition> () select p.ParameterType.GetFullName ()).ToArray ()
					: null;

				string return_type_name = method.ReturnType.GetFullName ();
				foreach (TypeDefinition type in superTypes) {
					MethodDefinition superMethod = type.GetMethod (name, return_type_name, parameters);
					if (superMethod != null && !ThreadRocks.ThreadedNamespace (superMethod.DeclaringType.Namespace)) {
						ThreadModel superModel = superMethod.ThreadingModel ();
						if (model != superModel) {
							string mesg = String.Format (CultureInfo.InvariantCulture, 
								"{0} {1} must match {2} {3} method.", model, name, superModel,
								new_slot ? "interface" : "base");
							ReportDefect (method, Severity.High, Confidence.High, mesg);
						}
					}
				}
			}
			
			// Serializable cannot be applied to static methods, but can be applied to
			// operators because they're just sugar for normal calls.
			if (method.IsStatic && model.Is (ThreadModel.Serializable) && !name.StartsWith ("op_", StringComparison.Ordinal)) {
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
							ThreadModel targetModel = target.ThreadingModel ();
							if (targetModel == ThreadModel.MainThread) {
								string mesg = String.Format (CultureInfo.InvariantCulture, 
									"An anonymous thread entry point cannot call MainThread {0}.", 
									target.Name);
								
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
						TypeReference type = ctor.DeclaringType;
						if (type.IsDelegate ()) {
							string nspace = type.Namespace;
							// ldftn entry-point
							// newobj System.Void System.Threading.XXX::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a System.Threading delegate
							if (nspace == "System.Threading") {
								string name = type.Name;
								if (name == "ThreadStart" ||
									name == "ParameterizedThreadStart" ||
									name == "WaitCallback" ||
									name == "WaitOrTimerCallback" ||
									name == "TimerCallback") {
										candidate = (MethodReference) ins.Previous.Operand;
								}
							
							// ldftn entry-point
							// newobj System.Void System.AsyncCallback::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a async delegate
							} else if (nspace == "System") {
								if (type.Name == "AsyncCallback") {
									candidate = (MethodReference) ins.Previous.Operand;
								}
							
							// ldftn entry-point
							// newobj System.Void ThreadedDelegate::.ctor (System.Object,System.IntPtr)
							// i.e. creation of a delegate which is decorated with a threading attribute
							} else if (!ThreadRocks.ThreadedNamespace (nspace)) {
								// Delegates must be able to call the methods they are bound to.
								MethodDefinition target = ((MethodReference) ins.Previous.Operand).Resolve ();
								if (target != null) {
									ThreadModel callerModel = type.ThreadingModel ();
									if (!target.IsGeneratedCode () || target.IsProperty ()) {
										ThreadModel targetModel = target.ThreadingModel ();
										if (!IsValidCall (callerModel, targetModel)) {
											string mesg = String.Format (CultureInfo.InvariantCulture,
												"{0} delegate cannot be bound to {1} {2} method.", 
												callerModel, targetModel, target.Name);
											++DefectCount;
											Log.WriteLine (this, "Defect: {0}", mesg);
											Defect defect = new Defect (this, method, method, ins, Severity.High, Confidence.High, mesg);
											Runner.Report (defect);
										}
										
									} else if (!callerModel.Is (ThreadModel.MainThread)) {
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
					TypeReference call_type = call.DeclaringType;
					if (ins.Previous.Is (Code.Newobj) && ins.Previous.Previous.Is (Code.Ldftn)) {
						// A few events are blacklisted because they do not use SynchronizingObject and
						// are therefore always threaded.
						if (IsNonSynchronizedSetter (call)) {
							candidate = (MethodReference) ins.Previous.Previous.Operand;
						
						// But most events do use SynchronizingObject and therefore their threading
						// depends on whether and how SynchronizingObject is initialized.
						} else if (HasSynchronizingObject (call_type)) {
							List<MethodReference> methods;
							if (!synchronizedEvents.TryGetValue (call, out methods)) {
								methods = new List<MethodReference> ();
								synchronizedEvents.Add (call, methods);
							}
							
							methods.AddIfNew ((MethodReference) ins.Previous.Previous.Operand);
						
						// Misc threaded events.
						} else if (call_type.IsNamed ("System.ComponentModel", "BackgroundWorker")) {
							if (call.Name == "add_DoWork") {
								candidate = (MethodReference) ins.Previous.Previous.Operand;
							}
						}
					
					// callvirt System.Void System.Diagnostics.Process::set_SynchronizingObject (System.ComponentModel.ISynchronizeInvoke)
					} else if (SetSynchronizingObject.Matches (call)) {
						if (ins.Previous.OpCode.Code == Code.Ldarg_0) {
							thisSynchronized.Add (call_type);
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
			ThreadModel? method_model = null;
			foreach (KeyValuePair<MethodReference, List<MethodReference>> entry in synchronizedEvents) {
				// if the event is synchronized on this then the target must have the same thread
				// as the current method's type or better and it should not be treated as a entry point.
				if (thisSynchronized.Contains (entry.Key.DeclaringType)) {
					if (method_model == null)
						method_model = method.DeclaringType.ThreadingModel ();
					foreach (MethodReference mr in entry.Value) {
						MethodDefinition target = mr.Resolve ();
						if (target != null) {
							ThreadModel targetModel = target.ThreadingModel ();
							if (!IsValidCall (method_model.Value, targetModel)) {
								string mesg = String.Format (CultureInfo.InvariantCulture, 
									"{0} {1} cannot be bound to {2} {3} method.", 
									method_model, entry.Key, targetModel, target.Name);
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

		static bool IsNonSynchronizedSetter (MemberReference method)
		{
			switch (method.Name) {
			case "add_Disposed":
				return true;
			// MSDN is ambiguous about whether these are supposed to work with SynchronizingObject
			// but mono doesn't.
			case "add_ErrorDataReceived":
			case "add_OutputDataReceived":
				if (method.DeclaringType.IsNamed ("System.Diagnostics", "Process"))
					return true;
				break;
			}
			return false;
		}
		
		static bool HasSynchronizingObject (TypeReference tr)
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
					ThreadModel model = method.ThreadingModel ();
					
					if (method.IsGeneratedCode () && !method.IsProperty ()) {
						anonymous_entry_points.Add (method);
					
					} else if (model == ThreadModel.MainThread) {
						string mesg = String.Format (CultureInfo.InvariantCulture, 
							"{0} is a thread entry point and so cannot be MainThread.", method.Name);
						ReportDefect (method, Severity.High, Confidence.High, mesg);
					}
				}
			}
		}
		
		private void CheckForLegalCall (MethodDefinition caller, Instruction ins)
		{
			MethodDefinition target = ((MethodReference) ins.Operand).Resolve ();
			if (target != null) {
				ThreadModel callerModel = caller.ThreadingModel ();
				ThreadModel targetModel = target.ThreadingModel ();
				if (!IsValidCall (callerModel, targetModel)) {
					string mesg = String.Format (CultureInfo.InvariantCulture, "{0} {1} cannot call {2} {3}.", 
						callerModel, caller.Name, targetModel, target.Name);
					
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
		static bool IsValidCall (ThreadModel caller, ThreadModel target)
		{
			// MainThread code can call everything
			if (caller.Is (ThreadModel.MainThread))
				return true;

			// AllowEveryCaller code can be called by everything
			else if (target.AllowsEveryCaller ())
				return true;

			// SingleThread can call SingleThread/Serializable/Concurrent
			else if (caller.Is (ThreadModel.SingleThread) && (target.Is (ThreadModel.SingleThread) || target.Is (ThreadModel.Serializable) || target.Is (ThreadModel.Concurrent)))
				return true;


			// Serializable/Concurrent can call Serializable/Concurrent
			else if ((caller.Is (ThreadModel.Serializable) || caller.Is (ThreadModel.Concurrent)) &&
				(target.Is (ThreadModel.Serializable) || target.Is (ThreadModel.Concurrent)))
				return true;
			
			return false;
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
		
		#region Fields
		private HashSet<MethodReference> checked_entry_points = new HashSet<MethodReference> ();
		private HashSet<MethodDefinition> anonymous_entry_points = new HashSet<MethodDefinition> ();
		private bool displayed_no_attribute_defect;
		
		private static readonly OpCodeBitmask opcodes_mask = new OpCodeBitmask (0x8000000000, 0x400000000000, 0x0, 0x20);
		private static readonly MethodSignature SetSynchronizingObject = new MethodSignature ("set_SynchronizingObject", "System.Void", new string [] { "System.ComponentModel.ISynchronizeInvoke" });
		
		#endregion
	}
}
