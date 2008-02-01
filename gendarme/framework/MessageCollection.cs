//
// Gendarme.Framework.MessageCollection class
//
// Authors:
//	Christian Birkl <christian.birkl@gmail.com>
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
using System.Collections;

namespace Gendarme.Framework {
	
	/// <summary>
	/// Represents a typed collection of Messages
	/// </summary>
	public class MessageCollection : IEnumerable {

		private ArrayList innerList = new ArrayList ();


		public MessageCollection ()
		{
		}

		public MessageCollection (Message msg)
			: this ()
		{
			Add (msg);
		}

		/// <summary>
		/// Adds the given message to this collection
		/// </summary>
		/// <param name="message">message to be added</param>
		public void Add (Message message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			innerList.Add (message);
		}

		/// <summary>
		/// Amount of messages
		/// </summary>
		public int Count {
			get { return innerList.Count; }
		}
		
		/// <summary>
		/// Clears this collection
		/// </summary>
		public void Clear ()
		{
			innerList.Clear ();
		}

		/// <summary>
		/// Returns an enumerator for this collection
		/// </summary>
		/// <returns>enumerator</returns>
		public IEnumerator GetEnumerator ()
		{
			return innerList.GetEnumerator ();
		}
	}
}
