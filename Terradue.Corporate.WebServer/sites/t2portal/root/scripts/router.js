define([
	'jquery',
	'can',
	'underscore',
	'app',
	'modules/pages/controllers/pages',
	'config',
	'utils/helpers',
	'underscorestring',
	//'canroutepushstate'
], function($, can, _, App, Pages, Config, Helpers){
	// merge string plugin to underscore namespace
	_.mixin(_.str.exports());
	
	can.route.bindings.pushstate.root = "/portal/";
	//can.route.bindings.pushstate.querySeparator ="-";
		
	var Router = can.Control ({
		init: function () {},

		// ROUTES
		'route': 'index',
		'#:hash route': 'index',
		
		'pages/:id&:selector route': 'pages',
		'pages/:id route': 'pages',
		
		':controller/:action/:id route': function(data){
			this.dispatch(data)
		},
		':controller/:action route': function(data){
			this.dispatch(data)
		},
		':controller#:hash route': function(data){
			this.dispatch(data);
		},
		':controller route': function(data){
			this.dispatch(data);
		},

		// ACTIONS
		
		// index
		index: function (data) {

			if (this.checkHash(data))
				return; // if true you go to the hash, so doesn't change the controller

			Pages.index({
				fade: false,
				fnLoad: function(el){
					setTimeout(function(){
						Helpers.scrollToHash(); // scrolling to hash if exists, or to top
					}, 100);
				}
			});
		},
		
		// pages route action (dynamic open of static pages)
		pages: function (data) {
			Pages.load(data);
		},

		// rest route actions
		dispatch: function (data) {
			
			if (this.checkHash(data))
				return; // if true you go to the hash, so doesn't change the controller
			
			var me = this;
			
			// dispatch static
			var resPage = data.controller+(data.action ? '/'+data.action : '');
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
					
					setTimeout(function(){
						Helpers.scrollToHash();
					}, 100);
					
				}
				else Pages.errorView({}, "Controller not found: "+ControllerName);
				$('.dropdown-toggle').dropdown();
			}, function(err){
				console.error(err.stack);
				window.err = err;
				Pages.errorView({}, "Controller not found: "+ControllerName);
			});
		},
		
		dispatchStatic: function(resPage){
			if (typeof(Config.staticPages[resPage])=='string')
				Config.staticPages[resPage] = { url: Config.staticPages[resPage] };
			
			var viewOpt = $.extend({}, Config.staticPages[resPage], {
				fnLoad: function(el){
					setTimeout(function(){
						Helpers.scrollToHash(); // scrolling to hash if exists, or to top
					}, 100);
					if (Config.staticPages[resPage].fnLoad)
						Config.staticPages[resPage].fnLoad(el);
				},
				selector: Config.staticPages[resPage].selector || Config.mainContainer
			});
			
			Pages.view(viewOpt);
			
			return false;
		},
		
		checkHash: function(data){
			// check if only the hash is changed
			if (this.previousData 
					&& this.previousData.controller==data.controller 
					&& this.previousData.action==data.action
					&& this.previousData.hash!=data.hash){
				//console.log('ONLY SCROLL');
				
				Helpers.scrollToHash(data.hash, true);
				this.previousData = data;
				return true;
			}
			else if (data.hash)
				;//console.log('CHANGE PAGE AND SCROLL');
			else
				;//console.log('ONLY CHANGE PAGE');
			this.previousData = data;
			
			return false;
		}
	});
	
	return {
		init: function() {
			// route on document.ready
			$(function() {
				// deactivate routing until it's not instantiated
				//can.route.ready(false);

				// init
				new Router(document);

				// events
				can.route.bind('change', function(ev, attr, how, newVal, oldVal) {
					// menu change
					if (how === 'set') Pages.initMenu();
					else if (attr=='route' && how=='add' && newVal==""){
						console.log("INDEX");
						Pages.initMenu();
					}
				});

				// activate routing
				can.route.ready();
			});
		}
	};
});

