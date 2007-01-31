// ComparisonInfo.cs : ComparisonInfo class
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, 
// publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.


namespace IlContrast {

	using System;
	using System.IO;

	public class ComparisonInfo {

		string result_path;
		string base_assembly_path;
		string target_assembly_path;

		public ComparisonInfo (string base_assembly_path, string target_assembly_path, string result_path)
		{
			this.base_assembly_path = base_assembly_path;
			this.target_assembly_path = target_assembly_path;
			this.result_path = result_path;
		}
			
		public ComparisonInfo (string base_path, string target_path) : this (base_path, target_path, Path.Combine (Application.DeployPath, "temp.html")) {}

		public string BaseAssemblyPath {
			get { return base_assembly_path; }
			set { base_assembly_path = value; }
		}

		public string TargetAssemblyPath {
			get { return target_assembly_path; }
			set { target_assembly_path = value; }
		}

		public string ResultPath {
			get { return result_path; }
			set { result_path = value; }
		}
	}
}

