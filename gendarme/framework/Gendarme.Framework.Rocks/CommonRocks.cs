//
// Gendarme.Framework.Rocks.CommonRocks
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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
using System.Collections.Generic;

namespace Gendarme.Framework.Rocks {

	// Here we keep non-Gendarme/Cecil related rocks

	public static class CollectionRocks {

		/// <summary>
		/// Checks if the list does not contain the item. If so the item is added.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self">The list.</param>
		/// <param name="item">The item to add.</param>
		public static void AddIfNew<T> (this ICollection<T> self, T item)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (item == null)
				throw new ArgumentNullException ("item");

			if (!self.Contains (item))
				self.Add (item);
		}

		public static void AddRangeIfNew<T> (this ICollection<T> self, IEnumerable<T> items)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (items == null)
				throw new ArgumentNullException ("items");

			foreach (T item in items) {
				if (!self.Contains (item))
					self.Add (item);
			}
		}
	}

	public static class SystemRocks {

		/// <summary>
		/// Check if a Version is empty (all zeros).
		/// </summary>
		/// <param name="self">The Version to check</param>
		/// <returns>True if empty, False otherwise.</returns>
		public static bool IsEmpty (this Version self)
		{
			if (self == null)
				return true;
			return ((self.Major == 0) && (self.Minor == 0) && (self.Build <= 0) && (self.Revision <= 0));
		}
	}
}
