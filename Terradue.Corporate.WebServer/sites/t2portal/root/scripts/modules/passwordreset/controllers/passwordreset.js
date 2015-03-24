define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'bootbox',
	'modules/passwordreset/models/passwordreset',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, bootbox, PasswordResetModel){
	
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
				fnLoad: function(){
					self.initFormValidator();
				}
			});
			
		},
		
		initFormValidator: function(){
			var self = this;
			this.element.find('form').validate({
				rules : {
					username: 'required',
				},
				messages : {
					username: 'Enter your username',
				},
				submitHandler: function(form){
					self.submitForm(form);
				}
			});
			
		},


		submitForm: function(form) {
			var self = this,
				userData = Helpers.retrieveDataFromForm(form, ['username']);
			
			this.data.attr({
				loading: true, errorMessage: null, success: false,
			});
			new PasswordResetModel(userData).save()
			.then(function(){
				self.data.attr({
					loading: false, 
					success: true,
				});
			}).fail(function(xhr){
				self.data.attr({
					loading: false, 
					errorMessage: Helpers.getErrMsg(xhr, 'Unable to sign-in. Please contact the Administrator.'),
				});
			});

			return false;
		},
		
	}
);

return new PasswordResetControl(Config.mainContainer, {});
	
});
