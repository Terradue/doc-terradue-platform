define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'modules/login/models/login',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, LoginModel, bootbox){
	
var SigninControl = BaseControl(
	{ defaults: { fade: 'slow' }, },
	{
		// init
		init: function (element, options) {
		},
		
		// first level page : /newpeoplesimple
		index: function (options) {
			var self = this;
			
			this.data = new can.Observe({
			});
			App.Login.isLoggedDeferred.then(function(user){
				self.data.attr('user', user);
			});

			this.view({
				url: 'modules/signin/views/signin.html',
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
					password: 'required',
				},
				messages : {
					username: 'Enter your username',
					password: 'Enter your password',
				},
				submitHandler: function(form){
					self.submitForm(form);
				}
			});
			
		},
		
		submitForm: function(form) {
			var self = this,
				userData = Helpers.retrieveDataFromForm(form, ['username', 'password']);
			
			this.data.attr({
				loading: true, errorMessage: null, success: false,
			});
			LoginModel.login(userData, function(){
				var params = Helpers.getUrlParameters();
				if (params.back)
					document.location = decodeURIComponent(params.back);
				else
					document.location = '/portal/settings/profile';
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

return new SigninControl(Config.mainContainer, {});
	
});
