define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	//'modules/signup/models/signup',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, peopleData, bootbox){
	
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
			this.setupValidation();
		},
		
		// first level page : /newpeoplesimple
		index: function (options) {
			var self = this;
			this.view({
				url: 'modules/signup/views/signup.html',
				fnLoad: function(){
					self.initForm();
				}
			});
		},
		
		initForm: function(){
			this.element.find('form').validate({
				rules : {
					Email: "email",
				},
				messages : {
					Email: "The username is required",
				},
				submitHandler: function(form){
					self.submitForm(form);
				}
			});
			
			this.element.find('#inputPassword').popover({
				trigger: 'focus',
				placement: 'left',
				title: 'Password',
				html: true,
				content: "It must have:<ul>"
					+"<li>at least 8 characters</li>"
					+"<li>at least one uppercase character</li>"
					+"<li>at least one lowercase character</li>"
					+"<li>at least one number</li>"
					+"<li>at least one special character, chosen from the list: ! @ # $ % ^ & * ( ) _ +</li>"
					+"<li>no other special characters are permitted</li>"
					+"</ul>",
			});

		},
		
		submitForm: function(form) {
			alert('form submitted!');
			alert(PASSWORD_AT_LEAST_ONE_UPPER_CASE);
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
				"* atLeastOneUpper"
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
