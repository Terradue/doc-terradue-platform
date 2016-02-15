define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'modules/login/models/login',
	'modules/signin/models/signin',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, LoginModel, SigninModel, bootbox){
	
var SigninControl = BaseControl(
{ defaults: { fade: 'slow' }, },
{
	// init
	init: function (element, options) {
	},
	
	index: function (options) {
		var self = this;
		var element = this.element;

		this.data = new can.Observe({
			showSigninForm: true, // TODO maybe not at startup
		});
		
		window.data = this.data; // check
		
		this.firstTime = true;
		
		this.view({
			url: 'modules/signin/views/signin.html',
			data: this.data,
			fnLoad: function(){
				self.$signinForm = element.find('form.signinForm');
				self.$consentForm = element.find('form.consentForm');
				self.$username = element.find('form.signinForm input[name="username"]');
				self.$password = element.find('form.signinForm input[name="password"]');

				self.initFormValidator();
				self.initSSO();
			}
		});
	},
	
	initFormValidator: function(){
		var self = this;
		this.$signinForm.validate({
			rules : {
				username: 'required',
				password: 'required',
			},
			messages : {
				username: 'Enter your username',
				password: 'Enter your password',
			},
			submitHandler: function(form){
				try {
					self.submitSigninForm(form);
				} catch (e){
					console.log(e);
					window.e=e;
					return false;
				}
			}
		});
		
	},
	
	initSSO: function(){
		if (this.firstTime){
			this.loginFormInteractions();
			this.firstTime=false;
		}

		this.clearUI();

		// Get the query string with the encoded OpenID Connect authentication
		// request
		var queryString = location.search;
		this.log("Query string: " + (queryString ? queryString : "none"));
		
		if (! queryString) {
			var msg = "Invalid OpenID Connect authentication request: Missing query string";
			this.log(msg);
			this.displayErrorMessage('Seems that something is wrong with your request', msg);
			return;
		}
	},
	
	loginFormInteractions: function(){
		var self = this;
		var updateSubmitStatus = function(){
			self.data.attr({
				signinErrorMessage: '',
				isSubmitEnabled: (self.$username.val()!='' && self.$password.val()!='')
			});
			//$submit.prop('disabled', ($username.val()=='' || $password.val()==''));
		};
		
		this.$username.keyup(updateSubmitStatus);
		this.$password.keyup(updateSubmitStatus);
	},
	
	clearUI: function() {
		
	    this.$username.val('');
	    this.$password.val('');
	    
	    this.data.attr({
	    	signinErrorMessage: null,
	    	errorCode: null,
	    	errorDescription: null,
	    	signinLoading: false,
	    	consentLoading: false,
	    	disabledConsentDeny: false,
	    	disabledConsentAllow: false,
	    	isSubmitEnabled: false
	    });
	},
	
	
	displayErrorMessage: function(shortMsg, longMsg){
		
		this.errorView({}, shortMsg, longMsg, true);

//	    if (isModal)
//	    	this.element.find('#errorMessageModal').modal();
//	    else
//	    	this.data('showErrorMessage', true);
	    	//this.element.find('#errorMessage').show();
	},
	
	
	//Authenticates the subject by checking the entered username and password with
	//the LdapAuth JSON-RPC 2.0 service
	submitSigninForm: function(){
		var self = this;
		var data = this.data;

		var username = this.$username.val();
		var password = this.$password.val();
		
		this.log("Entered username: " + username);
		this.log("Entered password: *****");// + password);
		
		if (username.length === 0 || password.length === 0) {
			this.data.attr('signinErrorMessage', 'You must enter a username and a password');
			this.$username.focus();
			return false;
		}
		
		// Clear login form
		this.$username.val("");
		this.$password.val("");
		
		data.attr({
			signinLoading: true,
			isSubmitEnabled: false
		});
		
		var signinInfo = {
			username: username,
			password: password,
			query: Helpers.getUrlParameters().query,
		}
		SigninModel.signin(signinInfo).then(function(json, textStatus, jqXHR){
			var isRedirected = self.redirectToCallback(jqXHR);
			if (isRedirected)
				return;
			
			if (json && json.type=='consent')
				self.showConsentForm(json);
			
		}).fail(function(xhr){
			window.res = xhr;
			data.attr('signinErrorMessage', 'Signin failed. Wrong username or password.'); // TODO improve messages
		}).always(function(){
			data.attr('signinLoading', false);
		});
		
		return false; // avoid submit
	},
	
	
	showConsentForm: function(json){
		this.data.attr({
			showSigninForm: false,
			showConsentForm: true,
			consentData: json,
		});
	},
	
	'.consentForm submit': function(element){
		if (!element.prop('disabled'))
			return false;
		
		var self = this;
		var newScopes = $('.newScopes input[type="checkbox"]:checked').map(function(){return $(this).attr('name')});
		var consentInfo = {
			query: Helpers.getUrlParameters().query,
			//scope: newScopes,
			scope: ['openid'] // todo take from form
		};
		
		data.attr({
			consentLoading: true,
			disabledConsentAllow: true,
			disabledConsentDeny: true
		});
		
		SigninModel.consent(consentInfo).then(function(data, textStatus, jqXHR){
			self.redirectToCallback(jqXHR);
		}).fail(function(jqXHR){
			self.displayErrorMessage('Consent submit failed.', Helpers.getErrMsg(jqXHR));
		}).always(function(){
			data.attr('consentLoading', false);
		});
		
		return false;
	},
	
	'.newScopes input[type="checkbox"] click': function(){
		var isDisabled = ($('.newScopes input[type="checkbox"]:not(:checked)').length > 0);
		this.data.attr('disabledConsentAllow', isDisabled);
	},
	
	'.denyBtn click': function(){
		var self = this;
		this.data.attr({
			consentLoading: true,
			disabledConsentAllow: true,
			disabledConsentDeny: true
		});
		SigninModel.denyConsent().then(function(data, textStatus, jqXHR){
			self.redirectToCallback(jqXHR);
		}).fail(function(jqXHR){
			self.displayErrorMessage('Consent submit failed.', Helpers.getErrMsg(jqXHR));
		}).always(function(){
			self.data.attr('consentLoading', false);
		});
	},
	
	
	redirectToCallback: function(jqXHR){
		if (jqXHR.status==204){
			var location = jqXHR.getResponseHeader('Location');
			if (location){
				this.log("Received redirection URI: " + location);
				window.location.replace(location); // redirect
			} else{
				this.log("Missing Location response header");
				this.displayErrorMessage('Something went wrong.', 'Missing Location response header.');
			}
			return true;
		} else
			return false;
	},
	
	
	log: function(msg) {
		console.log(msg);
		//var txt = $("#console textarea");
		//txt.val( txt.val() + msg + "\n");
	}

	
}
);

return new SigninControl(Config.mainContainer, {});
	
});
