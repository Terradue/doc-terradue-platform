
define([
	'jquery',
	'can',
	'utils/baseControl',
	'config',
	'modules/cpanel/models/newsTumblr',
	'utils/helpers',
	'moment'
], function($, can, BaseControl, Config, NewsTumblrModel, Helpers){
	
	var Cpanel = BaseControl({
		defaults: { fade: 'slow' },
	}, {
		init: function(element, options){
			console.log("cpanel.init");
			this.newsTumblrDetails = new can.Observe({});
			this.selectedNewsTumblr = null;
			this.newsesTumblr = null;
			this.isLoginPromise = App.Login.isLoggedDeferred;
		},
		
		index: function(data){
			//alert(console.log(App.Login.isLoggedDeferred));
			var self = this;
			this.isLoginPromise.then(function(user){
				if (App.Login.isAdmin()){
					console.log("App.controllers.cpanl.index");
					self.element.html(can.view("modules/cpanel/views/index.html", {
						user: user,
					}));
				} else 
					self.accessDenied();
			}).fail(function(){
				self.accessDenied();
			});
		},
		
		retrieveFromForm: function(){
			return {
				Title: this.element.find('input[name="Title"]').val(),
				Name: this.element.find('input[name="Name"]').val(),
				Abstract: this.element.find('input[name="Abstract"]').val(),
				Author: this.element.find('input[name="Author"]').val(),
				Url: this.element.find('input[name="Url"]').val(),
			};
		}
		
	});
	
	return new Cpanel(Config.mainContainer, {});
	
});
