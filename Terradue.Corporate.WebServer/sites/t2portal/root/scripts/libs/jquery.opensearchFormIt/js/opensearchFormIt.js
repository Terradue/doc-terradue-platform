/**
 * See (http://jquery.com/).
 * @name jQuery
 * @class 
 * See the jQuery Library  (http://jquery.com/) for full details.  This just
 * documents the function and classes that are added to jQuery by this plug-in.
 */
 
/**
 * See (http://jquery.com/)
 * @name fn
 * @class 
 * See the jQuery Library  (http://jquery.com/) for full details.  This just
 * documents the function and classes that are added to jQuery by this plug-in.
 * @memberOf jQuery
 */


OpensearchFormIt = {
	SEARCH_TERMS: "searchTerms",
	COUNT: "count",
};

OpensearchFormIt.defaultOptions = {
	libPath: "imports/jquery.opensearchFormIt", // change if you insert the library in a different folder
	defaultType: "application/atom+xml", // the default template type format to retrieve 
	osDescription: null,	// required - it can be an url or an array of urls
	mode: "auto", // options: "auto", "guided" (case insensitive)
	fieldParameters: null, // array - [only for "auto" mode]
							// specify the subset of parameters for form creation
							// (if null all parameters will be render as fields)
	mainFieldParameters: null, // array - [only for "auto" mode]
								// specify the subset of parameters denoted as "main".
								// they will be show first, while the others will be collapsed at startup
								// (if null or is all parameters, all parameters fields are main)
	excludeFieldParameters: null, // array - [only for "auto" mode]
								// specify the subset of parameters to exclude from field rendering,
								// i.e. to obtain it as external widgets
								// (to exclude from fieldParameters, or from all fieldParameters)
								// (if null no any parameter field are excluded)
	showCaptions: true, // boolean - [only for "auto" mode]
						 // if true some field title caption will be shown on the left of fields 
	fieldsMap: null, // map - [only for "guided" mode]
					 // for each parameter indicates what is the relative field
	fixedHeight: false,
	dataLoadedCallback: null, // called after created the opensearch slurper object
	errorCallback: null, // called if something goes wrong
	searchCallback: null, // function(urlInstance, mapData, data.os) - called when a search is done
}

// add some extensions
OpensearchFormIt.extensions = {
	count: "digits", // it's like >>  count: { rule: { digits: true } }
	startPage: "digits",
	startIndex: "digits"
};

// the default extension
OpensearchFormIt.defaultExtension = {
	field: '<input type="text"/>',
	rule: {}, // always valid
}

OpensearchFormIt.getExtension = function(parameterCompleteName){
	var extension = OpensearchFormIt.extensions[parameterCompleteName];
	if (extension != null){		
		console.log("extension found for "+parameterCompleteName);
		
		if (typeof(extension)=="string")
			extension = {rule: extension};
		
		if (extension.field==null)
			extension.field = OpensearchFormIt.defaultExtension.field;
		if (extension.rule==null)
			extension.rule = OpensearchFormIt.defaultExtension.rule;
		else if (typeof(extension.rule)=="string"){
			var rule = extension.rule;
			extension.rule = {};
			extension.rule[rule] = true;
		}
			
		return extension;
	} else {
		console.log("extension not found for "+parameterCompleteName+", default extension taken");
		return OpensearchFormIt.defaultExtension;
	}
}


;(function($){


/**
   * OPENSEARCH Form It! - a jQuery plugin to construct form around opensearch. 
   *
   * @verion 1.0
   * @author "Francesco Cerasuolo (francesco.cerasuolo@terradue.com)"
   *
   * @class opensearchFormIt
   * @memberOf jQuery.fn
   * 
   *  dependencies:
   *  - jquery.loadmask
   *
   */
$.fn.opensearchFormIt = function(opt){
	
	// the callback that manages the os object result
	function manageOpensearchSlurperObject(os) {
		if (options.dataLoadedCallback!=null)
			options.dataLoadedCallback(os);		
		data.os = os;
		
		// all parameters
		data.allParams = os.urls[options.defaultType].params;
		
		// the current parameters values map (form fields and not form fields), stored in memory
		data.values = {};
		data.oldValues = null; // previous values (if values==oldValues avoids search)
		
		// get field parameters
		if (options.fieldParameters==null)
			var tmpParams = data.allParams;
		else {
			var tmpParams = {};
			$.each(options.fieldParameters, function(){
				if (data.allParams[this]!=null)
					tmpParams[this] = data.allParams[this];
			});
		}
		
		// exclude field parameters
		data.params = {};
		$.each(tmpParams, function(){
			if ($.inArray(this.completeName, options.excludeFieldParameters)==-1)
				data.params[this.completeName] = this;
		});
		
		// init the form div
		data.$formDiv = $("<div class='osfi-formContainer'></div>");
		$div.append(data.$formDiv);
		createForm();
	}
	
	// get the main parameters subset from options (by checking if subset exists in the parameters set)
	function getMainFieldParameters(params, mainFieldParameters) {
		if (options.mainFieldParameters==null || options.mainFieldParameters.length==0)
			return [];
		var subset = [], n=0;
		$.each(params, function(){
			n++;
			if ($.inArray(this.completeName, mainFieldParameters)!=-1)
				subset.push(this.completeName);//subset
		});
		//console.log("---",subset);
		if (subset.length==n)
			return []; // in this case we have one set
		else
			return subset;
	}
	
	function retrieveFieldValues() {
		var map = {};
		$.each(data.params, function(){
			var cname = this.completeName,
				$field = data.fields[cname];
			//if ($field!=null && $field.val()!="")
			map[cname] = $field.val();
		});
		console.log(map);
		return map;
	}
	
	function createForm(){
		var $formDiv = data.$formDiv, params = data.params,
			$form = $('<form action="" class="osfi-form form-horizontal"></form>');
		data.$form = $form;
		data.fields = {};
		$formDiv.append($form);
		var $mainSubForm = $('<div class="osfi-form-mainSubForm"></div>').appendTo($form);
		data.$mainSubForm = $mainSubForm;
		
		// manage main parameters subset
		var mainFieldParameters = getMainFieldParameters(params, options.mainFieldParameters);
		if (mainFieldParameters.length>0) {
			var $secondarySubForm = $('<div class="osfi-form-secondarySubForm">').hide(),
				$showBtn = $('<p class="osfi-form-showHideParameters"><i class="icon-double-angle-right"></i> Show Other Parameters</p>'),
				$hideBtn = $('<p class="osfi-form-showHideParameters"><i class="icon-double-angle-up"></i> Hide Other Parameters</p>').hide();
			$form.append($showBtn).append($hideBtn).append($secondarySubForm);
			
			$showBtn.click(function(){
				$showBtn.toggle(); $hideBtn.toggle();
				$secondarySubForm.show("blind", 200);
			});
			$hideBtn.click(function(){
				$showBtn.toggle(); $hideBtn.toggle();
			    $secondarySubForm.hide("blind", 200);
			});
		}
		
		// init validation object
		var validationObj = {
			rules: {},
			highlight: function(element) {
				$(element).closest('.control-group').removeClass('success').addClass('error');
			},
			success: function(element) {
				element
				//.html('<i class="icon-check text-success"></i>').addClass('valid')
				.closest('.control-group').removeClass('error').addClass('success');
			},
			submitHandler: function(){
				console.log("osfi: submit!");
				
				// retrieve form field values
				var formValues = retrieveFieldValues();
				
				// extends the current values set with the form
				$.extend(data.values, formValues);
				
				doSearch();

				return false;
			}
		};
		
		// add fields (and validation) for each params
		$.each(params, function(){
			var
				param = this,
				cname = (param.ns==null ? "" : param.ns+"_") + param.name,
				extension = OpensearchFormIt.getExtension(param.completeName),
				$controlGroup = $('<div class="control-group">'),
				$controls = $('<div class="controls">');
			
			// add info to the field
			$field = $(extension.field).addClass("osfi-form-field")
				.attr("name", "osfi-param-"+cname)
				.attr("placeholder", param.completeName);
			
			// add the caption, if required
			if (options.showCaptions)
				$controlGroup.append($('<label class="control-label" for="'+cname+'">'+param.name+(param.isMandatory ? " *" : "") + '</label>'));
			
			// create the field control
			$controls.css("margin-left", (options.showCaptions ? "133px" : "0px"));
			data.fields[param.completeName] = $field;
			$controls.append($field);
			$controlGroup.append($controls);
			
			// add the field control to the form
			if (param.completeName==OpensearchFormIt.SEARCH_TERMS)
				$mainSubForm.prepend($controlGroup);
			else
				if (mainFieldParameters.length>0 && $.inArray(param.completeName, mainFieldParameters)==-1)
					$secondarySubForm.append($controlGroup);
				else
					$mainSubForm.append($controlGroup);
			
			// add the required rule if param is mandatory
			extension.rule.required = param.isMandatory;
			validationObj.rules["osfi-param-"+cname] = extension.rule;
		});
		
		window.validation = validationObj;
		$form.validate(validationObj);
		var $submitBtn = $("<button class='btn btn-info btn-mini'>Search</button>")
			.click(function(){ $form.submit(); }),
			$searchBar = $("<div class='osfi-searchButton'>");
		$formDiv.append($searchBar.append($submitBtn));

		if (options.fixedHeight){
			$formDiv.css({
				height: "100%",
				position: "relative",
			});
			$form.css({
				"margin-bottom": "0px",
				position: "absolute",
				width: "100%",
				top: "0px",
				bottom: "30px",
				"overflow-y": "auto",
				"overflow-x": "hidden",
			});
			$searchBar.css({
				height: "25px",
				position: "absolute",
				bottom: "0px",
				width: "100%",
			});
		}
	}
	
	function doSearch() {
		if (!equals(data.values, data.oldValues)){
			var	url = data.os.urls[options.defaultType],
			urlInstance = url.getUrlInstance(data.values);
			
			// clone values to old values
			data.oldValues = jQuery.extend(true, {}, data.values);
			
			if (options.searchCallback!=null)
				options.searchCallback(urlInstance, data.values, data.os);
			
			$div.trigger('change', {
				urlInstance: urlInstance,
				values: data.values,
				os: data.os,
			});
			
		}
	}

	
	function error(msg){
		if (options.errorCallback!=null)
			options.errorCallback(msg);
		throw("[OpensearchFormIt] "+msg); 
	}

	function getOptions(){
		var options = null;	
		if (opt!=null && typeof(opt)=="string")
			options = {osDescription: opt};
		
		options = $.extend({}, OpensearchFormIt.defaultOptions, opt);
		
		return options;
	}
	
	function checkOptionsIntegrity(){
		if (options.osDescription==null)
			error("Please select at least one opensearch description url.");
		if (options.mode.toLowerCase()!="auto" && options.mode.toLowerCase()!="guided")
			error("Mode \""+options.mode+"\" not supported.");
	}
	
	function equals(obj1, obj2) {
		function _equals(obj1, obj2) {
			var clone = $.extend(true, {}, obj1),
			cloneStr = JSON.stringify(clone);
			return cloneStr === JSON.stringify($.extend(true, clone, obj2));
		}

		return _equals(obj1, obj2) && _equals(obj2, obj1);
	}
	
	this.setParameterValue = function(paramCompleteName, value, triggerSearch){
		var $field = data.fields[paramCompleteName];
		data.values[paramCompleteName] = value;
		if ($field!=null)
			$field.val(value);
		if (triggerSearch)
			doSearch();
	}
	
	this.setParameterValues = function(parameters, triggerSearch){
		for (paramCompleteName in parameters){
			var value = parameters[paramCompleteName];
			var $field = data.fields[paramCompleteName];
			data.values[paramCompleteName] = value;
			if ($field!=null)
				$field.val(value);
		}
		
		if (triggerSearch)
			doSearch();
	}
	
	this.removeParameterValue = function(paramCompleteName, triggerSearch){
		var $field = data.fields[paramCompleteName];
		delete data.values[paramCompleteName];
		if ($field!=null)
			$field.val("");
		if (triggerSearch)
			doSearch();
	}

	
	this.getParameterValue = function(paramCompleteName){
		return data.values[paramCompleteName];
	}
	
	this.getParameterValues = function(){
		return data.values;
	}
	
	this.getOsObject = function(){
		return data.os;
	}
	
	this.getFormContainer = function(){
		return data.$mainSubForm;
	}
	
	this.getForm = function(){
		return data.$form;
	}
	
	this.addWidget = function($widget){
		data.$mainSubForm.append($widget);
	}
	
	this.getOpensearchObject = function(){
		return this.os;
	}
	
	////////////
	//  MAIN  //
	////////////
	
	// manage the options
	var options = getOptions(), $div = $(this), data={};
	checkOptionsIntegrity();
	
	if (typeof(options.osDescription)=="string") {
		// call the opensearchSlurper to manage the o.s. description
		$div.mask("Loading opensearch description...");
		OpensearchSlurper.parse(options.osDescription, function(os){
			this.os = os;
			manageOpensearchSlurperObject(os);
			$div.unmask();
		});		
	} else
		manageOpensearchSlurperObject(options.osDescription);
	
	return this;
};


})(jQuery);
