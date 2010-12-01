//
// FxCopRule.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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

namespace FxCopMapBuilder {
	sealed class FxCopRule : IEquatable<FxCopRule> {
		public string Category { get; internal set; }

		public string Id { get; internal set; }

		public string Name { get; internal set; }

		public FxCopRule (string category, string id)
		{
			Category = category;
			// Splits a string like CA1034:NestedTypesShouldNotBeVisible to 
			// the Id (CA1034) and Name (NestedTypesShouldNotBeVisible)
			if (id.Contains(".")) {
				string [] ruleInfo = id.Split (':');
				Id = ruleInfo [0];
				Name = ruleInfo [1];
			} else {
				Id = id;
			}
		}

		public override string ToString ()
		{
			string returnValue = Category + "." + Id;
			if (!String.IsNullOrEmpty (Name))
				returnValue += ":" + Name;
			return returnValue;
		}

		public override bool Equals (object obj)
		{
			FxCopRule rule = obj as FxCopRule;
			return Equals (rule);
		}

		public bool Equals (FxCopRule other)
		{
			bool isEqual = (Category == other.Category) && (Id == other.Id);

			// sets the rule name from another reference to the same rule
			if (isEqual && String.IsNullOrEmpty (Name) && !String.IsNullOrEmpty (other.Name))
				Name = other.Name;

			return isEqual;
		}

		public override int GetHashCode ()
		{
			return Category.GetHashCode () ^ Id.GetHashCode ();
		}
	}
}
