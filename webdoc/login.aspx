<%@ Import Namespace="System.Web.Security" %>
<html>
<script language="C#" runat=server>

	void Allow ()
	{
		FormsAuthentication.RedirectFromLoginPage (UserEmail.Value, false);
		// PersistCookie.Checked);
	}

        void Login_Click (object sender, EventArgs e)
        {
		FormsAuthenticationTicket trust = null;
		HttpCookie c;

		switch (UserEmail.Value){
		case "miguel":
		        trust = new FormsAuthenticationTicket ("high", false, 1);
			c =  new HttpCookie ("level", FormsAuthentication.Encrypt (trust));
			Response.AppendCookie (c);
		        Allow ();
			break;
		case "guest":
		        trust = new FormsAuthenticationTicket ("low", false, 1);
			c =  new HttpCookie ("level", FormsAuthentication.Encrypt (trust));
			Response.AppendCookie (c);
			Allow ();
			break;
		default:
                        Msg.Text = "Invalid Credentials: Please try again";
			break;
		}
        }

	void Page_Load ()
	{
		Msg.Text = ">>> " + Request.QueryString ["ReturnUrl"] + "<<<";
	}
</script>
<body>
<form runat=server>

        <h3><font face="Verdana">Login Page</font></h3>
        <table>
                <tr>
                <td>Email:</td>
                <td><input id="UserEmail" type="text" runat=server/></td>
                <td><ASP:RequiredFieldValidator ControlToValidate="UserEmail"
                         Display="Static" ErrorMessage="*" runat=server/></td>
                </tr>
                <tr>
                <td>Password:</td>
                <td><input id="UserPass" type=password runat=server/></td>
                <td><ASP:RequiredFieldValidator ControlToValidate="UserPass"
                         Display="Static" ErrorMessage="*" runat=server/></td>
                </tr>
                <tr>
                <td>Persistent Cookie:</td>
                <td><ASP:CheckBox id=PersistCookie runat="server" /> </td>
                <td></td>
                </tr>
        </table>
        <asp:button text="Login" OnClick="Login_Click" runat=server/>
        <p>
        <asp:Label id="Msg" ForeColor="red" Font-Name="Verdana" Font-Size="10" runat=server />
</form>
</body>
</html>
