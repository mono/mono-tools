var search_input = $('#fsearch');
var search_window = $('#fsearch_window');
var lis = null;
var page_top_offset = $('#main_part').offset().top;

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
    search_window.css ({'display' : 'block', 'height' : 'auto', 'opacity' : 1.0, 'width': search_input.width() + 'px', 'top': page_top_offset + 'px' });
	is_shown = true;
};

var param = document.URL.split('#')[1];
if(param) {
    $('#content_frame').attr('src', '/plugins/search-plugin/fullsearch/search.html#' + param);
}

search_input.blur (function () {
	window.setTimeout (hide, 200);
	if (search_input.val ().length == 0)
		search_input.css ('width', '19em');
});
search_input.focus (function () {
	search_input.css ('width', '29em');
	if (search_window.text().length > 0 && search_input.val().length > 0)
		show ();
	window.setTimeout (function () {
		search_input[0].select ();
	}, 10);
});

search_input.keyup (function (event) {
	if ($(this).val () == "")
		hide();

    // Only process if we receive an alnum or backspace or del
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
			var url = val.url.replace (/[<>]/g, function (c) { return c == '<' ? '{' : '}'; });
			items.push('<li><a href="#" onclick="change_page(\''+url+'\')" title="'+(val.fulltitle == '' ? val.name : val.fulltitle)+'">' + item + '</a></li>');
		});

		var uls = $('<ul/>', { html: items.join (''), 'style': 'list-style-type:none; margin: 0; padding:0' });
		lis = uls.children ('li');
		var companion = $('#fsearch_companion');
		lis.hover (function () {
			var childA = $(this).children('a');
			var offset = childA.offset ();
			companion.css ({ 'top': offset.top + 'px', 'right': $('html').outerWidth () - offset.left + 10, 'display': 'block'});
			companion.text(childA.attr ('title'));
		}, function () {
			companion.css ('display', 'none');
		});
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
	$('#fsearch_companion').css ('display', 'none');

	switch (event.which)
	{
	case 13: // return
		if (selected.length != 0) {
			selected.children ('a').click ();
		} else {
			// Show full search page
			$("#content_frame").attr('src', '/plugins/search-plugin/fullsearch/search.html#' + encodeURI(search_input.val ()));
		}
		hide ();
		search_input.blur ();
		return false;
	case 38: // up
		if (selected.length != 0) {
			var prev = selected.prev ();
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
		if (selected != null) {
			selected.removeClass ('selected');
			selected.mouseleave();
		}
		newSelection.mouseenter();
		selected = newSelection;
	}
});
