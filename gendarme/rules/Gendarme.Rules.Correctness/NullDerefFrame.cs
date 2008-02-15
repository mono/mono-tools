/*
 * NullDerefFrame.cs: the fact passed around in dataflow analysis for
 * null-dereference checking.
 *
 * Authors:
 *   Aaron Tomb <atomb@soe.ucsc.edu>
 *
 * Copyright (c) 2005 Aaron Tomb and the contributors listed
 * in the ChangeLog.
 *
 * This is free software, distributed under the MIT/X11 license.
 * See the included LICENSE.MIT file for details.
 **********************************************************************/

using System;
using System.Text;

using Gendarme.Framework;

namespace Gendarme.Rules.Correctness {

	public class NullDerefFrame : ICloneable {
		[NonNull] private Nullity[] stack;
		[NonNull] private Nullity[] locals;
		[NonNull] private Nullity[] args;
		[NonNull] private IRunner runner;
		int stackDepth;

		/* Unused   Null	 NonNull  Unknown			*/
		/* -------------------------------------		 */
		/* Unused   Null	 NonNull  Unknown  | Unused  */
		/* Null	 Null	 Unknown  Unknown  | Null	*/
		/* NonNull  Unknown  NonNull  Unknown  | NonNull */
		/* Unknown  Unknown  Unknown  Unknown  | Uknown  */
		[NonNull] private static Nullity[][] lub = {
			new Nullity[] { Nullity.Unused, Nullity.Null,
				Nullity.NonNull, Nullity.Unknown },
			new Nullity[] { Nullity.Null, Nullity.Null,
				Nullity.Unknown, Nullity.Unknown },
			new Nullity[] { Nullity.NonNull, Nullity.Unknown,
				Nullity.NonNull, Nullity.Unknown },
			new Nullity[] { Nullity.Unknown, Nullity.Unknown,
				Nullity.Unknown, Nullity.Unknown },
		};

		public NullDerefFrame(int maxStackDepth, int numLocals, int numArgs, bool entry, [NonNull] IRunner runner)
		{
			int i;

			stackDepth = 0;
			stack = new Nullity[maxStackDepth];
			locals = new Nullity[numLocals];
			args = new Nullity[numArgs];
			for(i = 0; i < maxStackDepth; i++)
				stack[i] = Nullity.Unused;

			if (entry) {
				for(i = 0; i < numLocals; i++)
					locals[i] = Nullity.Null;
				for(i = 0; i < numArgs; i++)
					args[i] = Nullity.Unknown;
			} else {
				for(i = 0; i < numLocals; i++)
					locals[i] = Nullity.Unused;
				for(i = 0; i < numArgs; i++)
					args[i] = Nullity.Unused;
			}
			this.runner = runner;
		}

		public void PushStack (Nullity n)
		{
			if(stackDepth == stack.Length) {
				throw new Exception("Nullity stack overflow");
			}
			/*
			if(runner.Debug)
				Console.WriteLine("Push: {0} {1} {2}", stackDepth,
						stack.Length, n);
						*/
			stack[stackDepth] = n;
			stackDepth++;
		}

		[NonNull]
		public Nullity PopStack ()
		{
			if (stackDepth == 0) {
				throw new Exception("Nullity stack underflow");
			}
			/*
			if(runner.Debug)
				Console.WriteLine("Pop: {0} {1} {2}", stackDepth,
						stack.Length, stack[stackDepth - 1]);
						*/
			stackDepth--;
			Nullity result = stack[stackDepth];
			stack[stackDepth] = Nullity.Unused;
			return result;
		}

		public void PopStack (int count)
		{
			for(int i = 0; i < count; i++)
				PopStack ();
		}

		public void EmptyStack ()
		{
			PopStack (stackDepth);
		}

		[NonNull]
		public Nullity PeekStack()
		{
			if(stackDepth == 0) {
				throw new Exception("Nullity stack underflow");
			}
			return stack[stackDepth - 1];
		}

		public void SetLocNullity(int index, Nullity n)
		{
			if(runner.VerbosityLevel > 1)
				Console.WriteLine("SetLoc {0} {1} {2}", index, locals.Length, n);
			locals[index] = n;
		}

		[NonNull]
		public Nullity GetLocNullity(int index)
		{
			if (runner.VerbosityLevel > 1)
				Console.WriteLine ("GetLoc {0} {1} {2}", index, locals.Length,
						locals[index]);
			return locals[index];
		}

		public void SetArgNullity(int index, Nullity n)
		{
			if (runner.VerbosityLevel > 1) {
				Console.WriteLine("SetArg {0} {1} {2}", index, args.Length, n);
				Console.Out.Flush();
			}
			args[index] = n;
		}

		[NonNull]
		public Nullity GetArgNullity(int index)
		{
			if (runner.VerbosityLevel > 1) {
				Console.WriteLine("GetArg {0} {1}", index, args.Length);
				Console.Out.Flush();
			}
			return args[index];
		}

		public void MergeWith([NonNull] NullDerefFrame incoming)
		{
			int i;
			if(locals.Length != incoming.locals.Length ||
					args.Length != incoming.args.Length ||
					stack.Length != incoming.stack.Length)
				throw new Exception("Merging incompatible frames");

			for(i = 0; i < locals.Length; i++)
				locals[i] = MergeNullity(locals[i], incoming.locals[i]);
			for(i = 0; i < args.Length; i++)
				args[i] = MergeNullity(args[i], incoming.args[i]);
			for(i = 0; i < stack.Length; i++)
				stack[i] = MergeNullity(stack[i], incoming.stack[i]);
			if(incoming.stackDepth > stackDepth)
				stackDepth = incoming.stackDepth;
		}

		[NonNull]
		public static Nullity MergeNullity(Nullity n1, Nullity n2)
		{
			return lub[(int)n1][(int)n2];
		}

		public override bool Equals(object o)
		{
			if(o == null)
				return false;
			NullDerefFrame frame = (NullDerefFrame)o;
			if(this.stackDepth != frame.stackDepth)
				return false;
			if(this.stack.Length != frame.stack.Length)
				return false;
			if(this.args.Length != frame.args.Length)
				return false;
			if(this.locals.Length != frame.locals.Length)
				return false;
			int i;
			for(i = 0; i < this.stack.Length; i++)
				if(this.stack[i] != frame.stack[i])
					return false;
			for(i = 0; i < this.args.Length; i++)
				if(this.args[i] != frame.args[i])
					return false;
			for(i = 0; i < this.locals.Length; i++)
				if(this.locals[i] != frame.locals[i])
					return false;
			return true;
		}

		public override int GetHashCode()
		{
			/* FIXME: we can do better than this, perhaps? */
			return base.GetHashCode();
		}

		[NonNull]
		public object Clone()
		{
			NullDerefFrame result = new NullDerefFrame(stack.Length,
					locals.Length, args.Length, false, runner);
			int i;

			for(i = 0; i < locals.Length; i++)
				result.locals[i] = locals[i];
			for(i = 0; i < args.Length; i++)
				result.args[i] = args[i];
			for(i = 0; i < stack.Length; i++)
				result.stack[i] = stack[i];
			result.stackDepth = stackDepth;
			return result;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("Locals {");
			for (int i = 0; i < locals.Length; i++) {
				sb.Append (" ");
				sb.Append (locals [i].ToString ());
			}
			sb.AppendLine (" }");
			sb.Append ("Args { ");
			for (int i = 0; i < args.Length; i++) {
				sb.Append (" ");
				sb.Append (args [i].ToString ());
			}
			sb.AppendLine (" }");
			sb.Append ("Stack { ");
			for (int i = 0; i < stack.Length; i++) {
				sb.Append (" ");
				sb.Append (stack [i].ToString ());
			}
			sb.AppendLine (" }");
			sb.Append ("stackDepth = ");
			sb.Append (stackDepth);
			sb.AppendLine ();
			return sb.ToString ();
		}
	}
}
