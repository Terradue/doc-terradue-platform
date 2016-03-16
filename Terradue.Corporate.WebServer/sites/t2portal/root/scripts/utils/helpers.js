/**
 * This is a helper utils module
 */
define([
	'jquery',
	'can',
	'underscore',
	'underscorestring',
	'moment',
], function($, can, _) {
	//MERGE STRING PLUGIN TO UNDERSCORE NAMESPACE
	_.mixin(_.str.exports());
	
	var helpers = this;	
	
	// OK and KO test functions
	window.OK = function(){console.log("OK!");};
	window.KO = function(){console.log("KO!");};
	window.req = function(lib){ require([lib.toLowerCase()], function(ris){window[lib] = ris}) };

	/* JS HELPERS */
	if (!String.prototype.startsWith)
		String.prototype.startsWith = function(needle) {
			return(this.indexOf(needle) == 0);
		};

	if (!String.prototype.endsWith)
		String.prototype.endsWith = function(suffix) {
			return this.indexOf(suffix, this.length - suffix.length) !== -1;
		};

	if (!String.prototype.format)
		String.prototype.format = function() {
			var args = arguments;
			return this.replace(/{(\d+)}/g, function(match, number) { 
				return typeof args[number] != 'undefined' ? args[number] : match;
			});
		};
		
	if (!Array.prototype.unique)
		Array.prototype.unique = function(getId) {
		    var a = this;
		    if (!getId)
		    	getId = function(obj){ return obj; }; // identity function by default
		    for(var i=0; i<a.length; ++i) {
		        for(var j=i+1; j<a.length; ++j) {
		            if(getId(a[i]) === getId(a[j]))
		                a.splice(j--, 1);
		        }
		    }
	
		    return a;
		};
	String.prototype.hashCode = function() {
		var hash = 0, i, chr, len;
		if (this.length == 0) return hash;
		for (i = 0, len = this.length; i < len; i++) {
			chr   = this.charCodeAt(i);
			hash  = ((hash << 5) - hash) + chr;
			hash |= 0; // Convert to 32bit integer
		}
		return hash;
	};
	
	/* MUSTACHE HELPERS */
	
	can.mustache.registerHelper('ifEquals', function(value, valueCheck, options) {
		var v = (value==null || typeof(value)!='function' ? value : value());
		var vc = (valueCheck==null || typeof(valueCheck)!='function' ? valueCheck : valueCheck());
		return (v===vc ? options.fn() : null);
	});
	
	can.mustache.registerHelper('ifNotEquals', function(value, valueCheck, options) {
		var v = (value==null || typeof(value)!='function' ? value : value());
		return (v!==valueCheck ? options.fn() : null);
	});
	
	can.mustache.registerHelper('fromNow', function(dateStr, options) {
		var dateStr = (dateStr==null || typeof(dateStr)!='function' ? dateStr : dateStr());
		return moment(dateStr).fromNow();
	});
	
	can.mustache.registerHelper('simpleDate', function(dateStr, options) {
		var dateStr = (dateStr==null || typeof(dateStr)!='function' ? dateStr : dateStr());
		return moment(dateStr).format("MMM Do YYYY");
	});
	
	can.mustache.registerHelper('formattedDate', function(dateStr, format, options) {
		var dateStr = (dateStr==null || typeof(dateStr)!='function' ? dateStr : dateStr());
		var format = (format==null || typeof(format)!='function' ? format : format());
		return moment(dateStr).format(format);
	});
	
	can.mustache.registerHelper('spacer', function(width) {
		return '<div style="display:inline-block; width:' + width + 'px"></div>';
	});

	can.mustache.registerHelper('count', function(value) {
		var v = (value==null || typeof(value)!='function' ? value : value());
		return (v.length==null ? 1 : v.length);
	});
	
	can.mustache.registerHelper('truncate', function(text, limit, options) {
		var text = (text==null || typeof(text)!='function' ? text : text()),
			limit = (limit==null || typeof(limit)!='function' ? limit : limit());

		if (text==null)
			return "";
		else if (text.length<limit)
			return text;
			
		var words = text.split(" "), trunk="";
		$.each(words, function(i){
			var word = ""+this;
			if ((trunk.length + word.length) < limit)
				trunk += (i==0 ? "" : " ") + word;
			else
				return false;
		});
		return trunk + "...";
	});

	// register new mustache "each" helper to have #first and #last
	can.mustache.registerHelper('eachNew', function(expr, options){
		
		var isObserveLike = function (obj) {
			return obj instanceof can.Map || (obj && !! obj._get);
		};
		
        // Check if this is a list or a compute that resolves to a list, and setup
        // the incremental live-binding 

        // First, see what we are dealing with.  It's ok to read the compute
        // because can.view.text is only temporarily binding to what is going on here.
        // Calling can.view.lists prevents anything from listening on that compute.
        var resolved = Mustache.resolve(expr),
            result = [],
            keys,
            key,
            i;

        // When resolved === undefined, the property hasn't been defined yet
        // Assume it is intended to be a list
        if (can.view.lists && (resolved instanceof can.List || (expr && expr.isComputed && resolved === undefined))) {
            return can.view.lists(expr, function(item, index) {
                return options.fn(options.scope.add({
                            "@index": index,
                            "@first": (index()==0),
                            "@last": (index()==expr.length-1),
                        })
                    .add(item));
            });
        }
        expr = resolved;

        if ( !! expr && $.isArray(expr)) {
            for (i = 0; i < expr.length; i++) {
                result.push(options.fn(options.scope.add({
                                "@index": i
                            })
                        .add(expr[i])));
            }
            return result.join('');
        } else if (isObserveLike(expr)) {
            keys = can.Map.keys(expr);
            // listen to keys changing so we can livebind lists of attributes.

            for (i = 0; i < keys.length; i++) {
                key = keys[i];
                result.push(options.fn(options.scope.add({
                                "@key": key
                            })
                        .add(expr[key])));
            }
            return result.join('');
        } else if (expr instanceof Object) {
            for (key in expr) {
                result.push(options.fn(options.scope.add({
                                "@key": key
                            })
                        .add(expr[key])));
            }
            return result.join('');

        }
    });

	can.mustache.registerHelper('paginationBox', function(totalResults, pageOffset, page, count) {
		
		var n = totalResults = totalResults();
		var pageOffset = pageOffset();
		var page = page();
		var count = count();
		var np = totalPages = Math.ceil(totalResults/count);
		var ePage = page + 1 -pageOffset; // effective page shown to user, from 1 to np
		
		if (totalPages<=1)
			return '';
		
		var hasFirst, hasPrevDots, has2Prev, hasPrev, hasNext, has2Next, hasNextDots, hasLast;
		
		if (ePage>1) 	hasPrev = true;
		if (ePage>2)	has2Prev = true;
		if (ePage>3)	hasFirst = true;
		if (ePage>4)	hasPrevDots = true;
		
		if (ePage<np)   hasNext = true;
		if (ePage<np-1) has2Next = true;
		if (ePage<np-2) hasLast = true;
		if (ePage<np-3) hasNextDots = true;
		
		var html = //'totalResults='+totalResults+', pageOffset='+pageOffset+', page='+page+', count='+count+
			'<ul class="pagination paginationBox">';
		if (hasPrev)
			html+= '<li><a data-page="'+(page-1)+'" class="changePage" href="#">Prev</a></li>';
		if (hasFirst)
			html+= '<li><a data-page="'+pageOffset+'" class="changePage" href="#">1</a></li>';
		if (hasPrevDots)
			html+= '<li><a href="#" class="disabled">...</a></li>';
		if (has2Prev)
			html+= '<li><a data-page="'+(page-2)+'" class="changePage" href="#">' + (ePage-2) + '</a></li>';
		if (hasPrev)
			html+= '<li><a data-page="'+(page-1)+'" class="changePage" href="#">' + (ePage-1) + '</a></li>';
		html+= '<li class="active"><a href="#">'+(ePage)+'</a></li>';
		if (hasNext)
			html+= '<li><a data-page="'+(page+1)+'" class="changePage" href="#">' + (ePage+1) + '</a></li>';
		if (has2Next)
			html+= '<li><a data-page="'+(page+2)+'" class="changePage" href="#">' + (ePage+2) + '</a></li>';
		if (hasNextDots)
			html+= '<li><a href="#" class="disabled">...</a></li>';
		if (hasLast)
			html+= '<li><a data-page="'+(np+pageOffset-1)+'" class="changePage" href="#">' + (np) + '</a></li>';
		if (hasNext)
			html+= '<li><a data-page="'+(page+1)+'" class="changePage" href="#">Next</a></li>';
		html+= '</ul>';
		
		return html;
	});	
	
	////////////////////////////////

	return {
		
		// get random integer between min and max,
		// or a random element from an array
		random: function(min, max){
			
			// if min is an array get random element 
			if ($.isArray(min))
				return min[this.random(min.length-1)];
			
			if (max==null){	max = min; min = 0; };
			return Math.floor(Math.random() * (max - min + 1)) + min;
		},
		
		randomId: function(n){
			var id = '';
			for (i=0; i<n; i++)
				id += String.fromCharCode(65 + Math.floor(Math.random() * 26));
			return id;
		},
		
		arrayToObject: function(ar, keyName){
			var obj = {};
			if ($.isArray(ar))
				$.each(ar, function(){
					if (this[keyName])
						obj[this[keyName]] = this;
				});
			return obj;
		},
		
		// get the link parameter with @rel==<rel> from a links list
		getLinkFromLinks: function(links, rel, title){
			try {
				return links.filter(function(l){return (l["@rel"]==rel && (title ? l['@title']==title : true))})[0];
			} catch(e){
				return null;
			}
		},
		
		forceToArray: function(a){
			if ($.isArray(a) || (a.attr && $.isArray(a.attr())))
				return a;
			else
				return [a];
		},
		
		// get the href parameter with @rel==<rel> from a links list
		getHrefFromLinks: function(links, rel, title){
			var link = this.getLinkFromLinks(links, rel, title);
			return (link && link["@href"]) ? link["@href"] : "";
		},

		getCategoryFromCategories: function(categories, term){
			try {
				return categories.filter(function(l){return (l["@term"]==term)})[0];
			} catch(e){
				return null;
			}
		},

		getLabelFromCategories: function(categories, term){
			var category = this.getCategoryFromCategories(categories, term);
			return (category && category["@term"]==term) ? category["@label"] : "";
		},
		
		getSelfFromFeature: function(feature){
			return this.getHrefFromLinks(feature.properties.links, 'self');
		},
		
		truncateText: function(text, limit){
			if (text==null)
				return "";
			else if (text.length<limit)
				return text;
			
			var words = text.split(" "), trunk="";
			$.each(words, function(i){
				var word = ""+this;
				if ((trunk.length + word.length) < limit)
					trunk += (i==0 ? "" : " ") + word;
				else
					return false;
			});
			return trunk + "...";
		},

		
		keyValueArrayToJson: function(arr, keyName, valueName){
			if (!keyName)
				keyName = 'Key';
			if (!valueName)
				valueName = 'Value';
			
			json ={};
			$.each(arr, function(){
			  if (this[keyName] && this[valueName])
			    json[this[keyName]] = this[valueName];
			});
			return json;
		},
		
		retrieveDataFromForm: function(formSelector, inputNames){
			var $form = $(formSelector);
			if ($form.length==0) return null;
			
			var data = {}, _inputNames = ($.isArray(inputNames) ? inputNames : [inputNames]);
			$.each(_inputNames, function(){
				var $input = $form.find('input[name="'+ this +'"]');
				if ($input.length!=0){
					var value = ($input.attr('type')=='checkbox') ?
							$input.is(':checked') : $input.val();
					if ($.isArray(inputNames))
						data[this] = value;
					else
						data = value;
				}
			});
			return (data);
		},
		
		getUrlParameters: function(url) {
			var parametersUrl = url ? url.split('?')[1] : window.location.search.substring(1),
				params={
					'__baseUrl' : url ? url.split('?')[0] : window.location.origin + window.location.pathname,
					'__length' : 0,
				};
			
			if (parametersUrl){
				var urlParams = parametersUrl.split('&');
				
				for (var i=0; i<urlParams.length; i++) {
					var param = urlParams[i].split('=');
					if (param.length==2)
						params[param[0]] = param[1];
				}
				params['__length'] = urlParams.length;
			}
			return params;
		},
		
		addFormatJsonToUrl: function(url){
			var params = this.getUrlParameters(url)
			
			if (params.__length==0)
				url += (url[url.length-1]=='?' ? 'format=json' : '?format=json'); 
			else
				if (!params.format)
					url += '&format=json';
			
			return url;
		},
		
		// map x into a range to y in another range
		range: function(a1,b1,a2,b2,x,limit){
		    
		    var min1 = (a1<b1 ? a1 : b1),
		        max1 = (a1>b1 ? a1 : b1),
		        min2 = (a2<b2 ? a2 : b2),
		        max2 = (a2>b2 ? a2 : b2),
		        range1 = max1-min1,
		        range2 = max2-min2;
		    
		    // if limit, limit the x into the range
		    if (limit)
		        if (x<min1)
		            x=min1;
		        else if (x>max1)
		            x=max1;
		    
		    // normalization
		    var vnorm = (x-min1)/range1;
		    
		    // inversion if ranges are inverted between them
		    if ((b1-a1)*(b2-a2)<0)
		        vnorm = 1-vnorm;

		    // proportion
		    return vnorm * range2 + min2;
		},
		
		// scroll to top
		scrollToTop: function(){
			$('html, body').scrollTop(0);
		},
		
		scrollTop: function() {
			$('html, body').animate({
				scrollTop: 0
			}, 'slow');
		},
		
		scrollToHash: function(hash, animate){
			// get the hash
			if (hash){
				if (hash[0]!='#')
					hash = '#' + hash;
			} else
				hash = document.location.hash;
			
			var scrollPosition = 0; // go on the top by default
			if (hash && $(hash).length)
				scrollPosition = $(hash).offset().top - 50;
			
			// scroll
			if (animate)
				$('html,body').animate({
					scrollTop: scrollPosition
				});
			else
				$('html,body').scrollTop(scrollPosition);
		},

		openWindow: function(url, title, width, height) {
			var options = 'width=' + (width || 500) + ',height=' + (height || 500);
			return window.open(url, title, options);
		},

		bookmarkPage: function(title, url) {
			//DETERMINE TITLE AND URL
			title = title || document.title;
			url = url || window.location;

			//BOOKMARK PAGE BASED ON BROWSER
			if ($.browser.mozilla) window.sidebar.addPanel(title, url, "");
			else if ($.browser.msie) window.external.AddFavorite(url, title);
			else if (window.opera && window.print) {
				var elem = document.createElement('a');
				elem.setAttribute('href', url);
				elem.setAttribute('title', title);
				elem.setAttribute('rel', 'sidebar');
				elem.click();
			} else {
				alert('Unfortunately, this browser does not support the requested action,' + ' please bookmark this page manually.');
			}
		},

		sendClientMail: function(options) {
			//CONSTRUCT EMAIL PARAMETERS
			var url = 'mailto:' + encodeURIComponent(options.mailto) + '?';
			if (options.cc) url += 'cc=' + encodeURIComponent(options.cc) + '&';
			if (options.subject) url += 'subject=' + encodeURIComponent(options.subject) + '&';
			if (options.body) url += 'body=' + encodeURIComponent(options.body) + '&';

			//TRIM TRAILING QUERYSTRING DELIMITERS
			_.rtrim(url, '?&');

			//TRIGGER BROWSER EMAIL REQUEST (TIMEOUT BECAUSE OF "REDIRECT")
			setTimeout(function() {
				window.location.href = url;
			}, 1000);
		},

		convertToBoolean: function(value) {
			//VALIDATE INPUT
			if (!this.isNullOrEmpty(value)) return false;

			//DETERMINE BOOLEAN VALUE FROM STRING
			if (typeof value === 'string') {
				switch (value.toLowerCase()) {
				case 'true':
				case 'yes':
				case '1':
					return true;
				case 'false':
				case 'no':
				case '0':
					return false;
				}
			}

			//RETURN DEFAULT HANDLER
			return Boolean(value);
		},

		parseJson: function(json) {
			//USES BROWSER JSON IF AVAILABLE FOR PERFORMANCE
			return JSON && JSON.parse(json) || $.parseJSON(json);
		},

		parseRss: function(url, options, fnLoad) {
			//VALIDATE INPUT
			options = options || {};

			//OVERLOAD FUNCTION (OPTIONS PARAM NOT REQUIRED)
			if (typeof options === 'function' && !fnLoad) {
				fnLoad = options;
				options = {};
			}

			//GET RSS ITEMS
			$.get(url, function(data) {
				//PARSE RSS
				var items = [];
				$(data).find('item').each(function(index) {
					var this$ = $(this);
					items.push({
						title: options.maxTitleChars
							? _.truncate(this$.find('title').text(), options.maxTitleChars)
							: this$.find('title').text(),
						description: options.maxDescriptionChars
							? _.truncate(_.stripTags(this$.find('description').text()), options.maxDescriptionChars)
							: _.stripTags(this$.find('description').text()),
						link: this$.find('link').text(),
						pubDate: moment(this$.find('pubDate').text(), options.parseFormat
							|| 'ddd, DD, MMM YYYY hh:mm:ss Z')
							.format(options.dateFormat || 'dddd, MMMM DD, YYYY'),
						author: this$.find('author').text()
					});
					//STOP AT COUNTER IF APPLICABLE
					if (options.maxItems && options.maxItems <= index + 1) return false;
				});
				//RETURN RSS ITEMS TO CALLBACK
				fnLoad(items);
			});
		},
		
		isInDom: function($el){
			return $.contains(document, $el[0]);
		},

		isDeferred: function(data) {
			//DETERMINE IF ALL DATA IS DEFERRED IF APPLICABLE
			var isDeferred = false;
			if (data) {
				isDeferred = true;
				//HANDLE MULTIPLE INPUTS
				var arr = _.toArray(data);
				for (var i = 0; i < arr.length; i++) {
					//DUCK-TYPING TO CHECK IF DEFERRED OBJECT
					if (!arr[i] || !arr[i].promise) {
						isDeferred = false;
						break;
					}
				}
			}
			return isDeferred;
		},

		isNullOrEmpty: function(value) {
			return typeof value === 'undefined' || value === null || value.length === 0;
		},

		getValueOrDefault: function(value, defaultValue) {
			return !this.isNullOrEmpty(value) ? value : defaultValue;
		},
		
		getErrMsg: function(xhr, message){
			try {
				return xhr.responseJSON.ResponseStatus.Message;
			} catch(e){
				if (xhr.responseText)
					return xhr.responseText;
				else if (message)
					return message;
				else 
					return xhr.statusText;
			}
		},
		
		gotoInnerLink: function(aName){
			$el = $("a[name='"+aName+"'");
			if ($el.size)
				$('html,body').animate({
					scrollTop: $el.offset().top
				},'slow');
			return false;
		},
		
		findById: function(list, Id){
			var ris = null;
			list.each(function(){
				if (this.Id==Id){
					ris = this; return false;
				}
			});
			return ris;
		},
		
		filterKeys: function(object, keys){
			if (typeof(object)!='object')
				return object;
			
			var objectNew = {};
			for (key in object){
				if ($.inArray(key, keys) != -1)
					objectNew[key] = object[key];
			}
			return objectNew;
		},
		
		initCollapsiblePanel: function($panel){
			if ($panel.hasClass('panel-collapsible-init'))
				return;
			$panel.addClass('panel-collapsible-init');
			
			var $header = $panel.find('.panel-header'),
				$body = $panel.find('.panel-body'),
				isOpen = !$panel.hasClass('panel-closed'),
				$i = $('<i class="fa fa-chevron-circle-'+ (isOpen?'down':'right') + '"></i>').appendTo($header);
			
			$body.css({
				'background-color': $body.css('background-color'),
				'padding': $body.css('padding'),
				'border-radius': $body.css('border-radius'),
			});
			
			var show = function(){
				$panel.removeClass('panel-closed');
				$body.show('blind', function(){					
					$i.removeClass('fa-chevron-circle-right').addClass('fa-chevron-circle-down');
				});
				isOpen = true;
			};
			var hide = function(){
				$body.hide('blind', function(){
					$panel.addClass('panel-closed');
					$i.removeClass('fa-chevron-circle-down').addClass('fa-chevron-circle-right');
				});
				isOpen = false;
			}
			
			if (!isOpen)
				hide();
			
			$header.click(function(){
				if (isOpen)
					hide();
				else
					show();
			});
		},
		
		// NOTE: you must load validation lib first
		addPasswordValidationMethods: function(){
			// Validator extensions
			$.validator.addMethod(
				"atLeastOneUpper",
				function(value, element) {
					return this.optional(element) || new RegExp("[A-Z]").test(value);
				},
				"* atLeastOneUpper"
			);

			$.validator.addMethod(
				"atLeastOneLower",
				function(value, element) {
					return this.optional(element) || new RegExp("[a-z]").test(value);
				},
				"* atLeastOneUpper"
			);

			$.validator.addMethod(
				"atLeastOneNumber",
				function(value, element) {
					return this.optional(element) || new RegExp("[\\d]").test(value);
				},
				"* atLeastOneNumber"
			);

			$.validator.addMethod(
				"atLeastOneSpecialChar",
				function(value, element) {
					return this.optional(element) || new RegExp("[!#@$%^&*()_+]").test(value);
				},
				"atLeastOneSpecialChar"
			);

			$.validator.addMethod(
				"noOtherSpecialChars",
				function(value, element) {
					return this.optional(element) || new RegExp('^[a-zA-Z0-9!#@$%^&*()_+]+$').test(value);
				},
				"Please remove special characters"
			);			
		},
		
/*
 * 
<div class="panel panel-collapsible panel-closed">
  <div class="panel-header">This is the title</div>
  <div class="panel-body">This is the body <br>This is the body <br>This is the body <br>This is the body <br></div>
</div>

*/		
		logoInConsole: function(){
			var myVar = ""
				+ "                                                                                                         \n"
				+ "                                                                                                                                   \n"
				+ "                                                                                                                   #```+:          \n"
				+ "                                                                                                                   +,::;;+;        \n"
				+ "                                                                                                                   +:;'''++#       \n"
				+ "                                                                                       ;'',           .'''`        ';''+++###:     \n"
				+ "                                                                                   `+;'++####;     ';;''++++#.     '''+++#####:    \n"
				+ "                                                   ,;;`                            ;''##'+#####   #;'+#+;####+#`   '''++++####+`   \n"
				+ "  '++                                              ,;;   `                        '''#'    +###` :''+#     ####    ''''++++####+   \n"
				+ "  '++                                              ,;;                                    :+##+  ++++:     ####.   '''''+++#####   \n"
				+ "++++###   `#'###`     +'+   +'+   '++##:      .;;;;:;;  ;:;   ,;;`   `;;;;`             `#####`  +###:     +###.   ;''''+++#####   \n"
				+ ";;+##;;  ''+;;+###  +'+#; ;'+#; ;;##'++##;  ..;;::;;;;` :,;   ,;;` `.;;:;;;;.          #####;    +###:     +###.   '''''+++#####   \n"
				+ "  +##   +'########, ###   ###   ;++   `###  `:;    ;;:  ;,;   ,;;` `,;::::;;;        +##+#+      +###:     ####`   ''''++++#####   \n"
				+ "  ###   ;+#: `      ###   ###   '+#    #+#` .::    ;:,  ;,;   ,;:` ,:;```````      ''++##        :####     #+##    ''''++++####+   \n"
				+ "  ,####  :######+.  ###   ###    +#+#++###   :;;;;;:,`  `::;;;;,.   :;;;;;:,`     #'+##+ `####`   +#########+''    ''+++++#####    \n"
				+ "     ``     .:`     ..,   ,,,      ,:: :::     .,,.        ,::.       .,,.        :+##+' '#+##     ,######+''     ''++++######`    \n"
				+ "                                                                                           `   '       ...      .++++++######      \n"
				+ "                                                                                           ``:'+++`           :'+++++#######       \n"
				+ "                                                                                             ;;'+##++#+;;'++++++##+#####++         \n"
				+ "                                                                                               .+##################+###;           \n"
				+ "                                                                                                  `++###############,              \n"
				+ "                                                                                                         `,,,.                     \n"
				+ "\n";			console.log(myVar);
		}
	}
});
