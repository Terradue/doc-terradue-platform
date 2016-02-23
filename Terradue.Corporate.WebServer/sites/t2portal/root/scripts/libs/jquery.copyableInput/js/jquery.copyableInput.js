//
// COPYABLE INPUT - BY CERAS
//
// it requires zeroclipboard.js
//

(function($) {

$.fn.copyableInput = function(textToCopy, options){
	if (this.attr('data-copyableInput-enabled'))
		return;
	
	if (options.isButton)
		// use the button as content
		var $button = $(this);
	else {
		// init content
		var $div = $('<div class="input-append copyableInput">');
		
		if (!options.hideInput)
			var $input = $('<input class="span2 copyableInput-input" type="text" readonly="readonly">')
				.appendTo($div)
				.val(textToCopy)
				.css('min-width', options.minWidth ? options.minWidth : 110)
				.click(function(){
					$(this).select();
				});
		
		var $button = $('<button class="btn '+(options.btnClass ? options.btnClass : 'btn-small') +' copyableInput-button copyableInput-round"><i class="icon-copy"></i></button>')
			.appendTo($div);
		
		this.append($div);
	}
	
	$button.attr('data-clipboard-text', textToCopy);
	
	var zeroClient = new ZeroClipboard($button).on("aftercopy", function(event) {
		$button.attr('data-original-title', 'copied!').tooltip('show');
	});
	
	// set tooltip
	$button.tooltip({
		trigger: 'manual',
		title: options.title ? options.title : 'copy to clipboard',
		placement:'bottom'
	}).mouseover(function(){
 		$(this).attr('data-original-title', options.title ? options.title : 'copy to clipboard');
 		$button.tooltip('show');
 		//$('.copyableInput>button').attr('title', 'copy to clipboarda').tooltip('show');
	}).mouseout(function(){
		$button.tooltip('hide');
	});
	
	this.attr('data-copyableInput-enabled', 'true');
	return this;
};

}(jQuery));
