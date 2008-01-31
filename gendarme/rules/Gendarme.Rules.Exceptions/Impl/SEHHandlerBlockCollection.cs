using System;
using System.Collections;

namespace Gendarme.Rules.Exceptions {

	internal class SEHHandlerBlockCollection : IList, ICollection {
	
		private ArrayList list;
		
		public SEHHandlerBlockCollection ()
		{
			list = new ArrayList ();
		}
		
		public int Add (SEHHandlerBlock sehHandlerBlock)
		{
			return list.Add (sehHandlerBlock);
		}
		
		public void Remove (SEHHandlerBlock sehHandlerBlock)
		{
			if (list.Contains (sehHandlerBlock))
				list.Remove (sehHandlerBlock);
		}
		
		public void Clear ()
		{
			list.Clear ();
		}
		
		public bool Contains (SEHHandlerBlock sehHandlerBlock)
		{
			return list.Contains (sehHandlerBlock);
		}
		
		public SEHHandlerBlock this[int index] {
			get { return (SEHHandlerBlock)list [index]; }
		}
		
		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}
		
		public object SyncRoot {
			get { return list.SyncRoot; }
		}
		
		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}
		
		public int Count {
			get { return list.Count; }
		}
		
		public void RemoveAt(int index)
		{
			list.RemoveAt (index);
		}

		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsFixedSize {
			get { return false; }
		}

		public int IndexOf (SEHHandlerBlock sehHandlerBlock)
		{
			return list.IndexOf (sehHandlerBlock);
		}
		
		public void Insert (int index, SEHHandlerBlock sehHandlerBlock)
		{
			list.Insert (index, sehHandlerBlock);
		}
								
		int IList.Add (object obj)
		{
			return Add ((SEHHandlerBlock)obj);
		}
		
		void IList.Remove (object obj)
		{
			Remove ((SEHHandlerBlock)obj);
		}
		
		object IList.this[int index] {
			get { return list [index]; }
			set { throw new NotSupportedException (); }
		}
		
		int IList.IndexOf (object obj)
		{
			return IndexOf ((SEHHandlerBlock)obj);
		}
		
		bool IList.Contains (object obj)
		{
			return Contains ((SEHHandlerBlock)obj);
		}
		
		void IList.Insert (int index, object obj)
		{
			Insert (index, (SEHHandlerBlock)obj);
		}
	}
}
