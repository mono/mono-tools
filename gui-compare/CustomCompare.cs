// CustomCompare.cs
//
// Copyright (c) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using Gtk;
using System.IO;

namespace GuiCompare
{
	public partial class CustomCompare : Gtk.Dialog
	{	
		public CustomCompare()
		{
			this.Build();
		}
		
		CompareDefinition Error (string format, params string [] args)
		{
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, String.Format (format, args));
			md.Run ();
			md.Destroy ();
			return null;
		}
		
		public CompareDefinition GetCompare ()
		{
			if (String.IsNullOrEmpty (reference.File))
				return Error ("No reference file was provided");
			
			if (String.IsNullOrEmpty (target.File))
				return Error ("No target file was provided");
			
			if (!File.Exists (reference.File))
				return Error ("Reference file {0} does not exist", reference.File);
				
			if (!File.Exists (target.File))
				return Error ("Target file {0} does not exist", target.File);
				
			
			CompareDefinition cd = new CompareDefinition (reference.IsInfo, reference.File, target.IsInfo, target.File);
			cd.IsCustom = true;
			return cd;
		}
	}
}
