
//main require
require.config({
	baseUrl: '/scripts', 
	paths: {
		jquery: 'libs/jquery/jquery-1.10.2',
		underscore: 'libs/underscore/underscore-min',
//		bootstrap: 'libs/bootstrap/js/bootstrap.min',
		bootstrap: 'libs/bootstrap-3.2.0-dist/js/bootstrap',
		bootbox: 'libs/bootbox/bootbox.min',
		loadmask: 'libs/jquery.loadmask/jquery.loadmask.min',
		can: 'libs/canjs/can.jquery',
		supercan: 'libs/canjs/can.construct.super',
		canroutepushstate: 'libs/canjs/can.route.pushstate',
		canpromise: 'libs/canjs/can.list.promise',
		underscorestring: 'libs/underscore/underscore.string.min',
		skrollr: 'libs/skrollr/skrollr.min',
		moment: 'libs/moment/moment.min',
		messenger: 'libs/messenger/js/messenger.min',
		messengerThemeFlat: 'libs/messenger/js/messenger-theme-flat',
		ajaxFileUpload: 'libs/ajaxFileUpload/ajaxfileupload',
		jqueryValidate: 'libs/jquery.validate/js/jquery.validate',//.additional-methods.js
		//bootstrapFileUpload: 'libs/bootstrap-fileupload/bootstrap-fileupload.min',
		jasnyBootstrap: 'libs/jasny-bootstrap/js/jasny-bootstrap.min',
		droppableTextarea: 'libs/jquery.droppableTextarea/js/jquery.droppableTextarea',
		zeroClipboard: 'libs/zeroClipboard/ZeroClipboard',
		jqueryCopyableInput: 'libs/jquery.copyableInput/js/jquery.copyableInput',
		summernote: 'http://cdnjs.cloudflare.com/ajax/libs/summernote/0.8.1/summernote',
		latinise: 'libs/latinise/latinise.min',
		datePicker: 'libs/datePicker/js/bootstrap-datepicker'
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
		canroutepushstate: ['can'],
		canpromise: ['can'],
		underscorestring: ['underscore'],
		messenger: ['jquery'],
		ajaxFileUpload: ['jquery'],
		jqueryValidate: ['jquery'], 
		jasnyBootstrap: ['jquery', 'bootstrap'],
		droppableTextarea: ['jquery', 'bootstrap'],
		zeroClipboard: {
			exports: 'ZeroClipboard'
		},
		jqueryCopyableInput: ['zeroClipboard', 'jquery'],
		summernote: ['jquery', 'bootstrap']
	}
});

//init
require([
	'app',
	'router',
	'config',
	'utils/helpers',
], function(App, Router, Config, Helpers) {
	window.Helpers = Helpers;
	
	window.App = App;
	if (!document.location.pathname.startsWith('/portal'))
		document.location = '/portal' + document.location.pathname;
	else{
		App.init(Config); // app init
		Router.init(); // router init
	}

});
