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

if (!String.prototype.startsWith)
	String.prototype.startsWith = function(needle) {
		return(this.indexOf(needle) == 0);
	};


OpensearchNavigator = {
	RESULT_FORMAT: "application/json",
};

OpensearchNavigator.defaultOptions = {
	
	innerShadow: true,
	
	libPath: "imports/jquery.opensearchNavigator", // change if you insert the library in a different folder
	osDescription: null,	// required - it can be an url or an array of urls
	mapIds: ["ciccioceras.g7f8goff", "examples.map-qfyrx5r8"],
	fullscreen: false, // fullscreen support, only for the map.
	fullscreenPosition: 'bottomright',
	zoomPosition: 'topright',
	drawPosition: 'topright',
	searchOnMapMove: false, // if true, perform search (with bbox) when mapview change
	
	resultsJsonTableOptions: {  // see jquery.jsonTable
		noDataToDisplay: "<p class='text-error'>No results found.</p>",
		tableClass: "table table-striped table-condensed table-hover osn-results-table",
		css: { "font-size" : "10px" },
		idColumn: "id",
	},
	tableOffset: 0,
	
	//resultsSelectable: false,
	resultsSelectable: {
		selectButton: null, //"Select these items",
		selectHandler: function(jsonRows){
			alert("Selected "+jsonRows.length+" elements.");
			console.log(jsonRows);
		},
		onFeaturesSelected: null, // function that fires when user select one or more features 
	},
	
	onFeatureClick: null, // function that fires when user clicks on a feature 

	
	// popupContent (false | function): it can be false if no popup are shown, 
	// 	or a string:function(id, properties) that creates html content starting from the properties obj
	popupContent: false,

	// externalContent (false | object): it can be false if no external content are shown,
	// or an object with two parameters:
	// 		- element [string | jquery element | html element] : the external element 
	//				  that will contains properties info (if string a jquery selector)
	//		- content string|jquery element: function(properties): the function that creates 
	//				  the content starting from the properties 
	externalContent: false,
	
	zoomOnFeatures: false, // if true the map pan over the clicked feature
	panOnFeatures: true,
	
	// it can be a jquery selector or an element of a preexistent container. if false or null the control is inside the map
	externalSearchControl: false, 
	
	// it can be a jquery selector or an element of a preexistent container. if false or null the control is inside the map
	externalResultsControl: false,
	
	searchControlStyle: {},//height:"inherit", width:"inherit", position:"absolute", top:0, bottom:0, left:0, right:0
	resultsControlStyle: {},
	resultsWidth: 300,
	resultsHeight: 300,
	
	// features styles: default highlighted and hovered styles (null accepted for no specification)
	featureStyle: { color: "#ff7800", fillOpacity: 0.4, opacity: 1, weight: 1 },
	highlightedFeatureStyle: { color: '#666', fillOpacity: 0.5, weight: 3, dashArray: '' },
	hoveredFeatureStyle: { fillOpacity: 0.6, weight:2 },
	
	pagination: true, // true if pagination are managed
	count: 20,
	
	searchDoneCallback: null, // if not null it's triggered when a search is done
	searchFailMessage: function(jqXHR, textStatus, errorThrown){
		return "<h4>Error</h4><br/>"+textStatus;
	},
	searchFailCallback: null,
	
	onMapLoad: null // function(map)
}

OpensearchNavigator.RECTANGLE = "rectangle";
OpensearchNavigator.POLYGON = "polygon";



;(function($){


/**
   * OPENSEARCH Navigator - a jQuery plugin to construct a gis client around opensearch. 
   *
   * @verion 1.0
   * @author "Francesco Cerasuolo (francesco.cerasuolo@terradue.com)"
   *
   * @class opensearchNavigator
   * @memberOf jQuery.fn
   * 
   *  dependencies:
   *  - jquery.loadmask
   *  - jquery.opensearchFormIt
   *
   */
$.fn.opensearchNavigator = function(opt){
	
	this._getOptions = function(){
		var options = null;	
		if (opt!=null && typeof(opt)=="string")
			opt = {osDescription: opt};
		
		options = $.extend(true, {}, OpensearchNavigator.defaultOptions, opt);
		
		return options;
	}
	
	this._checkOptionsIntegrity = function() {
		if (this.options.osDescription==null)
			this._error("Please select at least one opensearch description url.");
	}
	
	this._error = function(msg){
		if (this.options.errorCallback)
			this.options.errorCallback(msg);
		else
			alert("Opensearch Navigator\n\n"+msg);
		throw("[OpensearchNavigator] "+msg); 
	}

	this._initAll = function(os) {
		var data = this.data, options=this.options;			
		
		// init the map
		var map = new L.Map(this.$map.get(0), {
				zoomControl: false,
				center:[0,0],
				zoom:2,
				worldCopyJump: true,
			});

		var resultsLayer = new L.FeatureGroup().addTo(map);
		
		// layers control
		this._initLayersControl(map, resultsLayer);
		
		// zoom control
		new L.Control.Zoom({ position: options.zoomPosition }).addTo(map);
		
		// init the search form control
		this._initSearchControl(map, os);
		
		// draw control
		this._initDrawControl(map);
		
		// init the results control
		this._initResultsControl(map, os);
		
		// init the fullscreen control
		if (options.fullscreen)
			new L.control.fullscreen({position: options.fullscreenPosition }).addTo(map);
		
		// init search on map move (if enabled)
		if (options.searchOnMapMove)
			this._initSearchOnMapMove(map);
		
		this._initLoader();
		
		data.map = map;
		data.resultsLayer = resultsLayer;
		data.os = os;
		
		if (options.onMapLoad)
			options.onMapLoad(map, data);

	}
	
	this._initLayersControl = function(map, resultsLayer){
		var baseLayers = {}, self = this, first=null;
		
		$.each(this.options.baseMaps, function(name){
			var mapId = this, layer = null;
			
			if (mapId.startsWith('google:')){
				try {
					var mapType = mapId.substring(7, mapId.length);
					layer = new L.Google(mapType);
					$(layer._container).css('z-index',0); // fix z-index
				} catch (e){};
			}
			
			else if (mapId.startsWith('http'))
				layer = L.tileLayer(mapId, {});
			
			else if (mapId.startsWith('mapbox:')){
				var mapId = mapId.substring(7, mapId.length);
				layer = L.tileLayer('http://{s}.tiles.mapbox.com/v3/'+mapId+'/{z}/{x}/{y}.png', {});
			}
			
			else
				layer = L.tileLayer('http://{s}.tiles.mapbox.com/v3/'+mapId+'/{z}/{x}/{y}.png', {});
			
			if (layer){
				baseLayers[name] = layer;
				if (!first){
					layer.addTo(map);
					first = true;
				}
			}
		});
		var layerControl = L.control.layers(baseLayers, null).addTo(map);
		layerControl.addOverlay(resultsLayer, "Data Results");
		this.data.layerControl = layerControl;
	}
	
	this._initSearchOnMapMove = function(map){
		var self = this, data=this.data,
			$check = $('<div class="searchOnMapMove"><i class="fa fa-refresh"></i></div>')
				.popover({
					trigger: 'hover',
					content: 'Enable/disable performing dynamic search when map move',
				}),
			updateBoundingBoxView = function(){
				// remove previous bounding boxes
				data.selectionLayer.clearLayers();
				
				// get bounds
				var bounds = map.getBounds()
					w=self._limitatex(bounds.getWest()),
					s=self._limitatey(bounds.getSouth()),
					e=self._limitatex(bounds.getEast()),
					n=self._limitatey(bounds.getNorth()),					
					bbox = w+","+s+","+e+","+n;
				
				// update parameters
				data.$osfi.removeParameterValue("geo:geometry");
				data.$osfi.setParameterValue("geo:box", bbox, true);
			};
		
		new L.Control.Button({
			position: "topleft",
			content: $check,
			handler: function(){
				if ($check.hasClass('enabled'))
					$check.removeClass('enabled');
				else{
					$check.addClass('enabled');
					updateBoundingBoxView();
				}
			},
		}).addTo(map);

		map.on('moveend', function(e){
			if ($check.hasClass('enabled'))
				updateBoundingBoxView();
		});
	}
	
//	this._initModeSwitcher = function() {
//		var 
//			self = this,
//			$switch = $('<div class="osn-modeSwitcher">'
//				+ '<a href="javascript://" class="btn btn-mini btn-info navigationModeBtn">Navigation Mode</a>'
//				+ '<a href="javascript://" class="btn btn-mini disabled selectionModeBtn">Selection Mode</a>'
//				+'</div>'),
//			$navBtn = $switch.find('.navigationModeBtn'),
//			$selBtn = $switch.find('.selectionModeBtn');
//		
//		this.isSelectionEnabled = false;
//		$switch.find('a').click(function(){
//			var $this = $(this)
//				isSelectionEnabled = $this.hasClass('selectionModeBtn'),
//				$other = isSelectionEnabled ? $navBtn : $selBtn;
//			if ($this.hasClass('disabled')){
//				$this.removeClass('disabled').addClass('btn-info');
//				$other.removeClass('btn-info').addClass('disabled');
//				self.isSelectionEnabled = isSelectionEnabled;
//				self._switchMode(isSelectionEnabled);
//			}
//		});
//
//		$(this).append($switch);
//	} //TOREMOVE
	
	this._switchMode = function(siwtchToSelectionMode) {
		var data = this.data, that=this;
		if (data && data.$results && data.$results.$table){
			var $table = data.$results.$table;
			$table.selectable(siwtchToSelectionMode ? "enable" : "disable");
			that.$selectBtn.hide();
			$table.find('.ui-selected').removeClass('ui-selected');
		}
	}
	
	this._initLoader = function() {
		this.data.$loader = $("<div class='osn-loader'><i class='icon-spinner icon-spin'></i><br/>Loading</div>")
			.appendTo($(this));
	}
	
	this._initResultsControl = function(map, os) {
		var data=this.data, options=this.options;
		
		var $results = options.externalResultsControl ?
				$(options.externalResultsControl)
				: $("<div id='osn-result-"+intRnd(100000,99999)+"'>");
				
		$results.addClass("osn-results").css(this.options.resultsControlStyle);		
		
		//var $results = $("<div class='osn-results' id='osn-result-"+intRnd(100000,99999)+"'>").css(this.options.resultsControlStyle).html("Results!");
		
		if (!options.externalResultsControl){
			var resultsControl = new L.Control.Expandible({
				collapsedIcon: "icon-list",
				expandedContent: $results,
				disableMapScroll: true,
				padding: "10px",
				resizable: { initialWidth: options.resultsWidth, initialHeight: options.resultsHeight },
				overflow: "inherit",
			}).addTo(map).disable();
			data.resultsControl = resultsControl;
		}
		
		data.$results = $results;
	}
	
	this._initSearchControl = function(map, os) {
		var that=this, data=this.data, options=this.options;
		// call the opensearch form-it!
		var $osfi = $("<div class='osn-searchForm'></div>")
			.css(options.searchControlStyle)
			.opensearchFormIt({
				osDescription: os,
				mainFieldParameters: ["searchTerms", "geo:geometry"],
				excludeFieldParameters: (options.pagination ? ["startPage", "startIndex"] : null),
				showCaptions: false,
				fixedHeight: true,
				defaultType: OpensearchNavigator.RESULT_FORMAT,
				errorCallback: function(msg){
					$(that).html(msg);
				},
				searchCallback: function(url){that._search(url)},
			});
		blockScrollOnBounds($osfi.getForm());
		
		// add pagination options
		if (options.pagination){
			var indexOffset = os.urls[OpensearchNavigator.RESULT_FORMAT].indexOffset;
			$osfi.setParameterValue("count", options.count, false);
			$osfi.setParameterValue("startIndex", parseInt(indexOffset), false);
		}
		
		var osfiControl = new L.Control.Expandible({
			expandedContent: $osfi,
			disableMapScroll: true,
//			resizable: {initialWidth: 100, initialHeight:200},
		}).addTo(map);

		data.$osfi = $osfi;
		data.osfiControl = osfiControl;
		
		// date pickers
		$('.dateInput').datepicker({format: "yyyy-mm-dd"}).on("changeDate", function(){
			 $(this).datepicker("hide");
		});
	}
	
	this._initDrawControl = function(map) {
	    var data = this.data, that=this,
	    	selectionLayer = new L.FeatureGroup().addTo(map);
			drawControl = new L.Control.Draw({
				position: this.options.drawPosition,
				draw: {
					circle: false,
					marker: false,
					polyline: false,
					rectangle: {
						repeatMode: true,
		 				shapeOptions: {
		                    color: 'blue',
		                    weight: 3,
		                    fill: true,
		                    clickable: false
		            	}
					},
					polygon: {
						allowIntersection: false,
		 				shapeOptions: {
		 					weight: 3,
		                    fill: true,
		                    clickable: false
		            	}
					},
				},
				edit: false,
			}).addTo(map);
		window.drawControl = drawControl; // TODO toremove
			
		map.on('draw:created', function(e) {
			selectionLayer.clearLayers();
			e.layer.options.fill = false;
			selectionLayer.addLayer(e.layer);
			
			// manage bounding box
			if (e.layerType==OpensearchNavigator.RECTANGLE)
				that._setBboxFromSelection(e.layer);
			else if (e.layerType==OpensearchNavigator.POLYGON)
				that._setGeometryFromSelection(e.layer);
		});
		data.selectionLayer = selectionLayer;
		data.drawControl = drawControl;
	}
	
	this._initTest = function(map) {
		new L.Control.Expandible({
			collapsedIcon: "icon-info",
			expandedContent: "<a href='http://www.terradue.com' target='_blank'><img src='https://pbs.twimg.com/profile_images/3307145037/f6088330c5034bc724656cbf04d751cc_bigger.png'/>By Terradue</a>"
		}).addTo(map);

		new L.Control.Expandible({
			position: "bottomright",
			collapsedText: "Mac & Text test <i class='icon-exclamation-sign' />",
			expandedContent: $('<img src="http://www.cloudave.com/wordpress/wp-content/uploads/2013/10/apple-logo-png-transparent-i0.png" />'),
		}).addTo(map);
		
		var sexyControl = new L.Control.Expandible({
			position: "topright",
			collapsedContent: $('<img src="http://fla.fg-a.com/hot-animation-11.gif" style="width:50px; height:47px" />'),
			expandedContent: "<div class='kateUpton'></div><p style='margin:0px; text-align:center'>Hi dude! <a href='javascript:closeMe()'><small>(close me)</small></a></p>",
		}).addTo(map);
		
		window.closeMe = function(){ map.removeControl(sexyControl); };
	}
	
	this._setBboxFromSelection = function(layer){
		var data=this.data, bounds = layer.getBounds(),
			w=this._limitatex(bounds.getWest()),
			s=this._limitatey(bounds.getSouth()),
			e=this._limitatex(bounds.getEast()),
			n=this._limitatey(bounds.getNorth()),					
			bbox = w+","+s+","+e+","+n;
		
		data.$osfi.removeParameterValue("geo:geometry");
		data.$osfi.setParameterValue("geo:box", bbox, true);
		return bbox;
	}
	
	this._setGeometryFromSelection = function(layer) {
		var data=this.data, firstPoint = layer.getLatLngs()[0], geom = "POLYGON((";
		$.each(layer.getLatLngs(), function(i){
			geom += this.lng + " " + this.lat + ",";
		});
		geom += firstPoint.lng + " " + firstPoint.lat + "))"; // add the first point as last
		data.$osfi.removeParameterValue("geo:box");
		data.$osfi.setParameterValue("geo:geometry", geom, true);
		return geom;
	}
	
	this._search = function(generatedUrl){
		var data=this.data, options=this.options, that=this;
		// show url (only for test/debug)
		var showFn = function(){
			$(this).html('<a href="'+generatedUrl+'">'+generatedUrl+'</a>').show("fade");
		};
		if ($("#generatedUrl").is(":visible"))
			$("#generatedUrl").hide("fade", 200, showFn);
		else
			showFn.apply($("#generatedUrl"));
		
		// TODO: remove (debug url)
		//var generatedUrl = "http://10.13.0.6:8082/ngeo/catalogue/ASA_IM__0P_ENVISAT-1_ASAR_IM/search?format=json";
		
		// TODO manage more than one os
		
		// load data
		if (options.fakeResults){
			data.$loader.show();
			setTimeout(function(){
				data.$loader.fadeOut();
				if (!options.externalResultsControl)
					data.resultsControl.enable().expand();
				data.osfiControl.collapse();
				
				var json = that._getFakeResults();
				data.results = json;
				fillFeaturesWithIds(json);
				that._showResultOnMap(json);
				that._showResultOnTable(json);
			}, 1000);
		} else {
			//alert(generatedUrl);
			data.$loader.show();
			$.get(generatedUrl, function(json){
				data.results = json;
				fillFeaturesWithIds(json);
				data.$loader.fadeOut();
				if (!options.externalResultsControl)
					data.resultsControl.enable().expand();
				data.osfiControl.collapse();
				
				that._showResultOnMap(json);
				that._showResultOnTable(json, generatedUrl);
				
				if (options.searchDoneCallback)
					options.searchDoneCallback(json, generatedUrl);
			}, "json").fail(function(jqXHR, textStatus, errorThrown){
				if (options.searchFailCallback)
					options.searchFailCallback(jqXHR, textStatus, errorThrown);
				else {
					data.$loader.fadeOut();
					var $errorPanel = $('<div class="alert alert-block alert-error osn-searchFailPanel">'),
						$closeButton = $('<button type="button" class="close" data-dismiss="alert">&times;</button>')
							.click(function(){
								$errorPanel.remove();
							});
					$errorPanel.append($closeButton, options.searchFailMessage(jqXHR, textStatus, errorThrown))
						.appendTo($(that));
				}
			});			
		}
		
	}
	
	// if features haven't id, add some generated ids
	function fillFeaturesWithIds(json){
		$.each(json.features, function(index){
			if (this.id==null)
				this.id = "feature"+index;
		});
	}
	
	function intRnd(from, to) {
		var totValues = Math.abs(to-from)+1;
		return Math.floor((Math.random()*totValues))+from;
	}
	function arrayRnd(ar) {
		return ar[intRnd(0, ar.length-1)];
	}
	var fakeTitles = ["Fake Random Shape!","Yeah, this is a sample shape","Another random shape","Sample title","Lorem ipsum Title","<i class='icon-twitter'></i> Twitter icon sample!"];
	var fakeImages = ["https://pbs.twimg.com/profile_images/3307145037/f6088330c5034bc724656cbf04d751cc_bigger.png"
	                  ,"https://pbs.twimg.com/profile_images/3758828651/cdcce81a7ddb53d0aab5a6b86f4905a8_bigger.png"
	                  ,"https://pbs.twimg.com/profile_images/1778131615/logo_sscn_tw_bigger.jpg"
	                  ,"https://pbs.twimg.com/profile_images/55299999/Picture_3_bigger.png"
	                  ,"https://pbs.twimg.com/profile_images/378800000213779789/3c64e66195fbe763ec43d4c525943c7f_bigger.jpeg"
	                  ,"https://pbs.twimg.com/profile_images/2170641782/mapbox-symbol-256_reasonably_small.png"];
	var fakeTexts = [
	                 "What if the map on your iPhone was responsive, changing the shadows based on the time of day? We‘re experimenting with this new level of customizable maps. From planning a...",
	                 "Exploring the Ocean: Underwater drones are changing the way scientists study the sea http://www.nytimes.com/video/ science/100000002525812/ exploring-the-ocean.html?smid=tw-nytimes … @nytimesscience #climatechange",
	                 "Open Web and Cloud Computing e-Infrastructure for Earth Sciences",
	                 "Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
	                 "This is a sample text, lorem ipsum dolor sit amet consectetur adipiscing elit",
	                 ];
	
	this._getFakeResults = function(){
		var n=60, l=5, features = [];
		for (var i=0; i<n; i++) {
			var lon = intRnd(-175, 175),
				lat = intRnd(-70,70),
				r = intRnd(2,5);
			features.push({
				type: "Feature",
				//"properties": {"party": "Republican"},
				geometry: {
					type: "Polygon",
					coordinates: [[
						[lon-r, lat-r],
						[lon-r, lat+r],
						[lon+r, lat+r],
						[lon+r, lat-r],
					]]
				},
				id: "object_id_"+intRnd(1000000, 10000000),
				properties: {
					title: arrayRnd(fakeTitles),
					description: arrayRnd(fakeTexts),
					dateTime: "2013-11-19T11:43Z",
					coordinates: [[
					               [lon-r, lat-r],
					               [lon-r, lat+r],
					               [lon+r, lat+r],
					               [lon+r, lat-r],
					               ]],
					url: "http://www.terradue.com",
					logo: arrayRnd(fakeImages),
				},
			});
		}
		
		return { features: features };
	};
	
	this._showResultOnMap = function(json) {
		var options=this.options, 
			style = options.featureStyle,
			pointToLayer = options.pointToLayer,
			data=this.data, that=this;
		
		// remove previous layer features
		data.resultsLayer.clearLayers();
		data.layersMap = {}, data.selectedLayers=[], data.hoveredLayers=[];
		
		$.each(json.features, function(){
			var f = this,
				opt = { style: style };
			if (pointToLayer)
				opt.pointToLayer = pointToLayer;
			
			var l = L.geoJson(f, opt).addTo(data.resultsLayer);
				//l = L.geoJson(f, {style: style, pointToLayer: pointToLayer}).addTo(data.resultsLayer);
			// popup
			if (typeof(options.popupContent)=="function")
				l.bindPopup(options.popupContent(f.id, f.properties), {autoPan: options.panOnFeatures});
			
			// click event on the feature
			l.on("click", function(e){
				
				// zoom on feature
				if (options.zoomOnFeatures)
					data.map.fitBounds(e.target.getBounds(), {padding: [100, 100]});
				// pan on feature
				else if (options.panOnFeatures)
					data.map.panTo(e.target.getBounds().getCenter());
				
				// get feature
				var feature = this.getLayers()[0].feature;
				if (feature==null)
					return;
				
				that._featuresSelected([feature], false);
				
//				if (options.resultsSelectable && options.resultsSelectable.onFeaturesSelected)
//					options.onFeaturesSelected([feature]);
//				
//				that._showExternalContent(feature);
//				
//				that._highlightFeatureOnTable(feature, true);
//				
//				that._highlightFeatureOnMap(l); //TOREMOVE
					
			});
			
			that._bindHoverEvent(l);
			
			data.layersMap[f.id] = l;
		});
	}

	
	this._bindHoverEvent = function(l){
		var options=this.options, that=this;
		if (options.hoveredFeatureStyle)
			l.on({
				mouseover: function(e){
					l.setStyle(options.hoveredFeatureStyle);
					if (!L.Browser.ie && !L.Browser.opera)
						l.bringToFront();
				},
				mouseout: function(e){
//					if (!that._isLayerSelected(l))
					if (l.isSelected)
						l.setStyle(options.highlightedFeatureStyle);
					else
						l.resetStyle(e.target);
				},
			});
	}
	
//	this._isLayerSelected = function(l){
//		// the layer is selected if is in the data.selectedLayers array
//		return ($.inArray(l, this.data.selectedLayers)!=-1);
//	}
	
	this._showExternalContent = function(feature){
		var options=this.options,
			content = options.externalContent.content,
		showJson = options.externalContent.showJson,
		$element = $(options.externalContent.element).empty(); 
		
		if (content!=null && typeof(content)=="function")
			$element.append(content(feature.id, feature.properties));
		if (showJson)
			$element.append($("<div class='osn-json'>").jsonViewer(feature.properties));
	}

	
	// highlight table row and scroll to it (binding from table to)
//	this._highlightFeatureOnTable = function(feature, pan){
//		var options=this.options, data=this.data;
//		if (options.highlightTable){
//			data.$results.$table.unhighlightAll().highlight(feature.id); // TODO change for more than one repo
//			
//			if (pan){
//				var $tr = data.$results.$table.getRow(feature.id).$tr,
//				$table = data.$results.$table,
//				rowPos = Math.abs($tr.position().top - $table.find(".table").position().top);
//				
//				var $toAnimate = (options.externalResultsControl) ? $(options.externalResultsControl) : data.$results.$table;
//				$toAnimate.animate({ scrollTop: rowPos }, 'slow');
//			}
//		}
//	} //TOREMOVE
	
	// highlight table row and scroll to it (binding from table to)
	this._highlightFeaturesOnTable = function(features){
		if (features.lenght==0)
			return;
		
		var options=this.options, data=this.data;
		
		var minTrTop = null;
		$.each(features, function(){
			var feature = this;
			data.$results.$table.unhighlightAll().highlight(feature.id); // TODO change for more than one repo
			
			var trTop = data.$results.$table.getRow(feature.id).$tr.position().top;
			if (minTrTop==null || trTop<minTrTop)
				minTrTop = trTop;
		});
		
		// pan the first element on the table (scroll to show on the table)
		var $table = data.$results.$table,
			rowPos = Math.abs(minTrTop - $table.find(".table").position().top)+options.tableOffset;
		
		var $toAnimate = (options.externalResultsControl) ? $(options.externalResultsControl) : data.$results.$table;
		$toAnimate.animate({ scrollTop: rowPos }, 'slow');
	}

this._showResultOnTable = function(jsons, generatedUrl) {
		var options=this.options, data=this.data, that=this;
		if (!$.isArray(jsons))
			jsons = [jsons];
		
		data.$results.empty();
		
		// iterate each json result (for each
		$.each(jsons, function(i){
			var json = this,
				propertiesTable = [];

			// transform the result in table of properties
			$.each(json.features, function(){
				this.properties.id = this.id;
				propertiesTable.push(this.properties);
			});
			
			if (options.pagination)
				data.$results.append(that._getPagination(json));
			
			var $table = $("<div class='osn-results-tableContainer'>").jsonTable(propertiesTable, $.extend({}, options.resultsJsonTableOptions, 
					{
//						onRowClick:function(id, json){
//							that._rowClicked(id);
//						},//TOREMOVE
						onRowMouseover: function(id, jsonRow, $tr){
							var l = data.layersMap[id];
							l.setStyle(options.hoveredFeatureStyle);
							if (!L.Browser.ie && !L.Browser.opera)
								l.bringToFront();
//							that._rowHover(id); //TOREMOVE
						},
						onRowMouseout: function(id, jsonRow, $tr){
							var l = data.layersMap[id];
							if (l.isSelected)
								l.setStyle(options.highlightedFeatureStyle);
							else
								l.resetStyle(l);
//							that._rowHover(id); //TOREMOVE
						},
						highlightRow: function($tr, jsonRow){
							$tr.addClass("ui-selected");
						},
						unhighlightRow: function($tr){
							$tr.removeClass("ui-selected");
						},
					}
			));
			$table.children('table').addClass('osn-results-table');
			if (!options.externalResultsControl){
				$table.addClass("osn-results-tableContainer-internal");
				blockScrollOnBounds($table);
			}
				
			// selectable feature
			if (options.resultsSelectable){
				if (options.resultsSelectable.selectButton){
					var $selectBtn = $("<button class='btn btn-mini btn-warning osn-results-selectBtn'>"+options.resultsSelectable.selectButton+"</button>").hide()
					.click(function(){
						var ids = $table.find(".ui-selected").map(function(){ return $(this).data("id"); });
						if (options.resultsSelectable.selectHandler)
							options.resultsSelectable.selectHandler($table.getRows(ids).data);
					}).appendTo(data.$results);					
					that.$selectBtn = $selectBtn;
				}
				
				$table.selectable({
					filter: "tbody tr",
					stop: function(event, ui){
						var $rows = $(this),
							features = $rows.find('.ui-selected').map(function(){
								var id = $(this).data('id'),
									layer = data.layersMap[id];
								return layer.getLayers()[0].feature;
							});
						
						// if there's select button, manage show/hide
						if (options.resultsSelectable.selectButton){
							if (features.length>0)
								$selectBtn.show();
							else
								$selectBtn.hide();
						}
						
						that._featuresSelected(features, true);
						
					},
					//disabled: !that.isSelectionEnabled, //TOREMOVE
				});
			}
			
			
			data.$results.append($table);
			data.$results.$table = $table; // TODO change for more than one repo
		});
	}
	
	this._featuresSelected = function(features, triggeredFromTable){
		var options=this.options, data=this.data;
		
		// trigger event, if avaiable
		if (options.resultsSelectable && options.resultsSelectable.onFeaturesSelected)
			options.resultsSelectable.onFeaturesSelected(features);
		
		// show external content, if avaiable and one object is selected
		if (options.externalContent && fetures.length==1)
			that._showExternalContent(features[0]);
		
		if (triggeredFromTable){
			if (features.length==1){
				var l = data.layersMap[features[0].id];
				if (options.zoomOnFeatures)
					data.map.fitBounds(l.getBounds(), {padding: [100, 100]});
				else if (options.panOnFeatures)
					data.map.panTo(l.getBounds().getCenter());
			}
		}
		
		if (!triggeredFromTable){
			console.log('highlight features on table');
			this._highlightFeaturesOnTable(features);
		}
		
		// only if one feature is selected
		if (features.length==1){
			// trigger click event, if avaiable
			if (options.onFeatureClick)
				options.onFeatureClick(features[0]);
			
			// show external content, if avaiable
			this._showExternalContent(features[0]);

			// show popup content, if avaiable
			if (typeof(options.popupContent)=="function"){
				var l = data.layersMap[features[0].id];
				this._getSubLayersRecursive(l)[0].openPopup();
			}
		}
		
		// highlight features on map
		this._highlightFeaturesOnMap(features);
		
		// if there's draggable options, manage that
		if (options.resultsDraggable){
			var draggableOptions = $.extend({
				cursor: 'move',
				cursorAt: { top: 9, left: 12 },
				helper: function(a,b,c){
					return $('<div class="osn-dragHelper">')
						.html('<i class="fa fa-arrows"></i> ' 
							+ features.length
							+ ' feature' + (features.length==1 ? '' : 's')+ ' selected');
				},
				appendTo: 'body',
				start: function(){
					if (options.resultsDraggable.onFeaturesDragStart)
						options.resultsDraggable.onFeaturesDragStart(features);
				},
				stop: function(){
					if (options.resultsDraggable.onFeaturesDragStop)
						options.resultsDraggable.onFeaturesDragStop(features);
				},
			}, options.resultsDraggable);
			data.$results.find('.ui-draggable').draggable('destroy');
			data.$results.find('.ui-selected').draggable(draggableOptions);
		}
	}
	
	function blockScrollOnBounds($div){
		$div.bind('mousewheel', function(e){
			var delta=e.originalEvent.wheelDeltaY, $div = $(this), scrollTop=$div.scrollTop();
			if ((delta>0 && $div.scrollTop()==0) // scroll to top 
				|| (delta<0 && (scrollTop==$div.get(0).scrollHeight-$div.innerHeight()))) // scroll to bottom
					e.preventDefault();
		});
	}
	
	this._getSubLayersRecursive = function(layer){
		var that=this;
		if (layer.getLayers){
			var layersList = [];
			$.each(layer.getLayers(), function(){
				layersList = layersList.concat(that._getSubLayersRecursive(this));
			});
			return layersList;
		} else
			return layer; // base case
	}
	
	this._highlightFeaturesOnMap = function(features){
		var data = this.data, style=this.options.highlightedFeatureStyle;
		while(layerToRestore = data.selectedLayers.pop()){
			layerToRestore.resetStyle(layerToRestore);
			layerToRestore.isSelected = false;
		}
		
		$.each(features, function(){
			var l = data.layersMap[this.id];			
			l.setStyle(style);
			l.bringToFront();
			l.isSelected = true;
			data.selectedLayers.push(l);
		});		
	}

	this._getPagination = function(json){
		
		var data = this.data;
			$pagination = $("<div class='osn-results-pagination'>"),
			indexOffset = data.os.urls[OpensearchNavigator.RESULT_FORMAT].indexOffset;
			startIndex = parseInt(data.$osfi.getParameterValue("startIndex")),
			count = data.$osfi.getParameterValue("count");
		$pagination.pagination({
			totalResults: json.properties.totalResults,
			startIndex: startIndex,
			itemsPerPage: count,
			indexOffset: indexOffset,
			clazz: "pagination-mini",
			changePage: function(infoPage){
				data.$osfi.setParameterValue("startIndex", infoPage.startIndex, true);
				//alert(infoPage.startIndex+"\n"+infoPage.itemsPerPage);
			}
		});

		return $pagination;
	}
	
	function format() {
		var args = arguments;
		return this.replace(/{(\d+)}/g, function(match, number) { 
			return typeof args[number] != 'undefined' ? args[number] : match;
		});
	}
	
	this.getData = function(){
		return this.data;
	}
	
	this._limitatex = function(x){
		if (x<-180) return -180;
		else if  (x>180) return 180;
		else return x;
	},
	this._limitatey = function(y){
		if (y<-90) return -90;
		else if  (y>90) return 90;
		else return y;
	};

	
	
	////////////
	//  MAIN  //
	////////////
	
	// manage the options
	var that=this;
	$(this).addClass("osn-mapContainer");
	this.$map = $("<div>").addClass("osn-map").appendTo($(this));
	this.options = this._getOptions(); 
	
	if (this.options.innerShadow && !L.Browser.ie)
		$("<div>").addClass("osn-innerShadow").appendTo($(this));
		
	this.data={ };
	
	this._checkOptionsIntegrity();
	
	if (typeof(this.options.osDescription)=="string") {
		// call the opensearchSlurper to manage the o.s. description
		$(this).mask("Loading opensearch description...");
		OpensearchSlurper.parse(this.options.osDescription, function(os){
			// get ok
			$(that).unmask();
			that._initAll(os);
		}, function(){
			$(that).unmask();
			that._error("Unable to load the Opensearch Description Url: "+that.options.osDescription);
		});
	} else
		this._initAll(this.options.osDescription);
	
	//this.data.options = options;
	//window.data = data; // for debug
	
	return this;
};


})(jQuery);
