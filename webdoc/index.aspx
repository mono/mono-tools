<%@ Page Language="C#" ClassName="Mono.Website.Index" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<html>
  <head>
    <title><%=GetTitle ()%></title>
    <link rel="stylesheet" type="text/css" href="common.css" media="all" title="Default Style"/>
    <style>
#fsearch_window {
  display: none;
  position: absolute;
  z-index: 60;
  text-align: left;
  width: 220px;
  right: 50px;
  top: 40px;
  border: 1px solid black;
  background: white;
  padding: 5px;
  overflow: hidden;
  -webkit-transition: height 500ms ease-in 0;
  -moz-transition: height 500ms ease-in 0;
  -o-transition: height 500ms ease-in 0;
}

#fsearch_window a {
  color: blue;
  text-decoration: none;
  font-family: monospace;
}

#fsearch_window .threedots {
  color: #333;
  font-size: x-small;
  font-family: sans-serif;
}

iframe {
  margin: 0;
  padding: 0;
  border: 0;
  font-size: 100%;
  font: inherit;
  vertical-align: baseline;
}

body,div {
     margin: 0;
     padding: 0;
}
img.toolbarsep {
        border: 0px;
        margin-bottom: 1px;
        margin-top: 1px;
        padding-top: 3px;
        padding-bottom: 3px;
        vertical-align: middle;
}

img.toolbar {
        border: 0px;
        margin: 1px;
        padding: 3px;
        vertical-align: middle;
}

img.toolbar:hover {
        border-left: 1px solid white;
        border-right: 1px solid #B1A598;
        border-top: 1px solid white;
        border-bottom: 1px solid #B1A598;
        margin: 0px;
}

img.toolbar:active {
        border-right: 1px solid white;
        border-left: 1px solid #B1A598;
        border-bottom: 1px solid white;
        border-top: 1px solid #B1A598;
}


#login {
        position: fixed;
        top: 0px;
        right: 0px;
        float: right;
        padding: 5px;
}

#rightSide {
        position: fixed;
        top: 2px;
        right: 0px;
        float: right;
        padding: 5px;
}

#header {
  background: #679EF1 url(mdocimages/headerbg.png) no-repeat 100% 50%;
  background-color: #679EF1;
  background-position: 100% 50%;
  background-repeat: no-repeat;
  border-bottom: 1px dotted #3363BD;
  color: black;
  height: 40px;
  margin-bottom: 0px;
  padding: 0px 0px 0px 15px;
  position: relative;
}
#header h1 {
   color: white;
   font-family: arial, helvetica, verdana, sans-serif;
   font-size: 22px;
   font-weight: bold;
   line-height: 1.8em;
   margin: 0;
}
body,div {
  margin: 0;
  padding: 0;
  border: 0;
  font-size: 100%;
  font: inherit;
  vertical-align: baseline;
}
    </style>
    <link type='text/css' rel='stylesheet' href='ptree/tree.css'/>
	<link type='text/css' rel='stylesheet' href='sidebar.css'/>
  </head>
  <body>
        <script language="c#" runat="server">
		public string GetTitle ()
		{
			return Global.help_tree.GetTitle (Request.QueryString ["link"]);
		}

                // Get the path to be shown in the content fram
                string getContentFrame()
                {
                        // Docs get shown from monodoc.ashx
                        string monodocUrl="monodoc.ashx";
                        string defaultParams="?link=root:";
                        NameValueCollection qStringParams=Request.QueryString;

                        // If no querystring params, show root link
                        if(!qStringParams.HasKeys())
                                return(monodocUrl+defaultParams);
                        // else, build query for the content frame
                        string nQueryString=monodocUrl+"?";
                        foreach(string key in qStringParams)
                                nQueryString+=(HttpUtility.UrlEncode(key)+"="+HttpUtility.UrlEncode(qStringParams[key]));
                        return nQueryString;
                }
        </script>

  <script language="C#" runat="server">
        void Page_Load (object sender, EventArgs e)
        {
		if (User.Identity.IsAuthenticated){
			login.NavigateUrl = "logout.aspx";
			login.Text = "Logged in as " + User.Identity.Name;
		} else {
			login.NavigateUrl = "javascript:parent.content.login (parent.content.window.location)";
			//login.Text = "Sign in / create account"; 
		}
        }
  </script>
<div style="color: rgb(255, 255, 255); background-color: #c0dda2;">
     <div id="header">
        <h1>Mono Documentation</h1>
     </div>
     <div id="dlogin">
       <asp:HyperLink id="login" runat="server"/>
     </div>

     <div id="rightSide">
       <label for="search">Search: </label>
       <input id="fsearch" type="search" placeholder="Enter search request" style="width:19em"/>
       <!-- <a href="javascript:parent.content.print();">
          <img class="toolbar" src="images/print.png" alt="Print" title="Print this document"/> 
       </a>-->
       <a id="pageLink" href="/">
          <img class="toolbar" src="images/link.png" width="24" height="24" alt="Link to this document" title="Link to this document"/>
       </a>
    </div>
    <div id="fsearch_window"></div>
</div>
<div>
        <div style="width:20%; height: 95%; float: left; border-right: 1px solid black; overflow-x: hidden; overflow-y: auto">
	      <div id='contents' class='activeTab'>
	         <div id='contentList'>
	         </div>
	      </div>
        </div>
        <div><iframe id="content_frame" style="width:79.9%; height: 95%;" src="<% =getContentFrame() %>"></iframe></div>
</div>
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
<script type="text/javascript">
var search_input = $('#fsearch');
var search_window = $('#fsearch_window');
var content_frame = $('#content_frame');
var page_link = $('#pageLink');

change_page = function (pagename) {
    content_frame.attr ('src', 'monodoc.ashx?link=' + pagename);
    page_link.attr ('href', '?link=' + pagename);
};
page_link.attr ('href', document.location.search);

var hide = function () { search_window.css ('display', 'none'); search_window.css ('height', 'auto'); };
var show = function () {
    search_window.css ('display', 'block');
    var height = document.getElementById ('fsearch_window').getBoundingClientRect ().height + 'px';
    search_window.css ('height', '0px')
    search_window.css ({'display': 'block', 'height' : height}); };

search_input.blur (function () { window.setTimeout (hide, 10); });
search_input.focus (function () { if (search_window.text().length > 0 && search_input.val().length > 0) show (); });
search_input.keyup (function (event) {
     hide();
     search_window.empty();

     // Only process if we receive an alnum or return or del
     if (event.which != 8 && event.which != 46
                      && (event.which < 'A'.charCodeAt(0) || event.which > 'Z'.charCodeAt(0))
                      && (event.which < '0'.charCodeAt(0) || event.which > '9'.charCodeAt(0)))
        return;
     $.getJSON ('monodoc.ashx?fsearch=' + $(this).val (),
      function (data) {
          if (data == null || data.length == 0)
             return;

          var items = [];

          $.each (data, function(key, val) {
                var item = val.name;
                if (item.length > 25) {
                   item = item.substring (0, 25);
                   item += '<span class="threedots">...</span>';
                }
                items.push('<li><a href="#" onclick="change_page(\''+val.url+'\')" title="'+val.name+'">' + item + '</a></li>');
          });

          $('<ul/>', { html: items.join (''), 'style': 'list-style-type:none; margin: 0; padding:0' }).appendTo ('#fsearch_window');
          show ();
     });
});
</script>
<script src='xtree/xmlextras.js'></script>
<script src='ptree/tree.js'></script>
<script src='sidebar.js'></script>
<script type="text/javascript">
var tree = new PTree ();
tree.strSrcBase = 'monodoc.ashx?tree=';
tree.strActionBase = '?link=';
tree.strImagesBase = 'xtree/images/msdn2/';
tree.strImageExt = '.gif';
tree.onClickCallback = function (url) { change_page (url); };
var content = document.getElementById ('contentList');
var root = tree.CreateItem (null, 'Documentation List', 'root:', '', true);
content.appendChild (root);
<% = Global.CreateTreeBootFragment () %>

update_tree = function () {
  var tree_path = $('#content_frame').contents ().find ('meta[name=TreePath]');
  if (tree_path.length > 0) {
     var path = tree_path.attr ('value');
     tree.ExpandFromPath (path);
  }
};
update_tree ();
content_frame.load (update_tree);
</script>
</body>
</html>
