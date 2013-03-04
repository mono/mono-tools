<%@ Import Namespace="System.Web.Security" %>
<html>
<script language="C#" runat=server>
        void Page_Load (object sender, EventArgs e)
        {
		FormsAuthentication.SignOut ();
		Response.Redirect ("index.aspx");
        }
</script>
<body>
</body>
</html>
