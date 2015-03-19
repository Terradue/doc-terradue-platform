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
		init: function () {},

		// ROUTES
		'route': 'index',
		
		'pages/:id&:selector route': 'pages',
		'pages/:id route': 'pages',
		
		':controller/:action/:id route': 'dispatch',
		':controller/:action route': 'dispatch',
		':controller route': 'dispatch',

		// ACTIONS
		
		// index
		index: function () {
			Pages.index({ fade: false });
		},
		
		// pages route action (dynamic open of static pages)
		pages: function (data) {
			Pages.load(data);
		},

		// rest route actions
		dispatch: function (data) {
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
						Helpers.scrollToTop();
					}, 100);
				}
				else Pages.errorView({}, "Controller not found: "+ControllerName);
				$('.dropdown-toggle').dropdown();
			}, function(err){
				console.error(err);
				Pages.errorView({}, "Controller not found: "+ControllerName);
			});
		},
		
		dispatchStatic: function(resPage){
			if (typeof(Config.staticPages[resPage])=='string')
				Config.staticPages[resPage] = { url: Config.staticPages[resPage] };
			if (Config.staticPages[resPage].selector==null)
				Config.staticPages[resPage].selector = Config.mainContainer;
			Pages.view(Config.staticPages[resPage]);
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


/*
//MERGE STRING PLUGIN TO UNDERSCORE NAMESPACE
_.mixin(_.str.exports());

var Router = can.Control ({

	init: function () {},

	//ROUTES
	'route': 'index',
	'contact route': 'contact',
	
	'pages/:id&:selector route': 'pages',
	'pages/:id route': 'pages',
	
	':controller/:action/:id route': 'dispatch',
	':controller/:action route': 'dispatch',
	':controller route': 'dispatch',

	//ACTIONS
	index: function () {
		Pages.index({ fade: false });
	},

	contact: function () {
		Pages.contact();
	},

	pages: function (data) {
		Pages.load(data);
	},

	//	ROUTES TO CONTROLER / ACTION
	dispatch: function (data) {
		var me = this;

		//SCRUB URL PARAMS IF APPLICABLE
		var ControllerName = _.capitalize (data.controller);
		//CONVERT URL PARAM TO ACTION NAMING CONVENTION
		var actionName = data.action
		? data.action.charAt(0).toLowerCase() + _.camelize(data.action.slice(1))
				: 'index'; //DEFAULT TO INDEX ACTION

		//DYNAMICALLY REQUEST CONTROLLER AND PERFORM ACTION
		App.loadController (ControllerName, function (controller) {
			//CALL ACTION WITH PARAMETERS IF APPLICABLE
			if (controller && controller[actionName])
				controller[actionName](data);
//			TODO: FIX BUG, ONLY WORKS ON FIRST HIT
//			DUE TO HOW REUIREJS ERROR EVENT WORKS
			else App.navigate('pages/404');
		}, function(){
			console.log("noooooou");
			App.navigate('pages/404')
		});
	}
});

//ROUTE ON DOCUMENT READY
$(function () {
	//PAUSE ROUTING UNTIL INSTANTIATED
	//OTHERWISE ROUTER MUST BE INSTANTIATED BEFORE DOCUMENT READY
	// https://forum.javascriptmvc.com/ # Topic/32525000001070159
	//can.route.ready(false);

	//INITIALIZE ROUTER FOR LISTENING
	new Router(document);

	//EVENTS
	can.route.bind('change', function(ev, attr, how, newVal, oldVal) {
		//HANDLE ACTIVE MENU SELECTION BASED ON ROUTE SET ONLY
		if (how === 'set') Pages.initMenu();
	});

	//ACTIVATE ROUTING
	can.route.ready();
});
*/