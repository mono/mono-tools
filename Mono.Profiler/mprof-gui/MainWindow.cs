using System;
using System.Diagnostics;
using System.Text;
using Gtk;
using Mono.Profiler;
using Mono.Profiler.Widgets;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		view.AppendColumn ("Method", new CellRendererText (), "text", 0);
		view.AppendColumn ("Cost", new CellRendererText (), "text", 1);
	}
	
	protected override bool OnDeleteEvent (Gdk.Event ev)
	{
		Application.Quit ();
		return true;
	}

	protected virtual void OnQuitActivated (object sender, System.EventArgs e)
	{
		Application.Quit ();
	}

	protected virtual void OnNewActivated (object sender, System.EventArgs e)
	{
		FileChooserDialog d = new FileChooserDialog ("Select Application", this, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel, Stock.Execute, ResponseType.Accept);
		FileFilter filter = new FileFilter ();
		filter.AddPattern ("*.exe");
		d.Filter = filter;
		if (d.Run () == (int) ResponseType.Accept && !String.IsNullOrEmpty (d.Filename)) {
			Process proc = new Process ();
			proc.StartInfo.FileName = "mono";
			proc.StartInfo.Arguments = "--profile=logging:calls,o=tmp.mprof " + d.Filename;
			proc.EnableRaisingEvents = true;
			proc.Exited += delegate {
				DisplayOutput ();
			};
			proc.Start ();
		}
		d.Destroy ();		
	}

	
	void DisplayOutput ()
	{
		SyncLogFileReader rdr = new SyncLogFileReader ("tmp.mprof");
		ProfilerEventHandler data = new ProfilerEventHandler ();
		data.LoadedElements.RecordHeapSnapshots = false;
		while (!rdr.HasEnded) {
			BlockData current = null;
			try {
				current = rdr.ReadBlock ();
				current.Decode (data, rdr);
			} catch (DecodingException e) {
				Console.Error.WriteLine ("Stopping decoding after a DecodingException in block of code {0}, length {1}, file offset {2}, block offset {3}: {4}", e.FailingData.Code, e.FailingData.Length, e.FailingData.FileOffset, e.OffsetInBlock, e.Message);
				break;
			}
		}
		view.Model = new TreeModelAdapter (new CallsStore (data));
	}
	
	protected virtual void OnSaveAsActivated (object sender, System.EventArgs e)
	{
	}
}