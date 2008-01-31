using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions.Impl {

	public class ExecutionPath : CollectionBase, ICloneable {
	
		public ExecutionPath ()
		{
		}

		public void Add (ExecutionBlock block)
		{
			List.Add (block);
		}

		public bool Contains (Instruction instruction)
		{
			foreach (ExecutionBlock block in List) {
				if (block.Contains (instruction))
					return true;
			}

			return false;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			ExecutionPath other = obj as ExecutionPath;
			if(other == null)
				return false;			

			if (other.Count != Count)
				return false;

			IEnumerator thisEnum = List.GetEnumerator ();
			IEnumerator otherEnum = other.GetEnumerator ();
			while (thisEnum.MoveNext() && otherEnum.MoveNext ()) {
				ExecutionBlock thisBlock = (ExecutionBlock)thisEnum.Current;
				ExecutionBlock otherBlock = (ExecutionBlock)otherEnum.Current;
				if (thisBlock.First.Offset != otherBlock.First.Offset ||
					thisBlock.Last.Offset != otherBlock.Last.Offset)
					return false;
			}

			return true;
		}

		public override int GetHashCode ()
		{
			return List.GetHashCode ();
		}
		#region ICloneable Members

		public object Clone ()
		{
			ExecutionPath other = new ExecutionPath ();
			foreach (ExecutionBlock block in List)
				other.Add((ExecutionBlock)block.Clone ());

			return other;
		}

		#endregion
	}
}
