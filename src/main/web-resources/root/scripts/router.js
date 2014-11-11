define([
	'jquery',
	'can',
	'underscore',
	'app',
	'modules/pages/controllers/pages',
	'config',
	'utils/helpers',
	'underscorestring',
	'canroutepushstate'
], function($, can, _, App, Pages, Config, Helpers){
	// merge string plugin to underscore namespace
	_.mixin(_.str.exports());
	
	can.route.bindings.pushstate.root = "/portal/";
		
	var Router = can.Control ({
		init: function () {
			this.previousPage = null;
		},

		// ROUTES
		'route': 'index',
		
		'pages/:id&:selector route': 'pages',
		'pages/:id route': 'pages',
		
		':controller/:action/:id route': 'dispatch',
		':controller/:action route': 'dispatch',
		':controller route': 'dispatch',
		
		// ACTIONS
		
		// index
		index: function (data) {
			var self = this;
			
			// don't do anything if the page is the same of previous
			if (this.previousPage!=''){
				Pages.index({
					fade: false,
					fnLoad: function(){
						App.updateDynamicElements();
					},
				});
				this.previousPage = '';			
			}
		},
		
		// pages route action (dynamic open of static pages)
		pages: function (data) {
			// inner links management
			var pageUrl = 'pages/'+data.id;
			
			// don't do anything if the page is the same of previous
			if (this.previousPage!=pageUrl){
				Pages.load(data);
				this.previousPage = pageUrl;
			}
		},

		// rest route actions
		dispatch: function (data) {
			console.log('DATA', data);
			var me = this;
			
			// dispatch static
			var resPage = data.controller+(data.action ? '/'+data.action : '');
			
			// don't do anything if the page is the same of previous
			if (resPage==this.previousPage)
				return; 
			
			this.previousPage = resPage;
			
			// static pages management
			if (Config.staticPages[resPage])
				return this.dispatchStatic(resPage);
			
			var ControllerName = _.capitalize (data.controller);
			
			var actionName = data.action
					? data.action.charAt(0).toLowerCase() + _.camelize(data.action.slice(1))
					: 'index'; // default to index action

			// dinamically load controller
			App.loadController (ControllerName, function (controller) {
				// call action if applicable
				if (controller && controller[actionName]){
					controller[actionName](data);
					Helpers.scrollToTop();
				} else
					Pages.errorView({}, "Controller not found: "+ControllerName);
				
				App.updateDynamicElements();
			}, function(err){
				Pages.errorView({}, "Controller not found: "+ControllerName);
			});
		},
		
		dispatchStatic: function(resPage){
			var self = this;
			if (typeof(Config.staticPages[resPage])=='string')
				Config.staticPages[resPage] = { url: Config.staticPages[resPage] };
			if (Config.staticPages[resPage].selector==null)
				Config.staticPages[resPage].selector = Config.mainContainer;
			
			var fnLoad = Config.staticPages[resPage].fnLoad;
			Config.staticPages[resPage].fnLoad = function(){
				if (fnLoad)
					fnLoad();
				App.updateDynamicElements();
			}
			
			Pages.view(Config.staticPages[resPage]);
			return false;
		}
	});
	
	return {
		init: function() {
			var self = this;
			// route on document.ready
			$(function() {
				// deactivate routing until it's not instantiated
				//can.route.ready(false);

				// init
				self.router = new Router(document);

				// events
				can.route.bind('change', function(ev, attr, how, newVal, oldVal) {
					console.log(ev, attr, how, newVal, oldVal);
					// menu change
					if (how === 'set') Pages.initMenu();
					else if (attr=='route' && how=='add' && newVal==""){
						console.log("INDEX");
						Pages.initMenu();
					}					
				});
	
//				can.route.bind('to', function(ev, newVal, oldVal) {
//					console.log('TO', newVal, oldVal);
//				});
				
				// activate routing
				can.route.ready();
			});
		},
	};
});
