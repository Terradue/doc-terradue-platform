
define([
	'require',
	'jquery',
	'can',
	'underscore',
	'config',
	'utils/helpers',
	'bootstrap',
	'underscorestring',
], function(require, $, can, _, Config, Helpers) {
	
	//private properties
	var baseScriptsUrl = '~/'; //TODO: MANUALLY SPECIFY SCRIPTS ROOT FOLDER
	var baseUrl = require.toUrl('./').toLowerCase()
		.replace(baseScriptsUrl.substring(1) + '/./', '/');
	
	return {

		// controller cache
		controllers: {},

		// init
		init: function(options){
			
			Helpers.logoInConsole();
			
			options = options || {};
			
			// set default template engine for can.view
			can.view.types[''] = can.view.types['.' + options.template || Config.template];
			
			// set the identifier name
			can.Model.id = 'Id';
			
			// configure ajax requests
			$.ajaxSetup({
			    contentType : 'application/json',
			    processData : false,
			});
			$.ajaxPrefilter( function( options, originalOptions, jqXHR ) {
			    if (options.data)
			        options.data=JSON.stringify(options.data);
			});

			// merge string plugin to underscore namespace
			_.mixin(_.str.exports());
			
			// set canjs view extension to nothing for extensionless view filenames
			can.view.ext = '';
			
			// choose default template engine
			can.view.types[''] = can.view.types['.' + options.template || 'mustache'];
			
			//this.initErrorHandler(options);
		},
		
		loadController: function(controllerName, successCallback, errorCallback) {
			var self = this,
				_errorCallback = (errorCallback ? errorCallback : function(){
					self.error("Error to load script " + url);
				}),
				controllerNameLow = controllerName.toLowerCase();

			if (self.controllers[controllerName])
				// use cached controller
				successCallback(self.controllers[controllerName]);
			else
				// get controller via ajax
				require(['modules/'+controllerNameLow+'/controllers/'+controllerNameLow], function(controller){
					self.controllers[controllerName] = controller;
					successCallback(controller);
				}, _errorCallback);
		},
		
		loadView: function(options, fnLoad, fnError) {
			if (options && fnLoad) {
				var resolvedUrl = this.toViewsUrl(options.url);

				// load and merge data to view (if applicable)
				if (options.url && options.data) {
					if (Helpers.isDeferred(options.data))
						can.view(options.url, options.data).then(fnLoad);
					else fnLoad(can.view(options.url, options.data));
				} else if (options.url)
					$.get(resolvedUrl, fnLoad).fail(fnError);
				else if (options.content) fnLoad(options.content);
			}
		},
		
		navigate: function(route, data) {
			var hash = '!' + _.ltrim(route, '#!/');
			if (data) hash += '&' + can.param(data);
			// change page (hash)
			window.location.hash = hash;
		},

		getCurrentPage: function() {
			var file = window.location.pathname;
			var n = file.lastIndexOf('/');
			return n >= 0 ? file.substring(n + 1).toLowerCase() : '';
		},
		getCurrentHashPage: function() {
			var path = _.ltrim(window.location.hash, '#!/');

			//VALIDATE INPUT
			if (path === '') return '';

			//ROOT PAGE IF APPLICABLE
			if (path.indexOf('/') < 0) return path;

			//DETERMINE ROOT PAGE
			return path.substring(0, path.indexOf('/'));
		},

		
		// utils
		toUrl: function(url) {
			if (url && url.indexOf('~/') === 0)
				url = baseUrl + url.substring(2);
			return url;
		},
		toScriptsUrl: function(url) {
			return this.toUrl(baseScriptsUrl + '/' + url);
		},	
		toViewsUrl: function(url) {
			if (url[0]=='/')
				return url;
			else
				return this.toScriptsUrl(url);
		},
		
		error: function(msg){
			// TODO: improve
			alert(msg);			
		},
		
		updateDynamicElements: function(){
			var self = this;
			
			$('.dropdown-toggle').dropdown();
			
			$('a.innerLink').click(function(){
				var data = can.route.attr(),
					href = $(this).attr('href'),
					toLink = Helpers.getUrlParameters(href).to;
				
				//if (toLink && toLink==data.to){
					console.log('---REDIRECT FROM CLICK');
					Helpers.scrollToInnerLink(toLink);
					//return false;
				//}
			});
			
			App.tryInnerLinkRedirect();
		},

		
		tryInnerLinkRedirect: function(){
			if (can.route.attr().to)
				Helpers.scrollToInnerLink(can.route.attr().to);
		},

	}
	
});