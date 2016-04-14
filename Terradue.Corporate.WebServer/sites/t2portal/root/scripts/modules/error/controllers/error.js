
define([
	'can',
	'config',
	'utils/baseControl',
], function(can, Config, BaseControl){
	
	var ErrorControl = BaseControl(
		{ defaults: { fade: 'slow' } }, // if you want a page transition effect
		{
			// init
			init: function (element, options) {
			},
			
			// first level page : /people
			index: function (options) {
				
				var msg = can.route.attr('msg');
				var longmsg = can.route.attr('longmsg');
				this.errorView({}, msg, longmsg, true);
				
			},
			
			
		}
	);
	
	return new ErrorControl(Config.mainContainer, {});
	
});
