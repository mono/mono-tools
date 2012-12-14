$(function () {
//a hack for sizing our iframe correctly
/*	var getHeight = function () {
	<!--
	var viewportwidth;
	var viewportheight;
	// the more standards compliant browsers (mozilla/netscape/opera/IE7) use window.innerWidth and window.innerHeight
	if (typeof window.innerWidth != 'undefined')
	{
		viewportwidth = window.innerWidth,
		viewportheight = window.innerHeight
	}
	// IE6 in standards compliant mode (i.e. with a valid doctype as the first line in the document)
	else if (typeof document.documentElement != 'undefined'
		&& typeof document.documentElement.clientWidth !=
		'undefined' && document.documentElement.clientWidth != 0)
	{
		viewportwidth = document.documentElement.clientWidth,
		viewportheight = document.documentElement.clientHeight
	}
	// older versions of IE
	else
	{
		viewportwidth = document.getElementsByTagName('body')[0].clientWidth,
		viewportheight = document.getElementsByTagName('body')[0].clientHeight
	}
	return viewportheight;
	//-->
	}

	var main_part = $('#main_part');
	var content_frame = main_part.find('#content_frame');
	var resize_mainpart = function () {
//		main_part.height (getHeight() - 75);
//		main_part.children('#side').css ('height', '100%');
		content_frame.css ('height', '97.5%');
	}
	
	var resizeTimer;
	$(window).resize(function() {
		clearTimeout(resizeTimer);
		resizeTimer = setTimeout(resize_mainpart, 100);
	});
	
	resize_mainpart ();*/
});
