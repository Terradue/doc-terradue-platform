/**
 * Controller to manage the page logic and static pages
 */

define([
    	'jquery',
    	'underscore',
    	'can',
    	'app',
    	'config',
    	'utils/baseControl',
    	'utils/menu',
    	'modules/login/controllers/login',
    	'skrollr',
    	'loadmask',
], function($, _, can, App, Config, BaseControl, Menu, Login, skrollr){
	
if (App.controllers.Pages==null)
	App.controllers.Pages = new (BaseControl(
		{ defaults: {	fade: 'slow' } },
		{
			// init
			init: function (element, options) {
				
				var self = this;
				$(function(){
					// on page ready
					self.initMenu();
				});
				
				// set login public accessible
				App.Login = Login;
				
				// set dropdown
				$('.dropdown-toggle').dropdown();
			},
			
			initMenu: function () {
				//Menu.activate('header ul.nav li');
			},
			
			// actions
			index: function (options) {
				var self=this;
				
				// load base index page
				self.view({
					url: Config.firstPage ? Config.firstPage : 'modules/pages/views/index.html',
							selector: Config.mainContainer,
							fade: false,
				});				
				
				// login
				console.log('init login...');
				App.Login.isLoggedDeferred.always(function(user){
					if (user.state && user.state()=='rejected')
						user = {};
					
					console.log('...init login done.');
				});
			},
			
			load: function (options) {
				this.view({
					url: options.url ? options.url
							: 'modules/pages/views/' + options.id + '.html',
					selector: options.selector || Config.mainContainer,
					fnLoad: function(el){
						window.skrollr=skrollr;
						skrollr.init({
							forceHeight: false
						});
						$('.dropdown-toggle').dropdown();
					}
				});
			},
			
			loadStatic: function (url) {
				this.view({
					url: url+'.html',
					selector: Config.mainContainer,
				});
			},		
			
		}
	))(document);
	
return App.controllers.Pages;
	
});

