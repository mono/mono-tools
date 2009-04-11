<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="GuiCompare" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server" language="c#">

const string ImageMissing = "<img src='images/sm.gif' border=0 align=absmiddle title='Missing'>";
const string ImageExtra   = "<img src='images/sx.gif' border=0 align=absmiddle title='Extra'>";
const string ImageOk      = "<img src='images/sc.gif' border=0 align=absmiddle>";
const string ImageError   = "<img src='images/se.gif' border=0 align=absmiddle title='throw NotImplementedException'>";
const string ImageWarning = "<img src='images/mn.png' border=0 align=absmiddle title='warning'>";

CompareParameters cparam;
CompareParameters Parameters {
	get {
		if (cparam == null)
			cparam = new CompareParameters (Page.Request.QueryString);
		return cparam;
	}
}

NodeUtils db;
NodeUtils DB {
	get {
		if (db == null)
			db = new NodeUtils (Parameters.InfoDir, Parameters.Profile, Parameters.Assembly, Parameters.DetailLevel);
		return db;
	}
}


static string ImageTodo (ComparisonNode cn)
{
	string todo = GetTodo (cn);
	if (!String.IsNullOrEmpty (todo))
		todo = HttpUtility.HtmlEncode (todo);
	return String.Format ("<img src='images/st.gif' border=0 align=absmiddle title=\"{0}\">", todo);
}

static string Get (int count, string kind, string caption)
{
	if (count == 0)
		return "";

	caption = HttpUtility.HtmlEncode (caption);
	return String.Format ("<span class='report' title=\"{0} {2}\"><span class='icons suffix {1}'></span>{0}</span>", count, kind, caption);
}
	  
static string GetStatus (ComparisonNode n)
{
	string status = 
		Get (n.Missing, "missing", "missing members") +
		Get (n.Extra, "extra", "extra members") +
		Get (n.Warning, "warning", "warnings") +
		Get (n.Todo, "todo", "items with notes") +
		Get (n.Niex, "niex", "members that throw NotImplementedException");

	if (status != "")
		return n.Name + status;

	return n.Name;
}

public void Page_Load ()
{
	if (IsPostBack)
		return;

	Header.Title = String.Format ("Mono {1} in {0} vs MS.NET {2}", Parameters.InfoDir, Parameters.Assembly, Parameters.Profile);
	page_header.InnerText = Header.Title;

	string detail = Request.QueryString ["detail_level"];
	if (String.IsNullOrEmpty (detail) || detail != "detailed")
		detail = "normal";
	
	dlevel.SelectedValue = detail;
	var cp = Parameters;
	var n = DB.GetRootNode ();
	if (n == null) {
		tree.Visible = false;
		tree.Enabled = false;
		time_label.Text = "No data available for " + 
				HttpUtility.HtmlEncode (String.Format ("Mono <b>{1}</b> in {0} vs MS.NET {2}",
							Parameters.InfoDir, Parameters.Assembly, Parameters.Profile));
		return;
	}

	TreeNode tn = new TreeNode (GetStatus (n), n.InternalID.ToString ());
	tn.SelectAction = TreeNodeSelectAction.None;
	tn.PopulateOnDemand = true;
	tree.Nodes.Add (tn);

	var diff = DateTime.UtcNow - DB.LastUpdateTime;
	string t;
	if (diff.Days > 1)
		t = String.Format ("{0} days", diff.Days);
	else if (diff.Hours > 2)
		t = String.Format ("{0} hours", diff.Hours);
	else if (diff.Minutes > 2)
	        t = String.Format ("{0} minutes", diff.Minutes);
	else 
	        t = String.Format ("{0} seconds", diff.Seconds);

	time_label.Text = String.Format ("Assembly <b>{1}</b> last updated: {0} ago", t, HttpUtility.HtmlEncode (Parameters.Assembly));
}

static string GetTodo (ComparisonNode cn)
{
	StringBuilder sb = new StringBuilder ();
	foreach (string s in cn.Todos){
		string clean = s.Substring (20, s.Length-22);
		if (clean == "")
			sb.Append ("Flagged with TODO");
		else {
			sb.Append ("Comment: ");
			sb.Append (clean);
			sb.Append ("<br>");
		}
	}
	return sb.ToString ();
}

static string GetMessages (ComparisonNode cn)
{
	StringBuilder sb = new StringBuilder ();
	foreach (string s in cn.Messages){
		sb.Append (s);
		sb.Append ("<br>");
	}
	return sb.ToString ();
}

static string ImagesFromCounts (ComparisonNode cn)
{
	int x = (cn.Todo != 0 ? 2 : 0) | (cn.Warning != 0 ? 1 : 0);
	switch (x) {
        case 0:
       		return null;
	case 1:
		return ImageWarning;
	case 2:
	        return ImageTodo (cn);
	case 4:
	        return ImageTodo (cn) + ImageWarning;
	default:
		break;
	}
	return null;
}

static string MemberStatus (ComparisonNode cn)
{
	if (cn.Niex != 0)
		cn.Status = ComparisonStatus.Error;

	string counts = ImagesFromCounts (cn);

	switch (cn.Status) {
	case ComparisonStatus.None:
	        return ImageOk + counts;
		
	case ComparisonStatus.Missing:
		return ImageMissing;
		
	case ComparisonStatus.Extra:
		return ImageExtra + counts;
		
	case ComparisonStatus.Error:
	        return ImageError + counts;

	default:
		return "Unknown status: " + cn.Status;
	}
}

// {0} = MemberStatus
// {1} = child.Name
// {2} = child notes
// {3} = type
static string RenderMemberStatus (ComparisonNode cn, string format)
{
	return String.Format (format, 
		MemberStatus (cn), 
		cn.Name,
		(cn.Missing > 0 ? ImageMissing : "") + (cn.Extra > 0 ? ImageExtra : ""),
		cn.Type.ToString ());
}

// uses for class, struct, enum, interface
static string GetFQN (ComparisonNode node)
{
	if (node.Parent == null)
		return "";

	string n = GetFQN (node.Parent);
	int p = node.Name.IndexOf (' ');
	string name = p == -1 ? node.Name : node.Name.Substring (p+1);
	p = name.IndexOf ('<');
	if (p != -1)
		name = name.Substring (0, p); // remove generic parameters from URL

	return n == "" ? name : n + "." + name;
}

// used for methods
static string GetMethodFQN (ComparisonNode node)
{
	if (node.Parent == null)
		return "";

	int p = node.Name.IndexOf ('(');
	int q = node.Name.IndexOf (' ');
	
	string name = p == -1 || q == -1 ? node.Name : node.Name.Substring (q+1, p-q-1);
	p = name.IndexOf ('<');
	if (p != -1)
		name = name.Substring (0, p); // remove generic parameters from URL
	
	if (name == ".ctor")
		name = "";

	string n = GetFQN (node.Parent);
	return n == "" ? name : n + (name == "" ? "" : "." + name);
}

static string MakeURL (string type)
{
	return "http://msdn.microsoft.com/en-us/library/" + type.ToLower () + ".aspx";
}

static TreeNode MakeContainer (string kind, ComparisonNode node)
{
	TreeNode tn = new TreeNode (String.Format ("{0} {1} {2}", MemberStatus (node), kind, GetStatus (node)), node.InternalID.ToString ());
	
	tn.SelectAction = TreeNodeSelectAction.None;
	return tn;
}

static void AttachComments (TreeNode tn, ComparisonNode node)
{
	if (node.Messages.Count != 0){
		TreeNode m = new TreeNode (GetMessages (node));
		m.SelectAction = TreeNodeSelectAction.None;
		tn.ChildNodes.Add (m);
	}
	if (node.Todos.Count != 0){
		TreeNode m = new TreeNode (GetTodo (node));
		tn.ChildNodes.Add (m);
	}
}

void TreeNodePopulate (object sender, TreeNodeEventArgs e)
{
	string val = e.Node.Value;
	ComparisonNode cn = DB.GetNodeByName (val);
	if (cn == null){
		Console.WriteLine ("ERROR: Did not find the node " + e.Node.Value);
		e.Node.ChildNodes.Add (new TreeNode ("ERROR: Did not find the node", "Error"));
		return;
	}

	ComparisonNode chain = cn;
	int last = val.LastIndexOf ('-');
	while (last > 0) {
		string parent = val.Substring (0, last);
		ComparisonNode node = DB.GetNodeByName (parent, false, false);
		if (node == null)
			break;
		chain.Parent = node;
		chain = node;
		last = parent.LastIndexOf ('-');
	}

	foreach (var child in cn.Children) {
		TreeNode tn;

		switch (child.Type){
		case CompType.Namespace:
			tn = new TreeNode (GetStatus (child));
			tn.SelectAction = TreeNodeSelectAction.None;
			break;

		case CompType.Class:
		        tn = MakeContainer ("class", child);
			break;

		case CompType.Struct:
		        tn = MakeContainer ("struct", child);
			break;
			
		case CompType.Interface:
		        tn = MakeContainer ("interface", child);
			break;
			
		case CompType.Enum:
		        tn = MakeContainer ("enum", child);
			break;

		case CompType.Method:
			tn = new TreeNode (RenderMemberStatus (child, "{0}{1}{2}"));
			AttachComments (tn, child);
			switch (cn.Type){
			case CompType.Property:
			        tn.NavigateUrl = MakeURL (GetFQN (cn));
				break;

			default:
				tn.NavigateUrl = MakeURL (GetMethodFQN (child));
				break;
			}
			tn.Target = "_blank";
			break;
			
		case CompType.Property:
		case CompType.Field:
		case CompType.Delegate:
		case CompType.Event:
			tn = new TreeNode (RenderMemberStatus (child, "{0} {3} {1}{2}"));
			AttachComments (tn, child);

			// Fields whose parents are an enum are enum definitions, make the link useful
			if (child.Type == CompType.Field && cn.Type == CompType.Enum)
			   	tn.NavigateUrl = MakeURL (GetFQN (cn));
			else 
				tn.NavigateUrl = MakeURL (GetFQN (child));
			tn.Target = "_blank";
			break;

		case CompType.Assembly:
		case CompType.Attribute:
			tn = new TreeNode (RenderMemberStatus (child, "{0} {3} {1}{2}"));
			break;

		default:
			tn = new TreeNode ("Unknown type: " + child.Type.ToString());
			break;
		}

		tn.Value = child.InternalID.ToString ();
		tn.PopulateOnDemand = child.HasChildren;
		
		e.Node.ChildNodes.Add (tn);
	}
}

void OnLevelChanged (object sender, EventArgs args)
{
	if (dlevel.SelectedIndex < 0)
		return;

	string url = String.Format ("{0}?reference={1}&profile={2}&assembly={3}&detail_level={4}", Request.FilePath,
				Parameters.InfoDir, Parameters.Profile, Parameters.Assembly, dlevel.SelectedValue);
	Response.Redirect (url);
}

</script>
<html>
<head id="head1" runat="server">
<title>Mono API Compare</title>
<link href="main.css" media="screen" type="text/css" rel="stylesheet">
</head>
<body>
    <div id="header">
    	<h1 runat="server" id="page_header">Mono Class Status Pages</h1>
    </div>
    <form id="form" runat="server">
	<div id="content">
		<div id="treeview">
			<br>
			<asp:Label id="time_label" runat="server"/>
			<asp:TreeView ID="tree" Runat="server"
				OnTreeNodePopulate="TreeNodePopulate"
				EnableClientScript="true"
				PopulateNodesFromClient="true"
				ExpandDepth="1">
			</asp:TreeView>
		</div>
		<div id="detaillevel">
		<div style="font-weight: bold; margin-bottom: 0.5em; text-align: center;">Detail Level</div>
		<asp:RadioButtonList id="dlevel" runat="server"
			AutoPostBack="true"
			RepeatDirection="vertical"
			OnSelectedIndexChanged="OnLevelChanged">
			<asp:ListItem Text="Normal" Value="normal" Selected="true" />
			<asp:ListItem Text="Detailed" Value="detailed" />
		</asp:RadioButtonList>
		</div>
	</div>
    </form>
</body>
</html>
