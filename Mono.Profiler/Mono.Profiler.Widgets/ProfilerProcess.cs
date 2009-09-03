// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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


	
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Mono.Profiler;

namespace Mono.Profiler.Widgets {

	public class ProfilerProcess {	

		Process proc;
		string log_file;

		public ProfilerProcess (ProfileConfiguration config)
		{
			log_file = System.IO.Path.GetTempFileName ();
			proc = new Process ();
			proc.StartInfo.FileName = "mono";
			proc.StartInfo.Arguments = "--profile=logging:" + config.ToArgs () + ",o=" + log_file + ",cp=" + Port.ToString () + " " + config.AssemblyPath;
			proc.EnableRaisingEvents = true;
			proc.Exited += delegate { OnExited (); };
		}

		int port;
		int Port {
			get {
				if (port == 0) {
					IPEndPoint ep = new IPEndPoint (IPAddress.Any, 0);
					using (Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
						socket.Bind (ep);
						port = ((IPEndPoint) socket.LocalEndPoint).Port;
					}
				}
				return port;
			}
		}

		NetworkStream stream;
		NetworkStream Stream {
			get {
				if (stream == null) {
					IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, Port);
					TcpClient client = new TcpClient ();
					client.Connect (ep);
					stream = client.GetStream ();
				}
				return stream;
			}
		}
					
		public event EventHandler Exited;

		void OnExited ()
		{
			if (Exited == null)
				return;
			Exited (this, EventArgs.Empty);
		}

		public event EventHandler Paused;

		void OnPaused ()
		{
			if (Paused == null)
				return;
			Paused (this, EventArgs.Empty);
		}

		public string LogFile {
			get { return log_file; }
		}

		string SendCommand (string cmd)
		{
			byte[] buffer = System.Text.Encoding.ASCII.GetBytes (cmd); 
			NetworkStream stream = Stream;
			stream.Write (buffer, 0, buffer.Length);
			buffer = new byte [1024];
			int cnt = stream.Read (buffer, 0, buffer.Length);
			return System.Text.Encoding.ASCII.GetString (buffer, 0, cnt);
		}

		public void Pause ()
		{
			string resp = SendCommand ("disable\n");
			if (resp == "DONE\n")
				OnPaused ();
			else
				throw new Exception ("'disable' command produced error");
		}

		public void Resume ()
		{
			string resp = SendCommand ("enable\n");
			if (resp != "DONE\n")
				throw new Exception ("'enable' command produced error");
		}

		public void Start ()
		{
			proc.Start ();
		}
	}
}
