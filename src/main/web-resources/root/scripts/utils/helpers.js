﻿/**
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

	
	can.mustache.registerHelper('ifEquals', function(value, valueCheck, options) {
		return (value()===valueCheck ? options.fn() : null);
	});
	
	can.mustache.registerHelper('fromNow', function(dateStr, options) {
		return moment(dateStr()).fromNow();
	});
	
	can.mustache.registerHelper('simpleDate', function(dateStr, options) {
		return moment(dateStr()).format("MMM Do YYYY");
	});
	
	
	// register new mustache "each" helper to have #first and #last
	can.mustache.registerHelper('eachNew', function(expr, options){
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

        if ( !! expr && isArrayLike(expr)) {
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

	

	return {
		
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
		
		scrollTop: function() {
			$('html, body').animate({
				scrollTop: 0
			}, 'slow');
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

		isDeferred: function(data) {
			//DETERMINE IF ALL DATA IS DEFERRED IF APPLICABLE
			var isDeferred = false;
			if (data) {
				isDeferred = true;
				//HANDLE MULTIPLE INPUTS
				var arr = _.toArray(data);
				for (var i = 0; i < arr.length; i++) {
					//DUCK-TYPING TO CHECK IF DEFERRED OBJECT
					if (!arr[i].promise) {
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
