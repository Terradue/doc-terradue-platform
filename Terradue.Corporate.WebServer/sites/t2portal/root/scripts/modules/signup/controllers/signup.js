define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'modules/signup/models/signup',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, SignupModel, bootbox){
	
var SignupControl = BaseControl(
	{
		defaults: { fade: 'slow' },
		EMAIL_REQUIRED: 'Insert your email',
		EMAIL_WELL_FORMED: 'The provided email is not valid',
	    PASSWORD_MIN_LENGTH: 'Password must be at least 8 characters',
	    PASSWORD_REQUIRED: 'Create a password',
	    PASSWORD_AT_LEAST_ONE_UPPER_CASE: 'Password must include at least one uppercase character',
	    PASSWORD_AT_LEAST_ONE_LOWER_CASE: 'Password must include at least one lowercase character',
	    PASSWORD_AT_LEAST_ONE_NUMBER: 'Password must include at least one number',
	    PASSWORD_AT_LEAST_SPECIAL_CHAR: 'Password must include at least one special character in the list !@#$%^&*()_+',
	    PASSWORD_NO_OTHER_CHAR: 'Password can\'t include special characters different from the list !@#$%^&*()_+',
	    PASSWORD_CONFIRM: 'The password is not equal with the first',
	},
	{
		// init
		init: function (element, options) {
			window.Helpers=Helpers;
			this.setupValidation();
		},
		
		// first level page : /newpeoplesimple
		index: function (options) {
			var self = this;
			
			this.data = new can.Observe({
				recaptchaPublicKey: Config.recaptchaPublicKey
			});
			App.Login.isLoggedDeferred.then(function(user){
				self.data.user = user;
			});

			this.view({
				url: 'modules/signup/views/signup.html',
				data: this.data,
				fnLoad: function(){
					self.initForm();
				}
			});
		},
		
		initForm: function(){
			var self = this;
			this.element.find('form').validate({
				rules : {
					email: {
						required: true,
						email: true,
					},
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
						equalTo: '#signupPassword',
					},
					'g-recaptcha-response': 'required',
				},
				messages : {
					email: {
						required: 'Insert your email',
						email: 'The provided email is not valid',
					},
					password: {
						required: 'Create a password',
						minlength: 'Password must be at least 8 characters',
						atLeastOneUpper: 'Password must include at least one uppercase character',
						atLeastOneLower: 'Password must include at least one lowercase character',
						atLeastOneNumber: 'Password must include at least one number',
						atLeastOneSpecialChar: 'Password must include at least one special character in the list !@#$%^&*()_+',
						noOtherSpecialChars: 'Password can\'t include special characters different from the list !@#$%^&*()_+',
					},
					passwordRepeat: 'The password is not equal with the first',
				},
				submitHandler: function(form){
					self.submitForm(form);
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
		
		submitForm: function(form) {
			var self = this,
				userData = Helpers.retrieveDataFromForm(form, ['email', 'password']),
				captchaValue = this.element.find('[name="g-recaptcha-response"]').val();
			
			if (!captchaValue){
				this.data.attr('recaptchaError', true);
				return false;
			}
			this.data.attr({
				loading: true, errorMessage: null, success: false, recaptchaError:null,
			});
			userData.captchaValue = captchaValue;
			new SignupModel(userData).save().then(function(){
				document.location.reload();
			}).fail(function(xhr){
				self.data.attr({
					loading: false, 
					errorMessage: Helpers.getErrMsg(xhr, 'Unable to register. Please contact the Administrator.'),
				});
			});
			return false;
		},
		
		setupValidation: function(){
			// Validator extensions
			$.validator.addMethod(
				"atLeastOneUpper",
				function(value, element) {
					return this.optional(element) || new RegExp("[A-Z]").test(value);
				},
				"* atLeastOneUpper"
			);

			$.validator.addMethod(
				"atLeastOneLower",
				function(value, element) {
					return this.optional(element) || new RegExp("[a-z]").test(value);
				},
				"* atLeastOneUpper"
			);

			$.validator.addMethod(
				"atLeastOneNumber",
				function(value, element) {
					return this.optional(element) || new RegExp("[\\d]").test(value);
				},
				"* atLeastOneNumber"
			);

			$.validator.addMethod(
				"atLeastOneSpecialChar",
				function(value, element) {
					return this.optional(element) || new RegExp("[!#@$%^&*()_+]").test(value);
				},
				"atLeastOneSpecialChar"
			);

			$.validator.addMethod(
				"noOtherSpecialChars",
				function(value, element) {
					return this.optional(element) || new RegExp('^[a-zA-Z0-9!#@$%^&*()_+]+$').test(value);
				},
				"Please remove special characters"
			);			
		}
		
	}
);

return new SignupControl(Config.mainContainer, {});
	
});
