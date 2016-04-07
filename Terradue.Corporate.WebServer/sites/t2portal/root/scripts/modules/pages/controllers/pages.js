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
    	'modules/login/controllers/login',
    	'skrollr',
    	'loadmask',
    	'bootstrapHoverDropdown'
], function($, _, can, App, Config, BaseControl, LoginControl, skrollr){
	
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

					// set dropdown hover to the menu
					$('#menu .dropdown-toggle').dropdownHover({
						delay: 100
					});
				});
				
				// set login public accessible
				App.Login = new LoginControl(document, {
					showLoginMenu: true,
				});
				
			},
			
			initMenu: function() {
				_.mixin(_.str.exports());
				
				var activated = false;
				
				//ENSURE NAV PARAM IS A JQUERY OBJECT
				var $el = $('#menu li');
				// remove each previous active link
				$el.removeClass('active');
				
				//ACTIVATE NAV BUTTON
				$el.each(function() {
					// GET LINK AND COMPARE AGAINST URL
					var url = $('a', this).attr('href');
					var location = window.location.pathname;
					var hash = window.location.hash;
					
					// aearch routes mathing nav url link
					if (location+hash === url) {
						// found
						activated = true;
						
						// set class active
						$(this).addClass('active');
						
						// if is a submenu, set class active to topmenu too
						if ($(this).parent().hasClass('dropdown-menu'))
							$(this).parent().parent().addClass('active');
						
						return false; // stop each search
					}
				});
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
						
						if (options.fnLoad)
							options.fnLoad(el);
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

