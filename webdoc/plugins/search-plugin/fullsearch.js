//support for a full page of search results /monodoc.ashx?search=

var spinner = $('#s_spinner');
function process_hash () {
	var query = window.location.hash;
	if (query == null || query.length < 2)
		return;
	var ul = $('#s_results ul');
	ul.empty ();
	var currentNumber = 0;
	var count = 0;
	// Remove hash '#' symbol
	query = query.substring(1);
	$('#s_term').text (query);
	var fetch_and_add_results = function (url) {
		spinner.toggleClass ('hidden');
			$.getJSON (url, function (data) {
				spinner.toggleClass ('hidden');
				count = data.count;
				currentNumber += data.result.length;
				if (data.result.length == 0) {
					$('<div/>', { 'class': 's_message' }).text('No more results').replaceAll($('#s_morebtn')).fadeOut(4000, function () { $(this).remove(); });
				} else {
					var lis = $.map (data.result, function (element) {
						return '<li><a href="/monodoc.ashx?link=' + element.url + '"><span class="name">'
							+ element.name + '</span> '
							+ (element.fulltitle.length > 0 ? '<span class="fulltitle">(' + element.fulltitle + ')</span>' : '') + '</a></li>';
					});
					ul.append (lis.join (''));
				}
			});
	};
 	fetch_and_add_results ('/monodoc.ashx?search=' + query + '&callback=?');
	$('#s_morebtn input').click (function () {
		fetch_and_add_results ('/monodoc.ashx?search=' + query + '&start=' + currentNumber + '&count=' + count + '&callback=?');
	});
}

process_hash ();
window.addEventListener("hashchange", process_hash, false);
