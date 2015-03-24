define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, bootbox){
	
var PasswordResetControl = BaseControl(
	{ defaults: { fade: 'slow' }, },
	{
		// init
		init: function (element, options) {
		},
		
		index: function (options) {
			var self = this;
			
			this.data = new can.Observe({
			});

			this.view({
				url: 'modules/passwordreset/views/passwordreset.html',
				data: this.data,
			});
		},

		submitForm: function(form) {
			var self = this,
				userData = Helpers.retrieveDataFromForm(form, ['username']);
			
			this.data.attr({
				loading: true, errorMessage: null, success: false,
			});

			return false;
		},
		
	}
);

return new PasswordResetControl(Config.mainContainer, {});
	
});
