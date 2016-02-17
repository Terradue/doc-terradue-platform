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
			Helpers.addPasswordValidationMethods();
		},
		
		index: function (options) {
			var self = this;
			
			this.data = new can.Observe({
			});
			
			var token = Helpers.getUrlParameters().token;
			
			if (token)
				this.view({
					url: 'modules/passwordreset/views/passwordreset2.html',
					data: this.data,
					fnLoad: function(){
						self.initFormValidator2();
					}
				});
			else
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
			this.element.find('form.passwordResetForm').validate({
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
					errorMessage: Helpers.getErrMsg(xhr, 'Unable to reset the password. Please contact the Administrator.'),
				});
			});

			return false;
		},
		
		initFormValidator2: function(){
			var self = this;
			this.element.find('form.passwordResetForm2').validate({
				rules : {
					username: 'required',
					password: {
						required: true,
						minlength: 8,
						atLeastOneUpper: true,
						atLeastOneLower: true,
						atLeastOneNumber: true,
						atLeastOneSpecialChar: true,
						noOtherSpecialChars: true,
					},
					passwordRepeat: {
						equalTo: '#newPassword',
					},
					'g-recaptcha-response': 'required'
				},
				messages : {
					username: 'Enter your username',
					password: {
						required: 'Create a password',
						minlength: 'Password must be at least 8 characters',
						atLeastOneUpper: 'Password must include at least one uppercase character',
						atLeastOneLower: 'Password must include at least one lowercase character',
						atLeastOneNumber: 'Password must include at least one number',
						atLeastOneSpecialChar: 'Password must include at least one special character in the list !@#$%^&*()_+',
						noOtherSpecialChars: 'Password can\'t include special characters different from the list !@#$%^&*()_+',
					},
					passwordRepeat: 'The password is not equal with the first'
				},
				submitHandler: function(form){
					self.submitForm2(form);
				}
			});
			
			this.element.find('form [name="password"]').popover({
				trigger: 'focus',
				placement: 'left',
				title: 'Password',
				html: true,
				content: 'It must have:<ul>'
					+'<li>at least 8 characters</li>'
					+'<li>at least one uppercase character</li>'
					+'<li>at least one lowercase character</li>'
					+'<li>at least one number</li>'
					+'<li>at least one special character, chosen from the list: ! @ # $ % ^ & * ( ) _ +</li>'
					+'<li>no other special characters are permitted</li>'
					+'</ul>',
			});
		},


		submitForm2: function(form) {
			var self = this;
			var userData = Helpers.retrieveDataFromForm(form, ['username', 'password']);
			userData.token = Helpers.getUrlParameters().token;
			
			this.data.attr({
				loading: true, errorMessage: null, success: false,
			});
			PasswordResetModel.resetPassword(userData).then(function(){
				self.data.attr({
					loading: false, 
					success: true,
				});
			}).fail(function(xhr){
				self.data.attr({
					loading: false, 
					errorMessage: Helpers.getErrMsg(xhr, 'Unable to reset the password. Please contact the Administrator.'),
				});
			});

			return false;
		},
		
	}
);

return new PasswordResetControl(Config.mainContainer, {});
	
});
