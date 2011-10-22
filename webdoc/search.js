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
