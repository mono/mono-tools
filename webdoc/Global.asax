<%@ Application ClassName="Mono.Website.Global" %>
<%@ Import Namespace="Monodoc" %>
<%@ Assembly name="monodoc" %>

<script runat="server" language="c#" >
public static RootTree help_tree;

void Application_Start ()
{
	help_tree = RootTree.LoadTree ();
}

</script>