//
// NodeUtils.cs
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) Copyright 2009 Novell, Inc
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
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using GuiCompare;

public class CompareParameters {
	public CompareParameters (NameValueCollection nvc)
	{
		Assembly = nvc ["assembly"] ?? "mscorlib";
		InfoDir  = nvc ["reference"] ?? "3.5";
		profile = nvc ["profile"] ?? "2.0";
		detail_level = nvc ["detail_level"] ?? "normal";
		Validate (profile);
		BinDir = "binary/" + profile;
	}

	public DateTime GetAssemblyTime ()
	{
		return new FileInfo (DllFile).LastWriteTimeUtc;
	}

	static void Validate (string s)
	{
		if (s.IndexOf ("..") != -1 || s.IndexOf ('/') != -1 || s.IndexOf ('%') != -1 || s.IndexOf (' ') != -1)
			throw new Exception (String.Format ("Invalid parameter: {0}", s));
	}

	string profile;
	public string Profile {
		get { return profile; }
	}

	string detail_level;
	public string DetailLevel {
		get { return detail_level; }
	}

	string assembly;
	public string Assembly { 
		get { return assembly; }
		private set { 
			Validate (value);
			assembly = value;
		}
	}

	string infodir;
	public string InfoDir { 
		get { return infodir; }
		private set { 
			Validate (value);
			infodir = value;
		}
	}

	public string BinDir {  get; private set; } 

	string DllFile {
	       	get {
	       		return Path.Combine (HttpRuntime.AppDomainAppPath, Path.Combine (BinDir, Assembly) + ".dll");
		}
	}
}

