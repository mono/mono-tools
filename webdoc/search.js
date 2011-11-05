var search_input = $('#fsearch');
var search_window = $('#fsearch_window');
var content_frame = $('#content_frame');
var page_link = $('#pageLink');

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
    search_window.css ({'display' : 'block', 'height' : 'auto', 'opacity' : 1.0});
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
			if (item.length > 35) {
				item = item.substring (0, 35);
				item += '<span class="threedots">...</span>';
			}
			items.push('<li><a href="#" onclick="change_page(\''+val.url+'\')" title="'+val.name+'">' + item + '</a></li>');
		});

		var uls = $('<ul/>', { html: items.join (''), 'style': 'list-style-type:none; margin: 0; padding:0' });
		search_window.empty();
		uls.appendTo ('#fsearch_window');
		show ();
	};
	$.getJSON ('monodoc.ashx?fsearch=' + $(this).val (), callback);
});
