define([
	'jquery',
	'can',
	'config',
	'utils/baseControl',
	'utils/helpers',
	'modules/login/models/login',
	'modules/signin/models/ssoConfig',
	'modules/signin/models/signin',
	'bootbox',
	'jqueryValidate'
], function($, can, Config, BaseControl, Helpers, LoginModel, SSOConfig, SigninModel, bootbox){
	
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
		
		this.authzSession = SSOConfig.authzSession;
		this.subjectSession = SSOConfig.subjectSession;
		this.subject = SSOConfig.subject;
		
		this.view({
			url: 'modules/signin/views/signin.html',
			data: this.data,
			fnLoad: function(){
				self.$signinForm = element.find('form.signinForm');
				self.$consentForm = element.find('form.consentForm');
				self.$username = element.find('#username');
				self.$password = element.find('#password');
				self.$submit = element.find('#signinButton');

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
		this.resetGlobalVars();

		// Get the query string with the encoded OpenID Connect authentication
		// request
		var queryString = location.search;
		this.log("Query string: " + (queryString ? queryString : "none"));
		
		if (! queryString) {
			var msg = "Invalid OpenID Connect authentication request: Missing query string";
			this.log(msg);
			this.displayErrorMessage({"error_description":msg});
			return;
		}
	},
	
	loginFormInteractions: function(){
		var self = this;
		var updateSubmitStatus = function(){
			self.data.attr({
				signinErrorText: '',
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
	    	signinErrorText: null,
	    	showErrorMessage: false,
	    	errorCode: null,
	    	errorDescription: null,
	    	signinLoading: false,
	    	consentLoading: false,
	    	isSubmitEnabled: false
	    });
	},
	
	//Resets the global vars
	resetGlobalVars: function() {
		this.authzSession.id = null;
		this.subjectSession.id = null;
		this.subject = {};
	},
	
	
	displayErrorMessage: function(form, isModal){
    	this.data({
    		errorCode: form.error,
    		errorDescription: form.error_description ? form.error_description : 'Invalid OpenID Connect request'
    	});

	    if (isModal)
	    	this.element.find('#errorMessageModal').modal();
	    else
	    	this.data('showErrorMessage', true);
	    	//this.element.find('#errorMessage').show();
	},
	
	sizeWindow: function(){
	    var targetWidth = 800;
	    var targetHeight = 600;

	    var currentWidth = $(window).width();
	    var currentHeight = $(window).height();

	    window.resizeBy(targetWidth - currentWidth, targetHeight - currentHeight);

	    $("body").css("overflow-y", "scroll");
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
			this.data.attr('signinErrorText', 'You must enter a username and a password');
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
		
		SigninModel.signin(username, password).then(function(json){
			if (json.type=='consent')
				self.showConsentForm(json);
		}).fail(function(xhr){
			window.res = xhr;
			alert('fail!');
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
			disableConsentButtons: false
		});
	},
	
	'.signoutBtn click': function(){
		//logoutSubject
	},
	
	'.consentForm submit': function(){
		var self = this;
		var newScopes = $('.newScopes input[type="checkbox"]:checked').map(function(){return $(this).attr('name')});
		var consentInfo = {
			query: Helpers.getUrlParameters().query,
			//scope: newScopes,
			scope: ['openid'] // todo take from form
		};
		
		data.attr({
			consentLoading: true,
			disableConsentButtons: true
		});
		
		SigninModel.consent(consentInfo, function(location){
			if (location){
				self.log("Received redirection URI: " + location);
				alert('consent post successful! redirect to...\n' + location);
				window.location.replace(location); // redirect
			} else{
				alert("Missing Location response header");
				log("Missing Location response header");
			}

		}).fail(function(){
			alert('consent post fail!');
		}).always(function(){
			data.attr('consentLoading', false);
		});
		
		return false;
	},
	
	'.denyBtn click': function(){
		//denyAuthorization
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
