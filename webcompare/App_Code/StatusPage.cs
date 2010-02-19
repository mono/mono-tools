using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Collections.Specialized;
using GuiCompare;

namespace WebCompare {
	public class StatusPage : Page {
		static object [] ImageDescriptions;
		protected HtmlGenericControl page_header;
		protected TreeView tree;
		protected Label time_label;
		protected ListView tbl_legend;

		CompareParameters cparam;
		NodeUtils db;

		private CompareParameters Parameters {
			get {
				if (cparam == null)
					cparam = new CompareParameters (Page.Request.QueryString);
				return cparam;
			}
		}

		private NodeUtils DB {
			get {
				if (db == null)
					db = new NodeUtils (Parameters.InfoDir, Parameters.Profile, Parameters.Assembly, Parameters.DetailLevel);
				return db;
			}
		}

		static StatusPage ()
		{
			ImageDescriptions = new object [] {
					new { Image = "images/sm.gif", Description = "Missing: we need it" },
					new { Image = "images/sx.gif", Description = "Extra: not present in MS.NET" },
					new { Image = "images/se.gif", Description = "Error: declaration mismatch" },
					new { Image = "images/st.gif", Description = "TODO: some functionality might be missing" },
					new { Image = "images/mn.png", Description = "The method just throws a NotImplementedException" }
				};
		}

		protected override void OnLoad (EventArgs args)
		{
			if (IsPostBack)
				return;

			Header.Title = String.Format ("Mono {1} in {0} vs MS.NET {2}", Parameters.InfoDir, Parameters.Assembly, Parameters.Profile);
			page_header.InnerText = Header.Title;

			var n = DB.GetRootNode ();
			if (n == null) {
				tree.Visible = false;
				tree.Enabled = false;
				time_label.Text = "No data available for " + 
						String.Format ("Mono <b>{1}</b> in {0} vs MS.NET {2}",
								HttpUtility.HtmlEncode (Parameters.InfoDir),
								HttpUtility.HtmlEncode (Parameters.Assembly),
								HttpUtility.HtmlEncode (Parameters.Profile));
				return;
			}

			tbl_legend.DataSource = ImageDescriptions;
			tbl_legend.DataBind ();
			TreeNode tn = new TreeNode (GetHtmlForNode (n, true), n.InternalID.ToString ());
			tn.SelectAction = TreeNodeSelectAction.None;
			tn.PopulateOnDemand = true;
			tree.Nodes.Add (tn);
			SetLabelTime (DB.LastUpdateTime);
		}

		void SetLabelTime (DateTime last_update)
		{
			var diff = DateTime.UtcNow - last_update;
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

		static string GetHtmlForNode (ComparisonNode n, bool is_container)
		{
			HtmlGenericControl ctrl = new HtmlGenericControl ();
			ctrl.Controls.Add (GetStatusImage (n, is_container));
			ctrl.Controls.Add (GetTypeImage (n.Type));
			bool completed = (n.Missing + n.Extra + n.Warning + n.Todo + n.Niex == 0);
			string status_class = null;
			if (completed && n.Status == ComparisonStatus.None)
				status_class = "full ";
			ctrl.Controls.Add (new Label () { Text = n.Name, CssClass = status_class + "cname" });
			//if (is_container) {
				AddIconAndText (ctrl, "stmiss", n.Missing);
				AddIconAndText (ctrl, "stextra", n.Extra);
				AddIconAndText (ctrl, "sterror", n.Warning);
				AddIconAndText (ctrl, "sttodo", n.Todo);
				AddIconAndText (ctrl, "stniex", n.Niex);
			//}

			return GetHtmlFromControl (ctrl);
		}

		static string GetHtmlFromControl (Control ctrl)
		{
			StringWriter sr = new StringWriter ();
			HtmlTextWriter tr = new HtmlTextWriter (sr);
			ctrl.RenderControl (tr);
			tr.Flush ();
			return sr.GetStringBuilder ().ToString ();
		}

		static Control GetStatusImage (ComparisonNode n, bool is_container)
		{
			WebControl img = new WebControl (HtmlTextWriterTag.Span);
			img.Controls.Add (new Literal () { Text = "&nbsp;"} );
			img.CssClass = "icons ";
			switch (n.Status) {
			case ComparisonStatus.Missing: img.CssClass += "stmiss"; break;
			case ComparisonStatus.Extra: img.CssClass += "stextra"; break;
			case ComparisonStatus.Error: img.CssClass += "sterror"; break;
			case ComparisonStatus.None:
				if (!is_container && n.Niex > 0)
					img.CssClass += "stniex";
				else if (n.Todos.Count > 0)
					img.CssClass += "sttodo";
				else
					img.CssClass += "stnone";
				break;
			}
			return img;
		}

		static Control GetTypeImage (CompType type)
		{
			WebControl img = new WebControl (HtmlTextWriterTag.Span);
			img.Controls.Add (new Literal () { Text = "&nbsp;"} );
			img.CssClass = "icons ";
			switch (type) {
			case CompType.Assembly: img.CssClass += "tyy"; break;
			case CompType.Namespace: img.CssClass += "tyn"; break;
			case CompType.Attribute: img.CssClass += "tyr"; break;
			case CompType.Interface: img.CssClass += "tyi"; break;
			case CompType.Class: img.CssClass += "tyc"; break;
			case CompType.Struct: img.CssClass += "tys"; break;
			case CompType.Delegate: img.CssClass += "tyd"; break;
			case CompType.Enum: img.CssClass += "tyen"; break;
			case CompType.Method: img.CssClass += "tym"; break;
			case CompType.Property: img.CssClass += "typ"; break;
			case CompType.Field: img.CssClass += "tyf"; break;
			case CompType.Event: img.CssClass += "tye"; break;
			}
			return img;
		}

		static void AddIconAndText (Control parent, string klass, int n)
		{
			if (n <= 0)
				return;

			WebControl img = new WebControl (HtmlTextWriterTag.Span);
			img.Controls.Add (new Literal () { Text = ": " + n });
			img.CssClass = "icontext " + klass;
			parent.Controls.Add (img);
		}

		protected void TreeNodePopulate (object sender, TreeNodeEventArgs e)
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
				case CompType.Class:
				case CompType.Struct:
				case CompType.Interface:
				case CompType.Enum:
					tn = new TreeNode (GetHtmlForNode (child, true), child.InternalID.ToString ());
					tn.SelectAction = TreeNodeSelectAction.None;
					break;

				case CompType.Method:
					tn = new TreeNode (GetHtmlForNode (child, false));
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
					bool prop_or_evt = (child.Type == CompType.Property || child.Type == CompType.Event);
					tn = new TreeNode (GetHtmlForNode (child, prop_or_evt));
					AttachComments (tn, child);

					// Fields whose parents are an enum are enum definitions, make the link useful
					if (child.Type == CompType.Field && cn.Type == CompType.Enum)
						tn.NavigateUrl = MakeURL (GetFQN (cn));
					else
						tn.NavigateUrl = MakeURL (GetFQN (child));
					tn.Target = "_blank";
					break;

				case CompType.Attribute:
					tn = new TreeNode (GetHtmlForNode (child, false));
					tn.SelectAction = TreeNodeSelectAction.None;
					break;

				case CompType.Assembly:
					/* Should not happen */
					throw new Exception ("Should not happen");
				default:
					tn = new TreeNode ("Unknown type: " + child.Type.ToString());
					break;
				}

				tn.Value = child.InternalID.ToString ();
				if (tn.ChildNodes.Count == 0)
					tn.PopulateOnDemand = child.HasChildren;
				
				e.Node.ChildNodes.Add (tn);
			}
		}

		static void AttachComments (TreeNode tn, ComparisonNode node)
		{
			if (node.Messages.Count != 0){
				TreeNode m = new TreeNode (GetMessages (node));
				m.SelectAction = TreeNodeSelectAction.None;
				tn.ChildNodes.AddAt (0, m);
			}
			if (node.Todos.Count != 0){
				TreeNode m = new TreeNode (GetTodo (node));
				m.SelectAction = TreeNodeSelectAction.None;
				tn.ChildNodes.AddAt (0, m);
			}
		}

		static string GetMessages (ComparisonNode cn)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<b>");
			foreach (string s in cn.Messages){
				sb.Append (s);
				sb.Append ("<br>");
			}
			sb.Append ("</b>");
			return sb.ToString ();
		}

		static string GetTodo (ComparisonNode cn)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (string s in cn.Todos){
				int idx = s.IndexOf ('(');
				string clean = s;
				if (idx != 0) {
					int end = clean.LastIndexOf (')');
					int l = end - idx - 1;
					if (l >= 0)
						clean = clean.Substring (idx + 1, end - idx - 1);
				}
				if (clean == "") {
					sb.Append ("Flagged with TODO");
				} else {
					sb.Append ("<b>TODO Comment:</b> ");
					sb.Append (clean);
					sb.Append ("<br>");
				}
			}
			return sb.ToString ();
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

	}
}

