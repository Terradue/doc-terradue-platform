
define([
	'jquery',
	'can',
	'underscore',
	'app',
	'utils/helpers',
	'config',
	'underscorestring'
], function($, can, _, App, Helpers, Config){
	// merge string plugin to underscore namespace
	_.mixin(_.str.exports());
	
	return can.Control({
		
		init: function(element, options) {
			//DERIVED SHOULD CALL BASE LIKE:
			//this._super(element, options);
		},
		
		view: function(options) {
			var self = this;

			var callback = function(frag) {
				//GET SPECIFIED CONTENT AREA
				var el = options.selector
					? self.element.find(options.selector)
					: self.element;

				// change content
				el.hide().html(frag).fadeIn();

				// process callback function if applicable

				if (options.fnLoad) options.fnLoad(el);

				// rebind events after all views added
				self.on();
				
			};

			// load and merge data if applicable
			App.loadView(options, function(frag) {
				//handle view dependency if specified and not exists already
				if (options.dependency && self.element.find(options.selector).length === 0) {
					App.loadView(options.dependency, function(dependencyFrag) {
						//GET SPECIFIED CONTENT AREA
						var dependencyEl = options.dependency.selector
						? self.element.find(options.dependency.selector)
								: self.element;

						//RENDER IN DEPENDENCY AREA
						dependencyEl.html(dependencyFrag);

						//PROCESS DEPENDENCY CALLBACK IF APPLICABLE
						if (options.dependency.fnLoad)
							options.dependency.fnLoad(dependencyEl);

						//PROCESS CALLBACK
						callback(frag);
					});
				} else callback(frag);
			}, function(){
				self.errorView(options, null, "Unable to load " + options.url + " view.");
				Helpers.scrollToTop();
			});
		},
		
		errorView: function(options, shortMsg, longMsg, isMainContainer){
			options.url = Config.pageError;
			if (!isMainContainer)
				options.selector = Config.mainContainer;
			options.data = {
				shortMsg: shortMsg,
				longMsg: longMsg,
			};
			this.view(options);
		},

		accessDenied: function(){
			this.view({
				url: 'modules/pages/views/accessDenied.html',
			});
		},

		modal: function(options) {
			var self = this;

			//LOAD MODAL WRAPPER DEPENDENCY
			App.loadView({
				url: 'modules/pages/views/modal.html'
			}, function(modalFrag) {
				//PLACE MODAL DOM AND GET REFERENCE
				var el = $(document.body).append(modalFrag).find('> .modal').last();

				App.loadView(options, function(frag) {
					//UPDATE MODAL CONTENT
					el.find('> .modal-body').html(frag);
					el.find('> .modal-header > h3').html(options.title);

					//HANDLE MODAL FOOTER IF APPLICABLE
					var footerEl = el.find('> .modal-footer'),
						$submit = footerEl.find('> .submit-modal');
					if (options.footer !== false) {
						if (options.submit) $submit.html(options.submit);
						if (options.submitCss) $submit.addClass(options.submitCss);
						if (options.close) footerEl.find('> .close-modal').html(options.close);
					} else {
						footerEl.hide();
					}

					//SET STYLE OF MODAL IF APPLICABLE
					var css = {};
					if (options.width) {
						//SET WIDTH AND MAKE CENTER
						css.width = typeof css.width === 'number'
							? options.width + 'px' : options.width;

						//TODO: FIND BETTER WAY TO CENTER MODAL
						css['margin-left'] = function() {
							return -($(this).width() / 2);
						};
					}

					//OPEN MODAL WINDOW
					$(el).on('shown', function() {
						//PROCESS LOAD CALLBACK FUNCTION IF APPLICABLE
						if (options.fnLoad) options.fnLoad(el);

						//REBIND EVENTS AFTER ALL VIEWS ADDED
						self.on();
					}).on('hide', function() {
						//PROCESS HIDE CALLBACK FUNCTION IF APPLICABLE
						if (options.fnHide) options.fnHide(el);
					}).on('hidden', function() {
						//REMOVE CONTENT AND BINDINGS
						$(this).remove();
					}).modal('show').css(css);
					
					// submit event
					if (options.fnSubmit)
						$submit.click(function(e){
							options.fnSubmit(el, e);
						})
				});
			});
		},

		hideModal: function() {
			//HIDE ANY OPEN MODAL WINDOWS
			$('.modal.in', this.element).modal('hide');
		},
		
		loadCSS: function(href) {
			var cssLink = $("<link>");
			$("head").append(cssLink); //IE hack: append before setting href
			cssLink.attr({
				rel:  "stylesheet",
				type: "text/css",
				href: '/scripts/'+href
			});
		}
		
	});
});

