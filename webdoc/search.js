var search_input = $('#fsearch');
var search_window = $('#fsearch_window');
var content_frame = $('#content_frame');
var page_link = $('#pageLink');
var lis = null;

change_page = function (pagename) {
    content_frame.attr ('src', 'monodoc.ashx?link=' + pagename);
    page_link.attr ('href', '?link=' + pagename);
};
page_link.attr ('href', document.location.search);

var is_shown = false;
var hide = function () {
	if (!is_shown)
		return;
	search_window.css ({'display' : 'none', 'opacity' : 0});
	is_shown = false;
};
var show = function () {
	if (is_shown)
		return;
    search_window.css ({'display' : 'block', 'height' : 'auto', 'opacity' : 1.0, 'width': search_input.width() + 'px'});
	is_shown = true;
};

search_input.blur (function () { window.setTimeout (hide, 200); if (search_input.val ().length == 0) search_input.css ('width', '19em'); });
search_input.focus (function () { search_input.css ('width', '29em'); if (search_window.text().length > 0 && search_input.val().length > 0) show (); });
search_input.keyup (function (event) {
	if ($(this).val () == "")
		hide();

    // Only process if we receive an alnum or return or del
    if (event.which != 8 && event.which != 46
        && (event.which < 'A'.charCodeAt(0) || event.which > 'Z'.charCodeAt(0))
        && (event.which < '0'.charCodeAt(0) || event.which > '9'.charCodeAt(0)))
        return;

	var callback = function (data) {
		if (data == null || data.length == 0)
			return;

		var items = [];

		$.each (data, function(key, val) {
			var item = val.name;
			items.push('<li><a href="#" onclick="change_page(\''+val.url+'\')" title="'+val.name+'">' + item + '</a></li>');
		});

		var uls = $('<ul/>', { html: items.join (''), 'style': 'list-style-type:none; margin: 0; padding:0' });
		lis = uls.children ('li');
		search_window.empty();
		uls.appendTo ('#fsearch_window');
		show ();
	};
	$.getJSON ('monodoc.ashx?fsearch=' + $(this).val (), callback);
});

document.getElementById ('fsearch').onsearch = function () {
	if (search_input.val () == "") {
		hide ();
		search_input.blur ();
	}
};

search_input.keydown (function (event) {
	if (lis == null)
		return;
	var selected = lis.filter('.selected');
	var newSelection = null;

	switch (event.which)
	{
	case 13: // return
		if (selected.length != 0) {
			selected.children ('a').click ();
			hide ();
			search_input.blur ();
		}
		return false;

	case 38: // up
		if (selected.length != 0) {
			var prev = selected.prev ();
			if (prev.length != 0)
				newSelection = prev;
		} else {
			newSelection = lis.last ();
		}
		break;
	case 40: // down
		if (selected.length != 0) {
			var next = selected.next ();
			if (next.length != 0)
				newSelection = next;
		} else {
			newSelection = lis.first ();
		}
		break;
	}

	if (newSelection != null) {
		newSelection.addClass ('selected');
		if (selected != null)
			selected.removeClass ('selected');
		selected = newSelection;
	}
});
