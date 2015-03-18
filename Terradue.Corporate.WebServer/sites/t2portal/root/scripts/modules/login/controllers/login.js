
define([
	'jquery',
	'can',
	'utils/baseControl',
	'config',
	'modules/login/models/login',
	'loadmask'
], function($, can, BaseControl, Conf, LoginModel){
	
	var LoginControl = BaseControl.extend({}, {
		
		init: function($element, options){
			// init current user view
			this.User = new can.Observe({});
			this.isLoginOpen = false;
			
			if (options.showLoginMenu)
				$("#loginDiv").html(can.view("modules/login/views/login.html", this.User));
			
			this.view({
				selector: "#loginFormContainer",
				url: 'modules/login/views/loginForm.html',
			});
			
			var self=this;
			this.isLoggedDeferred = LoginModel.isLogged(function(user){
				self.User.attr({ current: user });
				$('.dropdown-toggle').dropdown();
			}).fail(function(xhr){
				self.User.attr({ noLogged: true });
				$('.dropdown-toggle').dropdown();
			});
			
			window.LoginModel = LoginModel;
		},
		
		'#loginDiv .login click': function(sender, e) {
			if (this.isLoginOpen)
				this.closeLoginForm();
			else 
				this.openLoginForm();
			return false;
		},

		'#loginButton click': function(){
			this.doLogin();
			return false;
		},
		
//		'.modal .submit-login click': function(sender, e) {
//			e.preventDefault();
//			this.doLogin();
//		},
		
		// TODO manage this
		'#loginOpenidButton click': function(){
			var url = "/" + Conf.api + '/auth/openId?provider=t2openid&url=' + document.location.origin;
			document.location = url;
		},
		
		'#loginDiv .logout click': function(sender, e) {
			var self=this;
			LoginModel.logout(function(){
				self.User.attr({noLogged: true, current:null});
				document.location = "/";
			});
			
			return false;
		},
		
		
		openLoginForm: function(){
			$("#loginFormContainer").animate({height: 188});
			this.isLoginOpen = true;
		},
		closeLoginForm: function(){
			$("#loginFormContainer").animate({height: 0});
			this.isLoginOpen = false;
		},		
		
		doLogin: function(){
			// get user and password
			var self = this,
				usr = {
					username : $("#loginForm input[name='username']").val(),
					password : $("#loginForm input[name='password']").val()
				},
				$errorMsg = $("#loginForm .text-error").empty();
			
			if (usr.username!="" && usr.password!=""){
				$("#loginForm").mask("login...");
				LoginModel.login(usr, function(){
					$("#loginForm").unmask();
//					self.closeLoginForm();
//					self.init();
					document.location.reload();
//					self.hideModal();
				}).fail(function(){
					$("#loginForm").unmask();
					$errorMsg.text("Invalid username or password.");
				});
			} else {
				$errorMsg.text("Insert username and password.")
			}
		},
		
		isLogged: function(){
			return (this.User && !this.User.noLogged);
		},
		
		isAdmin: function(){
			return (this.User && !this.User.noLogged && this.User.current && this.User.current.attr('Level') && this.User.current.attr('Level')==4);
		},
		
	});
	
	return LoginControl;
	
});
