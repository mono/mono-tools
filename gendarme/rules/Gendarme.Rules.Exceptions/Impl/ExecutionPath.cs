using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions.Impl {

	public class ExecutionPathCollection : CollectionBase, ICloneable {
	
		public ExecutionPathCollection ()
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

			return Equals (obj as ExecutionPathCollection);
		}

		public bool Equals (ExecutionPathCollection path)
		{
			if (path == null)
				return false;

			if (path.Count != Count)
				return false;

			IEnumerator thisEnum = List.GetEnumerator ();
			IEnumerator otherEnum = path.GetEnumerator ();
			while (thisEnum.MoveNext () && otherEnum.MoveNext ()) {
				ExecutionBlock thisBlock = (ExecutionBlock) thisEnum.Current;
				ExecutionBlock otherBlock = (ExecutionBlock) otherEnum.Current;
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
			ExecutionPathCollection other = new ExecutionPathCollection ();
			foreach (ExecutionBlock block in List)
				other.Add((ExecutionBlock)block.Clone ());

			return other;
		}

		#endregion
	}
}
