
define([
	'jquery',
	'can',
	'utils/baseControl',
	'config',
	'modules/login/models/login',
	'loadmask'
], function($, can, BaseControl, Conf, LoginModel){
	
	var ADMIN = 4;
	
	var LoginControl = BaseControl.extend({}, {
		
		init: function($element, options){
			// init current user view
			this.User = new can.Observe({});
			this.isLoginOpen = false;
			
			if (options.showLoginMenu)
				$("#loginDiv").html(can.view("modules/login/views/login.html", this.User));
			
			var self=this;
			this.isLoggedDeferred = LoginModel.isLogged(function(user){
				user.attr('isAdmin', user.Level==ADMIN);
				self.User.attr({ current: user });
				$('.dropdown-toggle').dropdown();
			}).fail(function(xhr){
				self.User.attr({ noLogged: true });
				$('.dropdown-toggle').dropdown();
			});
		},
		
		'.login click': function(){
//			if (location.pathname!='/portal/signin')
//				document.location = '/portal/signin?back='+encodeURIComponent(location.pathname);
		},
		
//		'#loginButton click': function(){
//			this.doLogin();
//			return false;
//		},
//		
//		// TODO manage this
//		'#loginOpenidButton click': function(){
//			var url = "/" + Conf.api + '/auth/openId?provider=t2openid&url=' + document.location.origin;
//			document.location = url;
//		},
//		
		'#loginDiv .logout click': function(sender, e) {
			var self=this;
			var jqXHR = LoginModel.logout();
			jqXHR.then(function(){
				self.User.attr({noLogged: true, current:null});
				if (jqXHR.status==204){
					var location = jqXHR.getResponseHeader('Location');
					var hash = can.route.attr('hash');
					if (location){
						window.location.replace(location); // redirect
					} else{
					}
					return true;
				} else
					return false;
			});
			
			return false;
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
