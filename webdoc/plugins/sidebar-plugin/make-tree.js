$(document).ready(function() {
	var content_frame = $('#content_frame');
	var page_link = $('#pageLink');

	change_page = function (pagename) {
    	content_frame.attr ('src', 'monodoc.ashx?link=' + pagename);
  	page_link.attr ('href', '?link=' + pagename);
    	if (window.history && window.history.pushState) {
       		window.history.pushState (null, '', '/?link=' + pagename);
	}  		
	};

	update_tree = function () {
  		var tree_path = $('#content_frame').contents ().find ('meta[name=TreePath]');
  		if (tree_path.length > 0) {
     			var path = tree_path.attr ('value');
     			tree.ExpandFromPath (path);
  		}
	};

	update_tree ();
	add_native_browser_link = function () {
        	var contentDiv = $('#content_frame').contents ().find ('div[class=Content]').first ();
        	if (contentDiv.length > 0 && contentDiv.attr ('id')) {
                	var id = contentDiv.attr ('id').replace (':Summary', '');
                	var h2 = contentDiv.children ('h2').first ();
                	if (h2.prev ().attr ('class') != 'native-browser')
                	h2.before ('<p><a class="native-browser" href="mdoc://' + encodeURIComponent (id) + '"><span class="native-icon"><img src="/views/images/native-browser-icon.png" /></span>Open in Native Browser</a></p>');
        	}
	};
	add_native_browser_link ();

	content_frame.load (update_tree);
	content_frame.load (add_native_browser_link);
});
