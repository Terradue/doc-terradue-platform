
//main require
require.config({
	baseUrl: '/scripts', 
	paths: {
		jquery: 'libs/jquery/jquery-1.10.2',
		underscore: 'libs/underscore/underscore-min',
		bootstrap: 'libs/bootstrap/js/bootstrap.min',
		bootbox: 'libs/bootbox/bootbox.min',
		loadmask: 'libs/jquery.loadmask/jquery.loadmask.min',
		can: 'libs/canjs/can.jquery',
		supercan: 'libs/canjs/can.construct.super',
		underscorestring: 'libs/underscore/underscore.string.min',
		skrollr: 'libs/skrollr/skrollr.min',
		moment: 'libs/moment/moment.min',
	},
	shim: {
		underscore: {
			exports: '_'
		},
		skrollr: ['jquery'],
		bootstrap: ['jquery'],
		bootbox: ['jquery', 'bootstrap'],
		loadmask: ['jquery'],
		can: {
			deps: ['jquery'],
			exports: 'can',
		},
		supercan: ['can'],
		underscorestring: ['underscore'],
	}
});

//init
require([
	'app',
	'router',
	'config',
], function(App, Router, Config) {
	window.App = App;
	App.init(Config); // app init
	Router.init(); // router init
});
