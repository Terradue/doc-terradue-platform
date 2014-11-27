/**
 * WPS SLuRpEr
 * @module wpsSlurper
 * @version 1.0
 * @author "Francesco Cerasuolo (francesco.cerasuolo@terradue.com)"
 * 
 */

mapXmlToObject = function($xml, mapping){
	var obj = {};
	return obj;
}

WpsSlurper = {
	ns: {
		ows: 'http://www.opengis.net/ows/1.1',
		wps: 'http://www.opengis.net/wps/1.0.0'
	},
};

WpsSlurper.parseProcessDescription = function(url, fnSuccess, fnFail) {
	//url = '/geobrowser/fakeWpsDescription.xml'; // TODO: REMOVE
	$.get(url, function(wpsDescription){
		
		var ows = WpsSlurper.ns.ows,
			wps = WpsSlurper.ns.wps,
			$descr = $(wpsDescription).find('ProcessDescription'),
			ris = {
				$descriptionDocument: $(wpsDescription),
				identifier: $descr.childrenNsURI(ows, 'Identifier').text(),
				title: $descr.childrenNsURI(ows, 'Title').text(),
				description: $descr.childrenNsURI(ows, 'Abstract').text(),
				responseDocument: $descr.find("ProcessOutputs > Output").first().findNsURI(ows,"Identifier").text(),
				parameters: {}
			};
		
		$descr.find('DataInputs > Input').each(function(){
			var $input = $(this),
				identifier = $input.findNsURI(ows, "Identifier").text(),
				$allowedValues = $input.findNsURI(ows, "AllowedValues"),
				$complexData = $input.findNsURI(wps, "ComplexData");
			ris.parameters[identifier] = {
				identifier: identifier,
				title: $input.findNsURI(ows, "Title").text(),
				description: $input.findNsURI(ows, "Abstract").text(),
				minOccurs: $input.attr("minOccurs"),
				maxOccurs: $input.attr("maxOccurs"),
				isComplexData: ($complexData.length>0),
				allowedValues: ($allowedValues.length) ? 
					$allowedValues.findNsURI(ows, "Value").map(function(){return $(this).text()})
					: null,
			}
		});
		
		fnSuccess(ris);
		
	}).fail(function(jqXHR, textStatus, errorThrown){
		if (fnFail)
			fnFail(jqXHR, textStatus, errorThrown);
	});
	return;
};

WpsSlurper.parseCapabilities = function(url, fnSuccess, fnFail, depth, isFake, ris) {
	
	// works on all browsers
	function getMapKeys(map){
		var keys = [];
		for (key in map) {
			keys.push(key)
		};
		return keys;
	}
	
	// read the opensearch descriptor url
	if (ris==null){
		$.get(url+(isFake?'/GetCapabilities.xml':''), function(wpsCapabilities){
			ris = { $capabilitiesDocument: $(wpsCapabilities) };
			WpsSlurper.parseCapabilities(url, fnSuccess, fnFail, depth, isFake, ris);
		}).fail(function(jqXHR, textStatus, errorThrown){
			if (fnFail)
				fnFail(jqXHR, textStatus, errorThrown);
		});
		return;
	}
	
		
	if (ris.title==null){
		var ows = WpsSlurper.ns.ows,
		wps = WpsSlurper.ns.wps,
		$xml = ris.$capabilitiesDocument.children(),
		$identification = $xml.findNsURI(ows, 'ServiceIdentification'),
		$provider = $xml.findNsURI(ows, 'ServiceProvider'),
		$operations = $xml.findNsURI(ows, 'OperationsMetadata'),
		$offerings = $xml.findNsURI(wps, 'ProcessOfferings'),
		$languages = $xml.findNsURI(wps, 'Languages');
		
		ris.title = $identification.childrenNsURI(ows, 'Title').text();
		ris['abstract'] = $identification.childrenNsURI(ows, 'Abstract').text();
		ris.description = ris['abstract'];
		ris.keywords =  $identification.childrenNsURI(ows, 'Keywords').childrenNsURI(ows, 'Keyword').map(function(){return $(this).text();});
		ris.version = $xml.children().findNs('ows', 'ServiceTypeVersion').first().text();
		
		ris.operations = {
			describeProcess: $operations.find('[name="GetCapabilities"]').findNsURI(ows, 'Get').attr('xlink:href'),
			execute: $operations.find('[name="Execute"]').findNsURI(ows, 'Get').attr('xlink:href'),
		};

		var $processes = $offerings.childrenNsURI(wps, 'Process');
		if (depth){
			ris.processes = {};
			$processes.each(function(){
				ris.processes[$(this).childrenNsURI(ows, 'Identifier').text()] = {};
			});
			WpsSlurper.parseCapabilities(url, fnSuccess, fnFail, depth, isFake, ris);
		} else {
			ris.processes = $offerings.childrenNsURI(wps, 'Process').childrenNsURI(ows, 'Identifier').map(function(){return $(this).text();});
			fnSuccess(ris);
		}
	} else {
		// we are in depth
		for (identifier in ris.processes){
			var process = ris.processes[identifier];
			if (process.identifier == null){
				process.identifier = identifier;
				var describeBaseUrl = ris.operations.describeProcess,
					interrogative = (describeBaseUrl[describeBaseUrl.length-1] == '?' ? '' : '?'),
					describeUrl = (isFake ? url+'/'+identifier+'.xml' : describeBaseUrl + interrogative +'service=WPS&version='+ris.version+'&request=DescribeProcess&identifier='+identifier);
				WpsSlurper.parseProcessDescription(describeUrl, function(description){
					ris.processes[identifier] = description;
					WpsSlurper.parseCapabilities(url, fnSuccess, fnFail, depth, isFake, ris);					
				});
				return;
			}
		}
		fnSuccess(ris);
	}
	
};


WpsSlurper.parseExecute = function(url, fnSuccess, fnFail) {
	
	var getUrlParameters = function(url) {
		var params={};
		
		try {
			var pageUrl = url.split('?')[1];
			var urlParams = pageUrl.split('&');
			for (var i=0; i<urlParams.length; i++) {
				var param = urlParams[i].split('=');
				if (param.length==2)
					params[param[0]] = param[1];
			}
		} catch (e){};
		return params;
	};

	
	var ows = WpsSlurper.ns.ows,
		wps = WpsSlurper.ns.wps;
	
	$.get(url, function(xml){
		var $xml = $(xml),
			statusLocation = $xml.findNsURI(wps, "ExecuteResponse").attr("statusLocation"),
			jobId=null;
			
		if (statusLocation){
			var params = getUrlParameters(statusLocation),
				jobId = params.id;
		}
			
		fnSuccess({
			$executeResult: $xml,
			statusLocation: statusLocation,
			exceptionText: $xml.findNsURI(ows, 'ExceptionText').text(),
			jobId: jobId
		});
		
	}).fail(function(jqXHR, textStatus, errorThrown){
		if (fnFail)
			fnFail(jqXHR, textStatus, errorThrown);
	});
};


WpsSlurper.parseStatus = function(url, fnSuccess, fnFail) {
	var ows = WpsSlurper.ns.ows,
		wps = WpsSlurper.ns.wps;
	
	$.get(url, function(xml){
		var $xml = $(xml), 
			ris = { $statusResult: $xml };
		
		// if process succeeded
		if ($xml.findNsURI(wps, "ProcessSucceeded").length>0) {
			ris.isSucceeded = true;
			ris.isTerminated = true;
		} else if ($xml.findNsURI(wps, "ProcessFailed").length>0) {
			ris.isFailed = true;
			ris.isTerminated = true;
			ris.exceptionText = $xml.findNsURI(ows, 'ExceptionText').text();
		} else if ($xml.findNsURI(wps, "ProcessStarted").length>0) {
			ris.isRunning = true;
			ris.isTerminated = false;
			
			var $processStarted = $xml.findNsURI(wps, "ProcessStarted"),
				percentCompleted = $processStarted.attr("percentCompleted");
			ris.percent = percentCompleted==null ? $processStarted.text() : percentCompleted;
		}
		
		fnSuccess(ris);
		
	}).fail(function(jqXHR, textStatus, errorThrown){
		if (fnFail)
			fnFail(jqXHR, textStatus, errorThrown);
	});
};




