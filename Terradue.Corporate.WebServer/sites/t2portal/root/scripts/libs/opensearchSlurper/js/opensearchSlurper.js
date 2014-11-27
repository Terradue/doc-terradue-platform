/**
 * OPENSEARCH SLuRpEr
 * @module opensearchSlurper
 * @version 1.1
 * @author "Francesco Cerasuolo (francesco.cerasuolo@terradue.com)"
 * 
 */


/** String function analogue to java format
 * 
 * Usage example: "Hello, my name is {0} {1}".format("ciccio","ceras") 
 */
if (!String.prototype.format)
	String.prototype.format = function() {
		var args = arguments;
		return this.replace(/{(\d+)}/g, function(match, number) { 
			return typeof args[number] != 'undefined' ? args[number] : match;
		});
	};


OpensearchSlurper = {};

/**
 * Parse an opensearch description document to create an OpensearchSlurper object.
 * @param {string} url - The opensearch description document ur;.
 * @param {function} handler - The function called when the Opensearch object is created ( handler(os) )
 */
OpensearchSlurper.parse = function(url, handler, fail, $os) {
	
	// works on all browsers
	function getMapKeys(map){
		var keys = [];
		for (key in map) {
			keys.push(key)
		};
		return keys;
	}
	
	function getTemplateParameters(template) {
		var params = {};
		template.replace(/{(\w*):?(\w*)(\??)}/g, function(match, value, value2, value3){
			var withNs = (value2!=""),
				name = (withNs ? value2 : value),
				ns = (withNs ? value : null),
				isMandatory = (value3=="");
				completeName = (withNs ? ns+":" : "") + name;				
			
			params[completeName] = {
				name: name,
				ns: ns,
				completeName: completeName,
				isMandatory: isMandatory
			};
			
			//console.log("ns="+ns+", name="+name, isMandatory);
			//return "["+ns+":"+name+"]";
		});
		return params;
	}
	
	// read the opensearch descriptor url
	if ($os==null){
		$.get(url, function(osDescribe){
			$os = $(osDescribe);
			OpensearchSlurper.parse(url, handler, fail, $os);
		}).fail(function(jqXHR, textStatus, errorThrown){
			if (fail)
				fail(jqXHR, textStatus, errorThrown);
		});
		return;
	}
	
	// init
	var ris = {
		$osDocument: $os, // the document
		images: [], $images: [],
		urls: {}, templates: [],
	};
	
	// get the os document description info
	$.each(["ShortName","LongName","Description","Tags","Contact","Developer","Attribution","SyndicationRight","AdultContent","Language","OutputEncoding","InputEncoding"], function(){
		ris[""+this] = $os.find(""+this).text();
	});
	
	// create tags array
	ris.tagsArray = $os.find("Tags").text().split(" ");
	
	// queries
	ris.queries = {};
	$os.find("Query").each(function(){
		var $query = $(this),
			role = $query.attr("role");
		if (role!=null)
			ris.queries[role] = {
				role: role,
				searchTerms: $query.attr("searchTerms"),
				startPage: $query.attr("startPage"),
				totalResults: $query.attr("totalResults"),
				title: $query.attr("title"),
			};
	});
	
	// images
	$os.find("Image").each(function(){
		var image = {
			height: $(this).attr("height"),
			width: $(this).attr("width"),
			type: $(this).attr("type"),
			url: $(this).text(),
		};
		image.$img = $("<img>").attr("height",image.height).attr("width",image.width).attr("type",image.type).attr("src",image.url);
		ris.images.push(image);
		ris.$images.push(image.$img);
	});
	
	// url and templates
	$os.find("Url").each(function(){
		//console.log("Url", this);
		var $url = $(this),
			type = $url.attr("type"),
			url = {
				type: type,
				template: $url.attr("template"),
				rel: $url.attr("rel"),
				indexOffset: $url.attr("indexOffset"),
				pageOffset: $url.attr("pageOffset"),
			};
		url.params = getTemplateParameters(url.template);
		
		// map data to the template (encoding data)
		url.getUrlInstance = function(data){
			if (data==null) data = {};
			return url.template.replace(/{([\w:]+)(\??)}/g, function(match, completeName, value2){
				var isMandatory = (value2!="?");
				return data[completeName] == null ? "" : encodeURIComponent(data[completeName]);
			});
		};
		url.getParamNames = function(){
			return getMapKeys(url.params);
		}
		
		ris.urls[type] = url;
	});
	
	ris.getUrlTypes = function(){
		return getMapKeys(ris.urls);
	}
	
	handler(ris);
	
}
