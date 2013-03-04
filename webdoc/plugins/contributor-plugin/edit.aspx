<%@ Assembly name="monodoc" %>
<%@ Import Namespace="Monodoc" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="System.IO" %>
<html>
<head>
  <script language="C#" runat=server>
	static RootTree help_tree = RootTree.LoadTree ();

        void Page_Load (object sender, EventArgs ea)
        {
		HttpWorkerRequest r = (HttpWorkerRequest) ((IServiceProvider)Context).GetService (typeof (HttpWorkerRequest));
		//
		// We need the untouched QueryString, as internally the editor uses the `@' symbol as a separator.
		//
		string q = Request ["link"];
		Console.WriteLine ("QueryString: " + q);
		try {
			XmlNode edit_node = EditingUtils.GetNodeFromUrl ("edit:" + q, help_tree);
			Monodoc2Wiki m2w = new Monodoc2Wiki ();
			Console.WriteLine ("XML TO TEXT: " + edit_node.InnerText);
			EditBuffer.Text = m2w.ProcessNode ((XmlElement) edit_node);
		} catch (Exception e){
			EditBuffer.Text = Request.QueryString.ToString () + e.ToString ();
		}
        }

	void Save (object o, EventArgs a)
	{
		
	}

	void Preview (object o, EventArgs a)
	{
		WikiStyleDocConverter p = new WikiStyleDocConverter (EditBuffer.Text);
		XmlNode result = p.ParseEntireDoc ();
	
		StringWriter sw = new StringWriter ();
		sw.Write ("YOOHO:" + result.InnerText);
		XmlTextWriter xw = new XmlTextWriter (sw);
		xw.Formatting = Formatting.Indented;
		result.WriteTo (xw);
		xw.Close ();

		TextPreview.Text = "Preview<BR>" + sw.ToString ();
	}
  </script>
</head>

<body>
  <form runat=server>
    <asp:Label id="TextPreview" runat=server/>
    <asp:TextBox id="EditBuffer" Text="multiline" TextMode="MultiLine" runat="server" rows=15 cols=80 />
    <p>
    <asp:Button Text="Save Page" OnClick="Save" runat=server/>
    <asp:Button Text="Show Preview" OnClick="Preview" runat=server/>
    <asp:LinkButton Text="Markup Help" runat=server/>
    <asp:LinkButton Text="Cancel" runat=server/>
  </form>
</body>

</html>